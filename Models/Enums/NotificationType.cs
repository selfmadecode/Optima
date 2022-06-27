using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Enums
{
    public enum NotificationType
    {
        Approved_Transaction = 1,
        Declined_Transaction,
        Partial_Approved_Transaction,
        New_Card,
        Card_Sale,
        Nil
    }
}
