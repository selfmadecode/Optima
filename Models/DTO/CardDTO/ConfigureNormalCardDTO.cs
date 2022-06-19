using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class ConfigureNormalCardDTO
    {
        [Required]
        public Guid CardId { get; set; }
        public List<CardConfigDTO> CardConfigDTO { get; set; } = new List<CardConfigDTO>();
    }
}
