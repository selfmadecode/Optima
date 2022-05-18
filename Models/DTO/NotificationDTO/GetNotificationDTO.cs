using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.NotificationDTO
{
    public class GetNotificationDTO
    {
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid NotificationId { get; set; }
        public bool IsRead { get; set; }
    }
}
