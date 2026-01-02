using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface ISecurityService
    {
        Task<ApplicationUser?> GetCurrentUserAsync();
        Task<ApplicationUser?> GetUserAsync(Guid userId);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<IEnumerable<ApplicationUser>> GetClientUsersAsync();
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string role);
        Task<bool> UpdateRolesForUserAsync(Guid userId, params string[] roles);
        Task<bool> UpdatePermissionsForUserAsync(Guid userId, params Permission[] permissions);
        Task<bool> ToggleUserIsEnabledAsync(Guid id, bool isEnabled);
    }
}
