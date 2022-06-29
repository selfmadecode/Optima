using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities.Helpers
{
    public static class PermisionProvider
    {
        public enum Permission
        {
            CARD = 1,
            DASHBOARD,
            TRANSACTION,
            MARKETPLACE,
            USERS,
            SETTINGS
        }

        public static List<Permission> SuperAdminPermission()
        {
            return new List<Permission>
            {
                Permission.CARD,
                Permission.DASHBOARD,
                Permission.TRANSACTION,
                Permission.MARKETPLACE,
                Permission.USERS,
                Permission.SETTINGS
            };
        }

        public static List<Permission> AdminPermission()
        {
            return new List<Permission>
            {
                Permission.CARD,
                Permission.DASHBOARD,
                Permission.USERS
            };
        }
    }
}
