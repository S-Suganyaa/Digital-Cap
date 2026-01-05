using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace DigitalCap.Persistence.Repositories
{
    public class PlatformUserRepository :  IPlatformUserRepository
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserManager<ApplicationUser> _userManager;
        public async Task<string> GetLoggedInUserName()
        {
            ClaimsPrincipal principal = _httpContext.HttpContext.User as ClaimsPrincipal;
            var userId = _userManager.GetUserId(principal);
            if (userId == null) return string.Empty;
            var user = await _userManager.FindByIdAsync(userId);
            return user.UserName;
        }

        public async Task<IEnumerable<UserType>> GetUserType(string objectId)
        {
            ClaimsPrincipal principal = _httpContext.HttpContext.User as ClaimsPrincipal;
            var userId = _userManager.GetUserId(principal);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<UserType>();
            }
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var ret = roles.ConvertAll(x =>
            {
                return (UserType)Enum.Parse(typeof(UserType), x.ToUpper());
            });
            return ret;
        }
    }
}
