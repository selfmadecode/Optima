using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CardSold : BaseEntity
    {
        public Guid CardTypeDenominationId { get; set; }
        public CardTypeDenomination CardTypeDenomination { get; set; }
        public string CardCode { get; set; }
        public int Quantity { get; set; }
        public Decimal Amount { get; set; }

        public List<CardCodes> CardCodes { get; set; }
        public Guid TransactionId { get; set; }
        public CardTransaction Transaction { get; set; }
    }    
}
