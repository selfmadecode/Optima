using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CardType : BaseEntity
    {      
        public virtual Card Card { get; set; }
        public Guid CardId { get; set; }

        public virtual Category CardCategory { get; set; }
        public Guid CardCategoryId { get; set; }        

        public virtual Country Country { get; set; }
        public Guid CountryId { get; set; }

        public virtual Receipt Receipt  { get; set; }
        public Guid? ReceiptId { get; set; }

        public virtual VisaPrefix SpecialPrefix { get; set; }
        public Guid? SpecialPrefixId { get; set; }

        public virtual Denomination Denomination { get; set; } // Card denomination ($10, $20...)
        public Guid DenominationId { get; set; }

        // Rate at which this card will be bought
        public decimal Rate { get; set; }

    }
}
