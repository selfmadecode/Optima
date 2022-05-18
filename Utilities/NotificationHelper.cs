using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities
{
    public static class NotificationHelper
    {
        public static string GetNotificationType(this NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Approved_Transaction:
                    return "APPROVED TRANSACTION NOTIFICATION";

                case NotificationType.Declined_Transaction:
                    return "DECLINED TRANSACTION NOTIFICATION";

                case NotificationType.New_Card:
                    return "NEW CARD NOTIFICATION NOTIFICATION";

                default:
                    return "NOTIFICATION ALERT";
            }
        }
    }
}
