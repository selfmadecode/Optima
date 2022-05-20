using Optima.Models.Entities;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Optima.Models.Entities
{
    public class CardType :  BaseEntity
    {
        public CardCategory CardCategory { get; set; }
        public Country Country { get; set; }
        public Guid CountryId { get; set; }
        public List<SubCardType> SubCardTypes { get; set; }
        public List<Denomination> Denominations { get; set; }
    }
}
 