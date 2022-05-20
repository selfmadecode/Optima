using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class Denomination : BaseEntity
    {
        public string DenominationName { get; set; }
        public int DenominationRate { get; set; }
        public Country Country { get; set; }
        public Guid CountryId { get; set; }
        public SubCardType SubCardType { get; set; }
        public Guid? SubCardTypeId { get; set; }
        public CardType CardType { get; set; }
        public Guid? CardTypeId { get; set; } 

    }
}
