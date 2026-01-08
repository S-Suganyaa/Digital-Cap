using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IUserAccountRepository : IRepositoryBase<UserAccountModel, Guid>
    {
        Task<UserAccountModel> GetByAspNetId(string id);
    }
}
