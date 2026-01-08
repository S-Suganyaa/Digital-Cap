using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using DigitalCap.Core.Security.Extensions;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class SecurityService : ISecurityService
    {
        private readonly UserManager<ApplicationDto> _userManager;
        private readonly RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAccountRepository _userAccountRepository;


        public async Task<ServiceResult<ApplicationDto>> GetCurrentUser()
        {
            var context = _httpContextAccessor.HttpContext;
            var claimsPrincipal = context?.User;
            var result = claimsPrincipal != null
                            ? await _userManager.GetUserAsync(claimsPrincipal)
                            : null;

            await MapUserDetails(result);

            return ServiceResult<ApplicationDto>.Success(result); 
        }

        private async Task MapUserDetails(ApplicationDto user)
        {
            await MapIsEnabled(user);
            await MapRolesAndClaimsToUser(user);
            await MapUserAccountToUser(user);
        }


        private async Task MapIsEnabled(ApplicationDto user)
        {
            if (user == null)
                return;

            var lockoutEndDate = await _userManager.GetLockoutEndDateAsync(user);
            user.IsEnabled = !lockoutEndDate.HasValue;
        }

        private async Task MapUserAccountToUser(ApplicationDto user)
        {
            if (user == null)
                return;

            user.UserAccount = await _userAccountRepository.GetByAspNetId(user.Id);
        }

        private async Task MapRolesAndClaimsToUser(ApplicationDto user)
        {
            if (user == null)
                return;

            user.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            var userRoles = _roleManager.Roles.Where(x => user.Roles.Contains(x.Name)).ToList();
            var permissions = new List<Permission>();

            foreach (var currentRole in userRoles)
            {
                var currentClaims = await _roleManager.GetClaimsAsync(currentRole);
                var currentPermissions = currentClaims.ToPermissions();
                permissions.AddRange(currentPermissions);
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userPermissions = userClaims.ToPermissions();
            permissions.AddRange(userPermissions);
            var distinctPermissions = permissions.Distinct().ToList();

            user.Permissions = new HashSet<Permission>(distinctPermissions);
        }

    }
}
