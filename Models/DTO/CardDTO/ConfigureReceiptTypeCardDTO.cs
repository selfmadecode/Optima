using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class ConfigureReceiptTypeCardDTO
    {
        [Required]
        public Guid CardId { get; set; }
        public List<ReceiptTypeCardConfigDTO> ReceiptTypeCardConfigDTO { get; set; }
    }
    public class ReceiptTypeCardConfigDTO : CardConfigDTO
    {
        public Guid ReceiptTypeId { get; set; }
    }
}
