using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class ConfigureReceiptTypeCardDTO
    {
        public Guid CardId { get; set; }

        public List<ReceiptTypeCardCountryConfigDTO> CardCountryConfig { get; set; }
    }

    
}
