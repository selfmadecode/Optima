using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities.Helpers
{
    public class RoleHelper
    {
        public static Guid SuperAdmin => Guid.Parse("131e8b31-59a7-4c80-9b71-08b60e951e5c");
        public const string SUPERADMIN = nameof(SUPERADMIN);
        public static Guid Admin => Guid.Parse("a1b6b6b0-0825-4975-a93d-df3dc86f8cc7");
        public const string ADMIN = nameof(ADMIN);

    }

}
