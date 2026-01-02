using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using DigitalCap.Core.Interfaces;
using DigitalCap.Core.Security;


namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IUserAccountRepository
    {
        Task<UserAccountModel> GetByAspNetIdAsync(string aspNetUserId);

        Task<UserAccountModel> GetByAspNetIdIncludingDeletedAsync(string aspNetUserId);

        Task<IEnumerable<UserAccountModel>> GetUsersInRoleAsync(string roleName);

        Task<IEnumerable<UserAccountModel>> GetUsersInRoleForClientAsync(string roleName, Guid clientId);

        Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission);

        Task<IEnumerable<CapUser>> GetAbsUsersAsync();

        Task<IEnumerable<CapUser>> GetClientUsersAsync(string clientId);

        Task InsertAsync(UserAccountModel entity);

        Task UpdateAsync(UserAccountModel entity);

        Task CommitAsync();
        Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission);
    }
}
