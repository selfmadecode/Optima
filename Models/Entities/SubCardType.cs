using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class SubCardType : BaseEntity
    {
        public string SubCardTypeName { get; set; }
        public Guid CardTypeId { get; set; }
        public CardType CardType { get; set; }
        public List<Denomination> Denominations { get; set; }

    }
}
