using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Security.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Security
{
    public class PermissionAuthorizationFilter : IAuthorizationFilter
    {
        private readonly IUser _user;

        public Permission[] Permissions { get; set; }
        public bool AreAllPermissionsRequired { get; set; }

        public PermissionAuthorizationFilter(IUser user)
        {
            _user = user;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            _user.Email = _user.UserName = context.HttpContext.User.Identity.Name;
            var distinctPermissions = context.HttpContext.User.Claims.ToPermissions();
            _user.Permissions = new HashSet<Permission>(distinctPermissions);

            if (_user.Permissions.Contains(Permission.SystemAdministrator))
            {
                return;
            }

            var hasRequiredPermissions = (AreAllPermissionsRequired
                                            ? _user?.HasAllPermissions(Permissions)
                                            : _user?.HasAnyPermissions(Permissions))
                                            ?? false;
            if (!hasRequiredPermissions)
                context.Result = new ForbidResult();
        }
    }

}
