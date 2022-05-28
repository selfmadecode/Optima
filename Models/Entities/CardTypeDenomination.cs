using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CardTypeDenomination : BaseEntity
    {
        public CardType CardType { get; set; }
        public Guid CardTypeId { get; set; }


        public Denomination Denomination { get; set; }
        public Guid DenominationId { get; set; }


        public virtual Receipt Receipt { get; set; }
        public Guid? ReceiptId { get; set; }


        public virtual Prefix Prefix { get; set; }
        public Guid? PrefixId { get; set; }


        public decimal Rate { get; set; }
    }
}
