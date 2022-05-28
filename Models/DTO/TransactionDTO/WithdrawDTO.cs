using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.TransactionDTO
{
    public class WithdrawDTO
    {
        public Guid UserSelectedBankAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
