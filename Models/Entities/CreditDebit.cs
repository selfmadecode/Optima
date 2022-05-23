﻿using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CreditDebit : BaseEntity
    {
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public Guid? ActionedBy { get; set; } // Admin that acted on the transaction
        public AccountBalance AccountBalance { get; set; }
        public Guid AccountBalanceId { get; set; }
    }
}
