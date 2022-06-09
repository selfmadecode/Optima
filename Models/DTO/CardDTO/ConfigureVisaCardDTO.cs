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
        [Required]
        public Guid CardId { get; set; }
        public List<VisaCardConfigDTO> VisaCardConfigDTO { get; set; }
    }

    public class VisaCardConfigDTO : CardConfigDTO
    {
        [Required]
        public Guid PrefixId { get; set; }       

    }
}
