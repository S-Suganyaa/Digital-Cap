using DigitalCap.Core.Models.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IUserPermissionRepository
    {
        Task<PermissionViewModel> GetRolePermissionByUserName(string username, string viewname, int? projectId = null);
        Task<List<int>> GetCurrentUserProjects(string username);
    }
}
