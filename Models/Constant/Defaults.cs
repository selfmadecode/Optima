using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Constant
{
    public class Defaults
    {
        public const string SuperAdminEmail = "superadmin@optima.com";
        public static readonly Guid SuperAdminId = Guid.Parse("3fb897c8-c25d-4328-9813-cb1544369fba");
        public const string SuperAdminMobile = "07060882817";

        public const string SysUserEmail = "systemuser@optima.com";
        public static readonly Guid SysUserId = Guid.Parse("e7d58c75-18bc-4868-b54d-0a1fdf8fe94d");
        public const string SysUserMobile = "07060882817";

        public static Guid AdminId = Guid.Parse("973AF7A9-7F18-4E8B-ACD3-BC906580561A");
        public const string AdminEmail = "admin@optima.com";
        public const string AdminMobile = "07060882817";
    }
}
