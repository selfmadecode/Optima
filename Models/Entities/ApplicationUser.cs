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
        public string ProfilePicture { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastLoginDate { get; set; }
        public UserTypes UserType { get; set; }
    }


    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() 
        {
            Id = Guid.NewGuid();
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }

    }
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    {
    }
    public class ApplicationUserClaim : IdentityUserClaim<Guid>
    {
    }
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {

    }
}
