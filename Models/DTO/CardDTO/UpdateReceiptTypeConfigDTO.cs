using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class UpdateReceiptTypeConfigDTO
    {

        [Required]
        public Guid CardId { get; set; }
        public List<ReceiptTypeUpdateConfigDTO> ReceiptTypeUpdateCardConfigDTO { get; set; }
    }

    public class ReceiptTypeUpdateConfigDTO : UpdateCardConfigDTO
    {
        [Required]
        public Guid ReceiptId { get; set; } 
    }
}
