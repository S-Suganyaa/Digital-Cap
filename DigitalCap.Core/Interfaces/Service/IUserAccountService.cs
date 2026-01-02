using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IUserAccountService
    {
        Task<UserAccountModel> GetByAspNetIdAsync(string aspNetUserId);

        Task<UserAccountModel> GetByAspNetIdIncludingDeletedAsync(string aspNetUserId);

        Task<IEnumerable<UserAccountModel>> GetUsersInRoleAsync(string roleName);

        Task<IEnumerable<UserAccountModel>> GetUsersInRoleForClientAsync(string roleName, Guid clientId);

        Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission);

        Task<IEnumerable<CapUser>> GetAbsUsersAsync();

        Task<IEnumerable<CapUser>> GetClientUsersAsync(string clientId);

        Task<UserAccountModel> SaveAsync(UserAccountModel account);

        Task CommitAsync();
    }
}




    


