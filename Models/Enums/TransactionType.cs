using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Description("Approved")]
        Approved = 1,
        [Description("Pending")]
        Pending,
        [Description("Declined")]
        Declined,
        [Description("Partial Approved")]
        PartialApproval
    }
}
