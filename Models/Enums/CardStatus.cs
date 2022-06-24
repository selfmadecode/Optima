using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Enums
{
    public enum CardStatus
    {
         [Description("Pending")]Pending = 1,
         [Description("Approved")] Active,
         [Description("Disabled")] Disabled
    }
}
