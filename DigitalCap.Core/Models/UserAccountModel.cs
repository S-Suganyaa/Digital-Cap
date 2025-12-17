using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DigitalCap.Core.Models
{

    [Table("UserAccounts")]
    public class UserAccountModel : AuditableEntity<Guid>
    {
        public string AspNetUserId { get; set; }
        public Guid ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public bool IsAgreeToPrivacy { get; set; }
        public string DisplayName => string.Join(" ", FirstName, LastName);
        public string ProfileImageUrl { get; set; }

        public string Title { get; set; }
        public string Phone { get; set; }
        public string TimeZone { get; set; } = "";
    }
}
