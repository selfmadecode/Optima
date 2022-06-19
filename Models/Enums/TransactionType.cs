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
        [Description("Pending")]
        Pending = 1,
        [Description("Declined")]
        Declined,
        [Description("Partial Approval")]
        PartialApproval,
        [Description("Approved")]
        Approved
    }
}
