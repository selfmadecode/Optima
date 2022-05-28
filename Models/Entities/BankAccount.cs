using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class BankAccount : BaseEntity
    {
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public virtual ApplicationUser User { get; set; }
        public Guid UserId { get; set; }
    }
}
