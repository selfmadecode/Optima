using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class ConfigureVisaCardDTO
    {
        public List<VisaCardConfigDTO> VisaCardConfigDTO { get; set; } = new List<VisaCardConfigDTO>();
    }

    public class VisaCardConfigDTO
    {
        [Required]
        public Guid CountryId { get; set; }

        [Required]
        public Guid CardTypeId { get; set; }

        [Required]
        public Guid PrefixId { get; set; }
        public List<CardRateConfigDTO> CardRates { get; set; }
    }
}
