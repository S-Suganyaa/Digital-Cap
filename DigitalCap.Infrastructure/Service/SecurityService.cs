using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Security;
using DigitalCap.Core.Security.Extensions;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace DigitalCAP.Core.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAccountRepository _userAccountRepository;

        public SecurityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor httpContextAccessor,
            IUserAccountRepository userAccountRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _userAccountRepository = userAccountRepository;
        }

       // User Access
        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
            return await MapUserDetailsAsync(user);
        }

        public async Task<ApplicationUser?> GetUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return await MapUserDetailsAsync(user);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();

            foreach (var user in users)
                await MapUserDetailsAsync(user);

            return users;
        }

        public async Task<IEnumerable<ApplicationUser>> GetClientUsersAsync()
        {
            var currentUser = await GetCurrentUserAsync();
            var clientId = currentUser?.UserAccount?.ClientId;

            var users = await GetAllUsersAsync();
            return users.Where(x => x.UserAccount?.ClientId == clientId);
        }

        public async Task<IEnumerable<string>> GetPermissionsForUserAsync(Guid userId)
        {
            var permissions = await _userAccountRepository
            .GetUserPermissionsAsync(userId);

            return permissions ?? Enumerable.Empty<string>();
        }


        // USER MAPPING HELPERS

        private async Task<ApplicationUser?> MapUserDetailsAsync(ApplicationUser? user)
        {
            if (user == null)
                return null;

            await MapIsEnabledAsync(user);
            await MapRolesAndPermissionsAsync(user);
            await MapUserAccountAsync(user);

            return user;
        }

        private async Task MapIsEnabledAsync(ApplicationUser user)
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            user.IsEnabled = !lockoutEnd.HasValue;
        }

        private async Task MapUserAccountAsync(ApplicationUser user)
        {
            user.UserAccount = await _userAccountRepository.GetByAspNetIdAsync(user.Id);
        }

        private async Task MapRolesAndPermissionsAsync(ApplicationUser user)
        {
            user.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            var permissions = new List<Permission>();

            foreach (var role in user.Roles)
            {
                var identityRole = await _roleManager.FindByNameAsync(role);
                if (identityRole == null) continue;

                var claims = await _roleManager.GetClaimsAsync(identityRole);
                permissions.AddRange(claims.ToPermissions());
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            permissions.AddRange(userClaims.ToPermissions());

            user.Permissions = permissions.Distinct().ToHashSet();
        }

        // ROLES & PERMISSIONS
        
        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string role)
        {
            var roleObj = await _roleManager.FindByNameAsync(role);
            if (roleObj == null) return Enumerable.Empty<Permission>();

            var claims = await _roleManager.GetClaimsAsync(roleObj);
            return claims.ToPermissions();
        }

        public async Task<bool> UpdateRolesForUserAsync(Guid userId, params string[] roles)
        {
            var user = await GetUserAsync(userId);
            if (user == null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            return (await _userManager.AddToRolesAsync(user, roles)).Succeeded;
        }

        public async Task<bool> UpdatePermissionsForUserAsync(Guid userId, params Permission[] permissions)
        {
            var user = await GetUserAsync(userId);
            if (user == null) return false;

            var existingClaims = await _userManager.GetClaimsAsync(user);
            await _userManager.RemoveClaimsAsync(user, existingClaims);

            var newClaims = permissions.Select(p => p.ToClaim());
            return (await _userManager.AddClaimsAsync(user, newClaims)).Succeeded;
        }

        public async Task<bool> ToggleUserIsEnabledAsync(string userId, bool isEnabled)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var lockoutEnd = isEnabled? (DateTimeOffset?)null: DateTimeOffset.MaxValue;

            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            return result.Succeeded;
        }



        //public async Task<bool> ToggleUserIsEnabledAsync(Guid id, bool isEnabled)
        //{
        //    var user = await _userManager.FindByIdAsync(id.ToString());
        //    if (user == null) return false;

        //    var lockDate = isEnabled ? null : DateTimeOffset.MaxValue;
        //    return (await _userManager.SetLockoutEndDateAsync(user, lockDate)).Succeeded;
        //}
    }
}
