using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class Notification : BaseEntity
    {
        public string Message { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
        public virtual ApplicationUser User { get; set; }
        public Guid UserId { get; set; }

        public bool IsAdminNotification { get; set; }
    }
}
