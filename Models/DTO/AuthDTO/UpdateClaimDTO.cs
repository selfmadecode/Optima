using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.AuthDTO
{
    public class UpdateClaimDTO
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class AdminDetailsDTO
    {
        public Guid UserId { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public bool Status { get; set; }

        public DateTime DateCreated { get; set; }

        public IList<string> Permissions { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class GetAdminDetailsDTO
    {
        public string EmailAddress { get; set; }
    }
}
