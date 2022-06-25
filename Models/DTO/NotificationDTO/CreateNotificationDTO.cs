using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.NotificationDTO
{
    public class CreateNotificationDTO
    {
        public NotificationType Type { get; set; }
        
        public Guid NotificationOwnerId { get; set; }

        public string Message { get; set; }
    }

    public class CreateAdminNotificationDTO
    {
        public NotificationType Type { get; set; }

        public string Message { get; set; }
        public Guid UserId { get; set; }
    }
}
