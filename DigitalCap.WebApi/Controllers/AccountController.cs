using DigitalCap.Core.DTO;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.ViewModels.AccountViewModels;
using DigitalCap.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCap.API.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IUserAccountService _userAccountService;

        public AccountController(IUserAccountService userAccountService)
        {
            _userAccountService = userAccountService;
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _userAccountService.LoginAsync(model);
            return result.Success ? Ok(result) : Unauthorized(result.Message);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userAccountService.LogoutAsync();
            return Ok();
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var result = await _userAccountService.ConfirmEmailAsync(userId, code);
            return result.Success ? Ok(result.Message) : BadRequest(result.Message);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            await _userAccountService.ForgotPasswordAsync(model.Email);
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            await _userAccountService.ResetPasswordAsync(model.Email, model.Code, model.Password);
            return Ok();
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLogin(string? returnUrl, string? remoteError)
        {
            var result = await _userAccountService.ExternalLoginAsync(returnUrl, remoteError);
            return result.Succeeded ? Ok(result) : Unauthorized(result.Message);
        }

        [HttpPost("{userId:guid}/permissions")]
        public async Task<IActionResult> AssignPermissions(
            Guid userId,
            AssignUserRolesAndPermissions model)
        {
            var success = await _userAccountService.AssignRolesAndPermissionsAsync(
                userId, model.Roles, model.Permissions);

            return success ? Ok() : StatusCode(500);
        }

        [HttpGet("{userId:guid}/permissions")]
        public async Task<IActionResult> GetPermissions(Guid userId)
            => Ok(await _userAccountService.GetUserPermissionsAsync(userId));

        [HttpGet("{userId:guid}/roles")]
        public async Task<IActionResult> GetRoles(Guid userId)
            => Ok(await _userAccountService.GetUserRolesAsync(userId));

        // ---------- ACCOUNT ----------

        [HttpGet("[action]")]
        public async Task<IActionResult> CreateAccountData()
            => Ok(await _userAccountService.GetCreateAccountDataAsync());

        //[HttpPost("create")]
        //public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        //    => Ok(await _userAccountService.GetCreateAccountDataAsync(request));

        [HttpGet("[action]")]
        public async Task<IActionResult> ManagedUsers()
            => Ok(await _userAccountService.GetManagedUsersAsync());

        // ---------- PROFILE ----------

        [HttpGet("[action]")]
        public async Task<IActionResult> Profile()
            => Ok(await _userAccountService.GetProfileAsync());

        [HttpPut("action")]
        public async Task<IActionResult> UpdateProfile(EditProfileRequest request)
        {
            await _userAccountService.UpdateProfileAsync(request);
            return Ok();
        }
    }
}
