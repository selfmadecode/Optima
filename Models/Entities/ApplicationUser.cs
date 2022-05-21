using Microsoft.AspNetCore.Identity;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class ApplicationUser: IdentityUser<Guid>
    {        
        public bool IsAccountLocked { get; set; }
        public bool HasAcceptedTerms { get; set; }
        public string FullName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastLoginDate { get; set; }
        public UserTypes UserType { get; set; }
    }

    public class ApplicationUserRole : IdentityRole<Guid>
    {
        public ApplicationUserRole() 
        {
            Id = Guid.NewGuid();
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }

    }
}
