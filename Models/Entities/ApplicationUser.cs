using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class ApplicationUser: IdentityUser<Guid>
    {
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
