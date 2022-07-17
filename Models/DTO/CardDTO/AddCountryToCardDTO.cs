using Microsoft.AspNetCore.Http;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class AddCountryToCardDTO 
    {
        [Required]
        public List<Guid> CountryIds { get; set; } = new List<Guid>();        
    }

    public class UpdateCardConfigDTO
    {     
        public Guid CardTypeDenominationId { get; set; }
        [Required]
        public Guid DenominationId { get; set; }

        [Required]
        public Decimal Rate { get; set; }

        public Guid CountryId { get; set; }

        public Guid CardTypeId { get; set; }
    }

    public class UpdateCardStatusDTO 
    {
        public CardStatus CardStatus { get; set; } 
    }

    /*public enum UpdateCardStatus
    {
        Activate = 1,
        Deactivate
    }*/
}
