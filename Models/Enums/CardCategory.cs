using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Enums
{

    public enum CardCategory // Rename to Card type
    {
        [Description("Physical")] PHYSICAL = 1,
        [Description("E-Code")]  E_CODE,
    }
     
}
