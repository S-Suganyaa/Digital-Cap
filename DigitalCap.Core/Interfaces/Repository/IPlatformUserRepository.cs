using DigitalCap.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IPlatformUserRepository
    {
        Task<IEnumerable<UserType>> GetUserType(string objectId);
    }
}
