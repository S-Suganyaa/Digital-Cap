using System;
using System.Collections.Generic;
using System.Text;
using DigitalCap.Core.Interfaces.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;

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

    public class ApplicationUser : IdentityUser, IUser
    {
    }
}
