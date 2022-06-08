using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class TransactionUploadFiles : BaseEntity
    {
        public string LogoUrl { get; set; }

        public Guid TransactionId { get; set; }
        public CardTransaction Transaction { get; set; }
    }
}
