using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class Receipt : BaseEntity
    {
        public ReceiptType ReceiptType { get; set; }
    }
}
