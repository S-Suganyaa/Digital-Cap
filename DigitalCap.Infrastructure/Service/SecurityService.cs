using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using DigitalCap.Core.Security.Extensions;
using DigitalCap.WebApi.Core;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class SecurityService : ISecurityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAccountRepository _userAccountRepository;

        public SecurityService(
        UserManager<ApplicationUser> userManager, RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager
            , IHttpContextAccessor httpContextAccessor, IUserAccountRepository userAccountRepository
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _userAccountRepository = userAccountRepository;
        }
        public async Task<ServiceResult<ApplicationDto>> GetCurrentUserAsync()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;

            if (claimsPrincipal == null)
                return ServiceResult<ApplicationDto>.Failure("No user context");

            var user = await _userManager.GetUserAsync(claimsPrincipal);
            if (user == null)
                return ServiceResult<ApplicationDto>.Failure("User not found");

            await MapUserDetails(user);

            var dto = new ApplicationDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                IsEnabled = user.IsEnabled,

                Roles = user.Roles ?? Array.Empty<string>(),
                Permissions = user.Permissions?.ToArray() ?? Array.Empty<Permission>(),

                UserAccount = user.UserAccount
            };


            return ServiceResult<ApplicationDto>.Success(dto);
        }

        private static class ErrorCodes
        {
            public const string UserAlreadyInRole = nameof(UserAlreadyInRole);
        }
        private async Task MapUserDetails(ApplicationUser user)
        {
            await MapIsEnabled(user);
            await MapRolesAndClaimsToUser(user);
            await MapUserAccountToUser(user);
        }


        private async Task MapIsEnabled(ApplicationUser user)
        {
            if (user == null)
                return;

            var lockoutEndDate = await _userManager.GetLockoutEndDateAsync(user);
            user.IsEnabled = !lockoutEndDate.HasValue;
        }

        private async Task MapUserAccountToUser(ApplicationUser user)
        {
            if (user == null)
                return;

            user.UserAccount = await _userAccountRepository.GetByAspNetId(user.Id);
        }

        private async Task MapRolesAndClaimsToUser(ApplicationUser user)
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
        public IEnumerable<string> Roles
    => _roleManager.Roles.Select(r => r.Name).ToList();
        public async Task<bool> UpdateRolesForUser(Guid userId, params string[] roles)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return false;

            return await UpdateRolesForIdentityUser(user, roles);
        }

        private async Task<bool> UpdateRolesForIdentityUser(ApplicationUser user, params string[] roles)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, roles);

            return true;
        }

        public async Task<ApplicationUser> GetUser(Guid userId)
        {
            var result = await _userManager.FindByIdAsync(userId.ToString());

            await MapUserDetails(result);

            return result;
        }
        public async Task<bool> UpdateRolesForUser(string userId, params string[] roles)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Get existing roles
            var existingRoles = await _userManager.GetRolesAsync(user);

            // Remove existing roles
            if (existingRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
                if (!removeResult.Succeeded)
                    return false;
            }

            // If no new roles, we are done
            if (!(roles?.Any() ?? false))
                return true;

            // Validate roles against RoleManager
            var allValidRoles = await _roleManager.Roles
                .Select(r => r.Name)
                .ToListAsync();

            var validRoles = roles
                .Where(r => allValidRoles.Contains(r))
                .Distinct()
                .ToArray();

            if (!validRoles.Any())
                return true;

            // Add new roles
            var addResult = await _userManager.AddToRolesAsync(user, validRoles);

            return addResult.Succeeded
                || addResult.Errors.All(e => e.Code == ErrorCodes.UserAlreadyInRole);
        }

        public async Task<bool> UpdatePermissionsForUser(Guid userId, params Permission[] permissions)
        {
            var user = await GetUser(userId);
            if (user == null)
                return false;

            return await UpdatePermissionsInternal(user, permissions);
        }

        private async Task<bool> UpdatePermissionsInternal(ApplicationUser user, params Permission[] permissions)
        {
            var existingClaims = await _userManager.GetClaimsAsync(user);

            if (existingClaims.Any())
            {
                var removeResult = await _userManager.RemoveClaimsAsync(user, existingClaims);
                if (!removeResult.Succeeded)
                    return false;
            }

            if (!(permissions?.Any() ?? false))
                return true;

            var newClaims = permissions.Select(p => p.ToClaim());
            var addResult = await _userManager.AddClaimsAsync(user, newClaims);

            return addResult.Succeeded;
        }

        public async Task<bool> ToggleUserIsEnabled(Guid id, bool isEnabled)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return false;

            var lockoutEndDate = isEnabled
                                    ? (DateTimeOffset?)null
                                    : DateTimeOffset.MaxValue;
            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEndDate);
            return result.Succeeded;
        }
    }
}
