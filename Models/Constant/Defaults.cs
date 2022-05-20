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


        public static Guid RateTenId = Guid.Parse("973AF7A9-7F18-4E8B-ACD3-BC906580561A");
        public static Guid RateHundredId = Guid.Parse("2808568f-f79f-4b6e-8a75-7ee2f0700636");
        public static Guid RateTwoHundredId = Guid.Parse("0104cf12-fff0-472f-930f-ee6fe2f1dc8f");
        public static Guid RateTwoFiftyId = Guid.Parse("2de23f19-7b67-4d94-b18f-106802088d93");
        public static Guid RateThreeHundredId = Guid.Parse("8f3bba1a-892f-47ec-afcc-d7b3f5ebbf6f");
        public static Guid RateFiveHundredId = Guid.Parse("533f5a0e-2e49-4a24-b769-352d1ebea827");


        public static Guid UK = Guid.Parse("973AF7A9-7F18-4E8B-ACD3-BC906580561A");
        public static Guid US = Guid.Parse("2808568f-f79f-4b6e-8a75-7ee2f0700636");
        public static Guid Canada = Guid.Parse("0104cf12-fff0-472f-930f-ee6fe2f1dc8f");

    }
}
