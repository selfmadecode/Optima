using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
