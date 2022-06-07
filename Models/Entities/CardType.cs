using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CardType : BaseEntity
    {
        public CardType()
        {
            CardTypeDenomination = new List<CardTypeDenomination>();
        }

        public CardStatus CardStatus { get; set; }

        public CardCategory CardCategory { get; set; }

        public Country Country { get; set; }
        public Guid CountryId { get; set; }
        
        public virtual Card Card { get; set; }
        public Guid CardId { get; set; }       

        //public Denomination Denomination { get; set; } // $10
        //public Guid DenominationId { get; set; }

        public List<CardTypeDenomination> CardTypeDenomination { get; set; }

        //public virtual Receipt Receipt { get; set; }
        //public Guid? ReceiptId { get; set; }

        //public virtual VisaPrefix SpecialPrefix { get; set; }
        //public Guid? SpecialPrefixId { get; set; }

        // Rate at which this card will be bought
        //public decimal Rate { get; set; } // 100
    }    
}
