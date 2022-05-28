using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class Card : BaseEntity
    {
        public string Name { get; set; }
        public string Logo { get; set; }


        public List<CardCreation> CardCreations { get; set; } = new List<CardCreation>();




    }
}
