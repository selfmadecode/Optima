using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.NotificationDTO
{
    public class DeviceDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
