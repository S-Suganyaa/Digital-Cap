using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class UserAccountRepository : RepositoryBase, IUserAccountRepository
    {
        public UserAccountRepository(IDbConnection connection, IDbTransaction transaction)
            : base(connection, transaction) { }

        // -------------------- Queries --------------------

        public async Task<UserAccountModel> GetByAspNetIdAsync(string aspNetUserId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", aspNetUserId);

                return await Connection.QuerySingleOrDefaultAsync<UserAccountModel>(
                    sql: Constants.StoredProcedures.UserAccounts.GetByAspNetId,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UserAccountModel> GetByAspNetIdIncludingDeletedAsync(string aspNetUserId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", aspNetUserId);

                return await Connection.QuerySingleOrDefaultAsync<UserAccountModel>(
                    sql: Constants.StoredProcedures.UserAccounts.GetByAspNetId_ActiveOrDeleted,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<UserAccountModel>> GetUsersInRoleAsync(string roleName)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@roleName", roleName);

                return (await Connection.QueryAsync<UserAccountModel>(
                    sql: Constants.StoredProcedures.UserAccounts.GetUsersInRole,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<UserAccountModel>> GetUsersInRoleForClientAsync(string roleName, Guid clientId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@roleName", roleName);
                parameters.Add("@clientId", clientId);

                return (await Connection.QueryAsync<UserAccountModel>(
                    sql: Constants.StoredProcedures.UserAccounts.GetUsersInRoleForClient,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@permissionName", permission.ToString());

                return (await Connection.QueryAsync<UserAccountModel>(
                    sql: Constants.StoredProcedures.UserAccounts.GetUsersWithPermission,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<CapUser>> GetAbsUsersAsync()
        {
            try
            {
                return (await Connection.QueryAsync<CapUser>(
                    sql: Constants.StoredProcedures.UserAccounts.GetAbsUsers,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<CapUser>> GetClientUsersAsync(string clientId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@clientId", clientId);

                return (await Connection.QueryAsync<CapUser>(
                    sql: Constants.StoredProcedures.UserAccounts.GetClientUsers,
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // -------------------- Commands --------------------

        public async Task<bool> InsertAsync(UserAccountModel entity)
        {
            try
            {
                await Connection.ExecuteAsync(
                    sql: Constants.StoredProcedures.UserAccounts.Insert,
                    param: entity,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(UserAccountModel entity)
        {
            try
            {
                await Connection.ExecuteAsync(
                    sql: Constants.StoredProcedures.UserAccounts.Update,
                    param: entity,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
