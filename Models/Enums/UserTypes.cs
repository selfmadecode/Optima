using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Enums
{
    public enum UserTypes
    {
        [Description("Super Admin")]
        SUPER_ADMIN = 1,
        [Description("Admin")]
        ADMIN = 2,
        [Description("User")]
        USER
    }
}
