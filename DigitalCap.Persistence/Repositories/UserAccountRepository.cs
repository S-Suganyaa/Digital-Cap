using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
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
