using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.NotificationDTO
{
  
    public class SendPushNotificationDTO
    {
        public List<Guid> UserIds { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public class TestPushNotificationDTO
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string Token { get; set; }
    }
    
}
