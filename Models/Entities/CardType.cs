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

        public List<CardSubType> CardSubType { get; set; }
    }    
}
