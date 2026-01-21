using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using DigitalCap.Core.Helpers.Constants;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class UserAccountRepository :RepositoryBase<UserAccountModel, Guid>, IUserAccountRepository
    {
        public UserAccountRepository(IUnitOfWork unitOfWork)
       : base(unitOfWork)
        {
            //_logger = logger;
        }

        public async Task<UserAccountModel?> GetByAspNetIdAsync(string aspNetUserId)
        {
            return await Connection.QuerySingleOrDefaultAsync<UserAccountModel>(
                sql: "sp_UserAccounts_GetByAspNetId",
                param: new { aspNetUserId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }

        public async Task<IEnumerable<UserAccountModel>> GetByAspNetIdActiveOrDeletedAsync(string aspNetUserId)
        {
            return await Connection.QueryAsync<UserAccountModel>(
                sql: "sp_UserAccounts_GetByAspNetId_ActiveOrDeleted",
                param: new { aspNetUserId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }

        public async Task<UserAccountModel?> GetByAspNetIdIncludingDeletedAsync(string id)
        {
            return await Connection.QuerySingleOrDefaultAsync<UserAccountModel>(
                sql: "sp_UserAccounts_GetByAspNetId_ActiveOrDeleted",
                param: new { id },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }


        public async Task<IEnumerable<UserAccountModel>> GetUsersInRoleAsync(string roleName)
        {
            return await Connection.QueryAsync<UserAccountModel>(
                sql: "sp_UserAccounts_GetUsersInRole",
                param: new { roleName },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }

        public async Task<IEnumerable<UserAccountModel>> GetUsersInRoleForClientAsync(
            string roleName, Guid clientId)
        {
            return await Connection.QueryAsync<UserAccountModel>(
                sql: "sp_UserAccounts_GetUsersInRoleForClient",
                param: new { roleName, clientId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }

        public async Task<IEnumerable<CapUser>> GetAbsUsersAsync()
        {
            return await Connection.QueryAsync<CapUser>(
                sql: "sp_UserAccounts_GetAbsUsers",
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }

        public async Task<IEnumerable<CapUser>> GetClientUsersAsync(string clientId)
        {
            return await Connection.QueryAsync<CapUser>(
                sql: "sp_UserAccounts_GetClientUsers",
                param: new { clientId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }

        public async Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission)
        {
            return await Connection.QueryAsync<UserAccountModel>(
                sql: "sp_UserAccounts_GetUsersWithPermission",
                param: new { permissionName = permission.ToString() },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }



        public async Task<IEnumerable<string>> GetPermissionsForUserAsync(Guid userId)
        {
            var result = await Connection.QueryAsync<string>(
            sql: "sp_UserAccounts_GetUsersWithPermission",
            commandType: CommandType.StoredProcedure,
            param: new { userId },
            transaction: Transaction);

            return result;
        }
        public async Task<UserAccountModel> GetByAspNetId(string id)
        {
            var result = await Connection.QuerySingleOrDefaultAsync<UserAccountModel>(
                sql: "sp_UserAccounts_GetByAspNetId",
                commandType: CommandType.StoredProcedure,
                param: new { id },
                transaction: Transaction);

            return result;
        }
    }
}

