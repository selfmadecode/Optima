using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CreditDebit : BaseEntity
    {
        // used to store all credit and debit
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public Guid? ActionedBy { get; set; } // Admin that acted on the transaction

        public WalletBalance WalletBalance { get; set; }
        public Guid WalletBalanceId { get; set; }

        public BankAccount BankAccount { get; set; }
        public Guid BankAccountId { get; set; }
    }
}
