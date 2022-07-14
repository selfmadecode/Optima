using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class ReceiptTypeCardCountryConfigDTO : CardCountryConfigDTO
    {
        public Guid ReceiptTypeId { get; set; }
    }

    public class CardCountryConfigDTO
    {
        public Guid CountryId { get; set; }

        public Guid CardTypeId { get; set; }
        public List<CardRateConfigDTO> Rates { get; set; }
    }

    public class CardRateConfigDTO
    {
        [Required]
        public Guid DenominationId { get; set; }

        [Required]
        public Decimal Rate { get; set; }
    }
}
