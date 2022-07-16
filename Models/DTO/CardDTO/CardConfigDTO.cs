using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class CardConfigDTOS
    {
        [Required]
        public Guid DenominationId { get; set; }

        [Required]
        public Decimal Rate { get; set; }

        public Guid CountryId { get; set; }

        public Guid CardTypeId { get; set; }
    }
}
