using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class AccountBalance : BaseEntity
    {
        public decimal Balance { get; set; }
        public decimal Excro { get; set; } // User withdrawal transaction request will be held in excro
        public decimal ActualBalance { get; set; }
        public ApplicationUser User { get; set; }
        public Guid UserId { get; set; }
    }
}
