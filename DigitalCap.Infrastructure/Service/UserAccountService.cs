using DigitalCap.Core.DTO;
using DigitalCap.Core.Helpers.Constants;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using DigitalCap.Core.Security.Extensions;
using DigitalCap.Core.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;



namespace DigitalCap.Infrastructure.Service
{
    public class UserAccountService : IUserAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ISecurityService _securityService;
        private readonly ISecurityClientService _securityClientService;
        private readonly ISecurityClientRepository _securityClientRepository;
        private readonly IPlatformUserService _platformUserService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserAccountService> _logger;

        public UserAccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserAccountRepository userAccountRepository,
            ISecurityService securityService,
            ISecurityClientService securityClientService,
            ISecurityClientRepository securityClientRepository,
            IPlatformUserService platformUserService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<UserAccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userAccountRepository = userAccountRepository;
            _securityService = securityService;
            _securityClientService = securityClientService;
            _securityClientRepository = securityClientRepository;
            _platformUserService = platformUserService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        // ---------------- AUTH ----------------

        public async Task<LoginResult> LoginAsync(LoginViewModel model)
        {
            if (model == null)
                return new LoginResult { Success = false, Message = "Invalid request" };

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new LoginResult { Success = false, Message = "Invalid login attempt" };

            var account = await _userAccountRepository.GetByAspNetIdAsync(user.Id);
            if (account == null || account.Deleted)
                return new LoginResult { Success = false, Message = "Invalid login attempt" };

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in");
                return new LoginResult { Success = true, Message = "Login successful" };
            }

            if (result.IsLockedOut)
                return new LoginResult { Success = false, Message = "User account locked" };

            return new LoginResult { Success = false, Message = "Invalid login attempt" };
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
        }


        //  EMAIL / PASSWORD 

        public async Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "User not found");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return result.Succeeded
                ? (true, "Email confirmed")
                : (false, "Email confirmation failed");
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var url =
                $"{_configuration["Frontend:ResetPasswordUrl"]}" +
                $"?email={Uri.EscapeDataString(email)}" +
                $"&token={Uri.EscapeDataString(token)}";

            await _emailService.SendEmailAsync(
                email,
                "Reset Password",
                $"Reset your password using this link:\n{url}");
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("Invalid reset request");

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(" | ", result.Errors.Select(e => e.Description)));
        }


        public async Task<ExternalLoginResult> ExternalLoginAsync(string? returnUrl,string? remoteError)
        {
            // 1️⃣ External provider error
            if (!string.IsNullOrWhiteSpace(remoteError))
            {
                return ExternalLoginResult.Fail(remoteError);
            }

            // 2️⃣ Read external login info
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return ExternalLoginResult.Fail("External login information not found");
            }

            // 3️⃣ Find user by external login
            var user = await _userManager.FindByLoginAsync(info.LoginProvider,info.ProviderKey);

            // 4️⃣ First-time external login
            if (user == null)
            {
                user = await HandleFirstTimeExternalLogin(info);
                if (user == null)
                {
                    return ExternalLoginResult.Fail("User registration failed");
                }
            }

            // 5️⃣ Validate internal account
            await ValidateUserAccount(user.Id);

            // 6️⃣ Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

             return ExternalLoginResult.Ok(returnUrl);
        }

        private async Task<ApplicationUser?> HandleFirstTimeExternalLoginAsync(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            var result = await _userManager.AddLoginAsync(user,new UserLoginInfo(
            info.LoginProvider,info.ProviderKey,info.ProviderDisplayName));

            if (!result.Succeeded)
                return null;

            await _platformUserService.InitialLoginActions(email);
            await NotifyAdmins(email);

            return user;
        }

        private async Task<ApplicationUser> HandleFirstTimeExternalLogin(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("Email not provided");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("User not registered");

            var result = await _userManager.AddLoginAsync(user,
                new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));

            if (!result.Succeeded)
                throw new InvalidOperationException("External login registration failed");

            await _platformUserService.InitialLoginActions(email);
            await NotifyAdmins(email);

            return user;
        }

        private async Task ValidateUserAccount(string aspNetUserId)
        {
            var account = await _userAccountRepository.GetByAspNetIdAsync(aspNetUserId);
            if (account == null || account.Deleted)
                throw new UnauthorizedAccessException("Invalid account");
        }

        private async Task NotifyAdmins(string email)
        {
            var admins = _configuration["EmailAbsAdmin"]?.Split(',');
            if (admins == null) return;

            foreach (var admin in admins)
            {
                await _emailService.SendEmailAsync(
                    admin,
                    "New User Registered",
                    $"{email} has registered and needs role assignment.");
            }
        }

        //  ROLES & PERMISSIONS 

        public async Task<bool> AssignRolesAndPermissionsAsync(
            Guid userId,
            IEnumerable<string> roles,
            IEnumerable<string> permissions)
        {
            if (!await _securityService.UpdateRolesForUserAsync(userId, roles))
                return false;

            return await _securityService.UpdatePermissionsForUserAsync(
                userId,
                permissions?.ToPermissions().ToArray());
        }

        //ACCOUNT

        public async Task<CreateAccountResponse> GetCreateAccountDataAsync()
        {
            return new CreateAccountResponse
            {
                Clients = await _securityClientService.GetAllClientsAsync()
            };
        }


        public async Task<object?> GetEditAccountDataAsync()
        {
            var user = await _securityService.GetCurrentUserAsync();
            if (user == null) return null;

            return new
            {
                aspNetUser = user,
                userAccount = user.UserAccount,
                clients = _securityClientRepository.GetAll().ToList()
            };
        }


        public async Task<Guid> CreateAccountAsync(CreateAccountRequest request)
        {
            var user = request.AspNetUser;
            user.UserName = user.Email;
            user.EmailConfirmed = true;

            await _userManager.CreateAsync(user, request.Password);

            request.UserAccount.AspNetUserId = user.Id;
            _userAccountRepository.Insert(request.UserAccount);
            _userAccountRepository.Commit();

            return Guid.Parse(user.Id);
        }


        public async Task<Guid> EditAccountAsync(AccountViewModel model)
        {
            if (model?.AspNetUser == null || model.UserAccount == null)
                throw new ArgumentNullException(nameof(model));

            var result = await _userManager.UpdateAsync(model.AspNetUser);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(" | ", result.Errors.Select(e => e.Description)));

            await SaveAsync(model.UserAccount);
            await CommitAsync();
            return Guid.Parse(model.UserAccount.AspNetUserId);
        }


        //  PROFILE 
        public async Task<UserProfileResponse> GetProfileAsync()
        {
            var user = await _securityService.GetCurrentUserAsync();
            var account = await _userAccountRepository.GetByAspNetIdAsync(user.Id);

            return new UserProfileResponse
            {
                User = user,
                Account = account
            };
        }

        public async Task UpdateProfileAsync(EditProfileRequest request)
        {
            await _userManager.UpdateAsync(request.AspNetUser);
            await SaveAsync(request.UserAccount);
            await CommitAsync();
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Enumerable.Empty<string>();

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            return await _userAccountRepository.GetPermissionsForUserAsync(userId);
        }

        public Task<UserAccountModel?> GetByAspNetIdAsync(string aspNetUserId)
        {
            return _userAccountRepository.GetByAspNetIdAsync(aspNetUserId);
        }

        public Task<UserAccountModel?> GetByAspNetIdIncludingDeletedAsync(string aspNetUserId)
            => _userAccountRepository.GetByAspNetIdIncludingDeletedAsync(aspNetUserId);

        public Task<IEnumerable<UserAccountModel>> GetUsersInRoleAsync(string roleName)
            => _userAccountRepository.GetUsersInRoleAsync(roleName);

        public Task<IEnumerable<UserAccountModel>> GetUsersInRoleForClientAsync(string roleName, Guid clientId)
            => _userAccountRepository.GetUsersInRoleForClientAsync(roleName, clientId);

        public Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission)
            => _userAccountRepository.GetUsersWithPermissionAsync(permission);

        public Task<IEnumerable<CapUser>> GetAbsUsersAsync()
            => _userAccountRepository.GetAbsUsersAsync();

        public Task<IEnumerable<CapUser>> GetClientUsersAsync(string clientId)
            => _userAccountRepository.GetClientUsersAsync(clientId);

        public async Task<UserAccountModel> SaveAsync(UserAccountModel account)
        {
            var existingList = await _userAccountRepository.GetByAspNetIdActiveOrDeletedAsync(account.AspNetUserId);

            var existing = existingList.FirstOrDefault();

            if (existing == null)
            {
                _userAccountRepository.Insert(account);
            }
            else
            {
                account.Id = existing.Id;
                _userAccountRepository.Update(account);
            }

            return account;
        }

       

        public async Task CommitAsync()
        {
            await Task.Run(() => _userAccountRepository.Commit());
        }
    }
}

