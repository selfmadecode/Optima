using Optima.Models.Entities;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class Card : BaseEntity
    {
        public Card()
        {
            CardType = new List<CardType>();
        }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public CardStatus CardStatus { get; set; }
        public List<CardType> CardType { get; set; } 
    }
}
