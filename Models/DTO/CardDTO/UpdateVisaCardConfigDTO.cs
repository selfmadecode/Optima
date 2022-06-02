using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class UpdateVisaCardConfigDTO
    {
        [Required]
        public Guid CardId { get; set; }
        public List<VisaCardUpdateConfigDTO> VisaCardUpdateConfigDTO { get; set; }
    }

    public class VisaCardUpdateConfigDTO : UpdateCardConfigDTO
    {
        public Guid PrefixId { get; set; }       
    }
}
