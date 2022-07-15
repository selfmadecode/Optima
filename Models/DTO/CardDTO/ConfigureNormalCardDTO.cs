using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class ConfigureNormalCardDTO
    {
        public List<NormalCardConfigDTO> NormalCardConfigDTO { get; set; } = new List<NormalCardConfigDTO>();
    }

    public class NormalCardConfigDTO
    {
        [Required]
        public Guid CountryId { get; set; }

        [Required]
        public Guid CardTypeId { get; set; }

        public List<CardRateConfigDTO> CardRates { get; set; }
    }
}
