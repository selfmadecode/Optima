using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Enums
{
    public enum TransactionType
    {
        Credit = 1,
        Debit
    }
    public enum TransactionStatus
    {
        Approved = 1,
        Pending,
        Declined,
        PartialApproval
    }
}
