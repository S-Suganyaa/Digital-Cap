using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IUserAccountRepository
        : IRepositoryBase<UserAccountModel, Guid>
    {
        // Single user lookups
        Task<UserAccountModel> GetByAspNetIdAsync(string aspNetUserId);
        Task<IEnumerable<UserAccountModel>> GetByAspNetIdActiveOrDeletedAsync(string aspNetUserId);

        Task<UserAccountModel> GetByAspNetIdIncludingDeletedAsync(string id);

        

        // Role-based queries
        Task<IEnumerable<UserAccountModel>> GetUsersInRoleAsync(string roleName);

        Task<IEnumerable<UserAccountModel>> GetUsersInRoleForClientAsync(string roleName,Guid clientId);

        // Permission-based queries
        Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission);

        Task<IEnumerable<string>> GetPermissionsForUserAsync(Guid userId);

        // ABS / Client users
        Task<IEnumerable<CapUser>> GetAbsUsersAsync();

        Task<IEnumerable<CapUser>> GetClientUsersAsync(string clientId);
    }
}



