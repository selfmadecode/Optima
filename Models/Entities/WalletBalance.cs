using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class WalletBalance : BaseEntity
    {
        public decimal Balance { get; set; }
        public decimal Excro { get; set; }
        // Sum total of user withdrawal transaction request(s) will be held in excro
        public decimal ActualBalance { get; set; }
        public ApplicationUser User { get; set; }
        public Guid UserId { get; set; }

        public List<CreditDebit> CreditDebit { get; set; } = new List<CreditDebit>();
    }
}
