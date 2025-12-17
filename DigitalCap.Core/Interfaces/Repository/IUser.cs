using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IUser
    {
        string UserName { get; set; }
        string Email { get; set; }
        bool IsEnabled { get; set; }
        IReadOnlyList<string> Roles { get; set; }
        HashSet<Permission> Permissions { get; set; }
        bool HasAllPermissions(params Permission[] permissions);
        bool HasAnyPermissions(params Permission[] permissions);
        UserAccountModel UserAccount { get; set; }
    }
}
