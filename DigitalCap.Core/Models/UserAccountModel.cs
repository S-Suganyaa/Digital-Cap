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
        public required string AspNetUserId { get; set; }
        public Guid ClientId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Company { get; set; }
        public bool IsAgreeToPrivacy { get; set; }
        public string DisplayName => string.Join(" ", FirstName, LastName);
        public required string ProfileImageUrl { get; set; }

        public required string Title { get; set; }
        public required string Phone { get; set; }
        public string TimeZone { get; set; } = "";
    }
}
