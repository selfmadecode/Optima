using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class UserDevice : BaseEntity
    {
        public Guid UserId { get; set; }
       
        public string Token { get; set; }
    }
}
