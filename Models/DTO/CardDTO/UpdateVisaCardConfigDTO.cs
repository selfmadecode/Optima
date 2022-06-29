using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class UpdateVisaCardConfigDTO
    {
        public List<VisaCardUpdateConfigDTO> VisaCardUpdateConfigDTO { get; set; } = new List<VisaCardUpdateConfigDTO>();
    }

    public class VisaCardUpdateConfigDTO : UpdateCardConfigDTO
    {
        [Required]
        public Guid PrefixId { get; set; }       
    }
}
