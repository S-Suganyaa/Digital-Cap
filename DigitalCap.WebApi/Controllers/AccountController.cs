using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.ViewModels.AccountViewModels;
using DigitalCap.Infrastructure.Service;
using DigitalCap.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
//using System.Web.Mvc;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISecurityService _securityService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserAccountService _userAccountService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserAccountService userAccountService,
            ISecurityService securityService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _userAccountService = userAccountService;
            _userManager = userManager;
            _signInManager = signInManager;
            _securityService = securityService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid login attempt");

            // 🔥 Use service here instead of repository
            var account = await _userAccountService.GetByAspNetIdAsync(user.Id);
            if (account == null || account.Deleted)
                return Unauthorized("Invalid login attempt");

            var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in");
                return Ok(new { message = "Login successful" });
            }

            if (result.IsLockedOut)
                return Unauthorized("User account locked");

            return Unauthorized("Invalid login attempt");
        }

        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
                return BadRequest("Invalid confirmation request.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var result = await _userManager.ConfirmEmailAsync(user, code);

            return result.Succeeded
                ? Ok(new { message = "Email confirmed successfully." })
                : BadRequest("Email confirmation failed.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(); // do not reveal user existence

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = $"{_configuration["Frontend:ResetPasswordUrl"]}?token={token}&email={model.Email}";

            await _emailService.SendEmailAsync(
                model.Email,
                "Reset Password",
                $"Reset your password: {resetUrl}");

            return Ok(new { message = "Password reset email sent" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid request");

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok(new { message = "Password reset successful" });
        }


        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return Forbid();
        }


        [HttpGet("corp-login")]
        //[AllowAnonymous]
        public IActionResult CorpLogin([FromQuery] string returnUrl = "")
        {
            var redirectUrl = Url.Action(
        nameof(LogInExternal),
        "Account",
        new { returnUrl },
        Request.Scheme);

            return this.ChallengeAzureAD(_signInManager, redirectUrl);
        }


        [HttpGet("external-login-callback")]
        public async Task<IActionResult> LogInExternal([FromQuery] string returnUrl = null,
           [FromQuery] string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                _logger.LogWarning($"External login error: {remoteError}");
                return Unauthorized(new { message = remoteError });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Unauthorized(new { message = "External login information not found." });
            }

            var loginInfo = await _userManager.FindByLoginAsync(
        info.LoginProvider,
        info.ProviderKey);

            UserAccount userAccountInfo = null;

            if (loginInfo != null)
            {
                userAccountInfo = await _userAccountService.GetByAspNetIdAsync(loginInfo.Id);
                if (userAccountInfo == null || userAccountInfo.Deleted)
                {
                    return Unauthorized(new { message = "Invalid login attempt." });
                }
            }

            // First-time external login
            if (loginInfo == null)
            {
                var identity = info.Principal.Identity as ClaimsIdentity;
                var email = identity?.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { message = "Email not found from provider." });

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return Forbid(new { message = "User not registered." });

                userAccountInfo = await _userAccountService.GetByAspNetIdAsync(user.Id);
                if (userAccountInfo == null || userAccountInfo.Deleted)
                {
                    return Unauthorized(new { message = "Invalid login attempt." });
                }

                var addLoginResult = await _userManager.AddLoginAsync(
            user,
            new UserLoginInfo(
                info.LoginProvider,
                info.ProviderKey,
                info.Principal.Identity.Name));

                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError(
                        $"External login registration failed: {string.Join(" | ", addLoginResult.Errors.Select(e => e.Description))}");

                    return StatusCode(500, "External login registration failed.");
                }

                loginInfo = await _userManager.FindByLoginAsync(
                    info.LoginProvider,
                    info.ProviderKey);

                await _platformUserService.InitialLoginActions(email);

                var adminEmails = _configuration["EmailAbsAdmin"]?.Split(',');
                if (adminEmails != null)
                {
                    foreach (var admin in adminEmails)
                    {
                        await _emailService.SendEmailAsync(
                            admin,
                            "New User - Digital Platform",
                            $"{email} has registered for the digital platform.");
                    }
                }
            }

            await _signInManager.SignInAsync(loginInfo, isPersistent: false);

            return Ok(new
            {
                message = "External login successful",
                returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl
            });
        }



    }
}
