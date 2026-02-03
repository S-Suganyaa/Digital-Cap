using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models.Permissions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class UserPermissionRepository : IUserPermissionRepository
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;
        public UserPermissionRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }
        public void Commit()
        {
            _unitOfWork?.Commit();
        }
        public async Task<PermissionViewModel> GetRolePermissionByUserName(string username, string viewname, int? projectId)
        {
            try
            {
                return await Connection.QueryFirstOrDefaultAsync<PermissionViewModel>(
                    sql: "dbo.GetCurrentUserPermission",
                    param: new
                    {
                        username,
                        viewname,
                        projectId
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );
            }
            catch
            {
                throw;
            }
        }




        public async Task<List<int>> GetCurrentUserProjects(string username)
        {
            var result = await Connection.QueryAsync<int>(
                sql: "dbo.GetCurrentUserProjects",
                param: new { username },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );

            return result.ToList();
        }

    }
}
