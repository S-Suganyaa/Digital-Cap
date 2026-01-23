using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface ISecurityService
    {
       // Task<ApplicationDto?> GetUser(Guid userId);
        // Task<IEnumerable<ApplicationDto>> GetAllUsers();
        // Task<IEnumerable<ApplicationDto>> GetClientUsers();
        // Task<IEnumerable<Permission>> GetPermissionsForRole(string role);
        Task<bool> UpdateRolesForUser(Guid userId, params string[] roles);
        //  Task<bool> UpdatePermissionsForUser(Guid userId, params Permission[] permissions);


        Task<bool> UpdatePermissionsForUser(Guid userId, params Permission[] permissions);

        Task<bool> ToggleUserIsEnabled(Guid id, bool isEnabled);
        Task<ServiceResult<ApplicationDto>> GetCurrentUserAsync();
        IEnumerable<string> Roles { get; }
        //Task<bool> UpdateRolesForUser(Guid userId, params string[] roles);
    }
}
