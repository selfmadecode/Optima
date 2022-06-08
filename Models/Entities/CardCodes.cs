using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CardCodes : BaseEntity
    {
        public CardSold CardSold { get; set; }
        public Guid CardSoldId { get; set; }
        public string CardCode { get; set; }
    }
}
