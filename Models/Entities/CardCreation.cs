using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CardCreation
    {
        public Guid Id { get; set; }
        public CardCategory CardCategory { get; set; }
        public virtual Card Card { get; set; }
        public Guid CardId { get; set; }
        public virtual Country Country { get; set; }
        public Guid CountryId { get; set; }
        public virtual Receipt Receipt  { get; set; }
        public Guid? ReceiptId { get; set; }
        public decimal Denomination { get; set; }
        public virtual Rate Rate { get; set; }
        public Guid RateId { get; set; }
        public virtual SpecialPrefix SpecialPrefix { get; set; }
        public Guid? SpecialPrefixId { get; set; } 
    }
}
