using DigitalCap.Core.DTO;
using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using DigitalCap.Core.ViewModels.AccountViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IUserAccountService
    {
        //Task<LoginResult> LoginAsync(LoginViewModel model);
        //Task LogoutAsync();

        //Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string code);
        //Task ForgotPasswordAsync(string email);
        //Task ResetPasswordAsync(string email, string token, string newPassword);

        //Task<ExternalLoginResult> ExternalLoginAsync(string? returnUrl, string? remoteError);

        //Task<bool> AssignRolesAndPermissionsAsync(Guid userId,IEnumerable<string> roles,IEnumerable<string> permissions);

        //Task<CreateAccountResponse> GetCreateAccountDataAsync();
        //Task<object?> GetEditAccountDataAsync();

        //Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
        //Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

        //Task<Guid> CreateAccountAsync(CreateAccountRequest request);
        //Task<Guid> EditAccountAsync(AccountViewModel model);

        ////Task<IEnumerable<ManagedUserResponse>> GetManagedUsersAsync();

        ////Task RequestAccessAsync(string email);
        ////Task ChangePasswordAsync(ChangePasswordRequest request);

        //Task<UserProfileResponse> GetProfileAsync();
        //Task UpdateProfileAsync(EditProfileRequest request);

        //// Repository passthroughs
        //Task<UserAccountModel?> GetByAspNetIdAsync(string aspNetUserId);
        //Task<UserAccountModel?> GetByAspNetIdIncludingDeletedAsync(string aspNetUserId);
        //Task<IEnumerable<UserAccountModel>> GetUsersInRoleAsync(string roleName);
        //Task<IEnumerable<UserAccountModel>> GetUsersInRoleForClientAsync(string roleName, Guid clientId);
        //Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission);
        //Task<IEnumerable<CapUser>> GetAbsUsersAsync();
        //Task<IEnumerable<CapUser>> GetClientUsersAsync(string clientId);

        //Task<UserAccountModel> SaveAsync(UserAccountModel account);
        ////Task<IEnumerable<UserAccountModel>> GetByAspNetIdActiveOrDeletedAsync(string id);
        //Task CommitAsync();
    }
}
