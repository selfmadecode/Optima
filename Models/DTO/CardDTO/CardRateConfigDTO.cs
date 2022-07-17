using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{    
    public class CardRateConfigDTO
    {
        //[Required]
        public Guid DenominationId { get; set; }

       // [Required]
        public Decimal Rate { get; set; }
    }

    public class UpdateCardRateDenominationConfigDTO : CardRateConfigDTO 
    {
        public Guid CardRateId { get; set; }

    }

    public class UpdateCardConfig
    {
       // [Required]
        public Guid CountryId { get; set; }

        //[Required]
        public Guid CardTypeId { get; set; }

        public List<UpdateCardRateDenominationConfigDTO> UpdateCardRateDenominationConfigDTO { get; set; } 
    }

}
