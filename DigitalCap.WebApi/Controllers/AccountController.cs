using DigitalCap.Core.DTO;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.View.Admin;
using DigitalCap.Core.ViewModels.AccountViewModels;
using DigitalCap.Infrastructure.Service;
using DigitalCap.WebApi.Core;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

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
            return Ok(result);
        }
        [HttpGet("CorpLogin")]
        [AllowAnonymous]
        public IActionResult CorpLogin([FromQuery] string returnUrl = "")
        {
            var redirectUrl = string.IsNullOrEmpty(returnUrl) ? "/app" : returnUrl;
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            return Challenge(properties, "AzureAD");
        }

        [HttpGet("azure-callback")]
        public IActionResult AzureCallback(string returnUrl = "/")
        {
            return Redirect(returnUrl);
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
            return Ok(result);
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
            return Ok(result);
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

        [HttpPut("account")]
        public async Task<IActionResult> EditAccount([FromBody] AccountViewModel accountVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var account = accountVM.UserAccount;

            var result = _userAccountService.EditAccount(accountVM);


            return Ok(result);
        }

        [HttpPost("RequestAccess")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestAccess([FromBody] RequestAccessDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new
                {
                    message = "Email is required",
                    error = "EMAIL_REQUIRED"
                });
            }

            try
            {
                await _userAccountService.RequestAccessAsync(dto.Email.Trim());
                return Ok(new
                {
                    message = "Access request sent successfully",
                    success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while processing your request",
                    error = ex.Message
                });
            }
        }

        //[HttpPost("create")]
        //public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        //    => Ok(await _userAccountService.GetCreateAccountDataAsync(request));

        //[HttpGet("[action]")]
        //public async Task<IActionResult> ManagedUsers()
        //    => Ok(await _userAccountService.GetManagedUsersAsync());

        // ---------- PROFILE ----------

        //[HttpGet("[action]")]
        //public async Task<IActionResult> Profile()
        //    => Ok(await _userAccountService.GetProfileAsync());

        //[HttpPut("action")]
        //public async Task<IActionResult> UpdateProfile(EditProfileRequest request)
        //{
        //    await _userAccountService.UpdateProfileAsync(request);
        //    return Ok();
        //}
    }
}
