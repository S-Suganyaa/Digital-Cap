using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DigitalCap.Core.Models
{
    public class LoginData
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string ProviderKey { get; set; }
    }

    public class ApplicationDto : IUser// : IdentityUser, IUser
    {
        public static readonly ApplicationDto Anonymous = new ApplicationDto();
        [Required]
        [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Id { get; set; }
        public string Email { get; set; }

        public IReadOnlyList<string> Roles { get; set; }
        public HashSet<Permission> Permissions { get; set; }
        public UserAccountModel UserAccount { get; set; }
        public string DisplayName => UserAccount?.DisplayName ?? UserName;
        public string DisplayRole => Roles?.Any() ?? false
                                        ? string.Join(", ", Roles)
                                        : null;
        public bool IsEnabled { get; set; }
        public string UserName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool HasAllPermissions(params Permission[] permissions)
        {

            if (!(permissions?.Any() ?? false))
                return true;

            if (!(Permissions?.Any() ?? false))
                return false;

            var result = permissions.All(Permissions.Contains);

            return result;
        }

    //public class ApplicationUser : IdentityUser, IUser
    //{
    //    public static readonly ApplicationUser Anonymous = new ApplicationUser();
    //    [Required]
    //    [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
    //    public override string Email { get; set; } = default!;

    //    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
    //    public HashSet<Permission> Permissions { get; set; } = new();
    //    public UserAccountModel? UserAccount { get; set; }
    //    public string DisplayName => UserAccount?.DisplayName ?? UserName ?? Email;
    //    public string? DisplayRole => Roles?.Any() ?? false
    //                                    ? string.Join(", ", Roles)
    //                                    : null;
    //    public bool IsEnabled { get; set; } = true;

    //    public bool HasAllPermissions(params Permission[] permissions)
    //    {

    //        if (permissions == null || permissions.Length == 0)
    //            return true;

    //        if (Permissions == null || Permissions.Count == 0)
    //            return false;

    //        return permissions.All(Permissions.Contains);
    //    }

    //    public bool HasAnyPermissions(params Permission[] permissions)
    //    {

    //        if (permissions == null || permissions.Length == 0)
    //            return true;

    //        if (Permissions == null || Permissions.Count == 0)
    //            return false;

    //        return permissions.Any(Permissions.Contains);
    //    }


    //}
        public bool HasAnyPermissions(params Permission[] permissions)
        {

            if (!(permissions?.Any() ?? false))
                return true;

            if (!(Permissions?.Any() ?? false))
                return false;

            var result = permissions.Any(Permissions.Contains);

            return result;
        }
    }
}

