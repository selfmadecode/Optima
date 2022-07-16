using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    /*public class UpdateReceiptTypeConfigDTO
    {
        public List<ReceiptTypeUpdateConfigDTO> ReceiptTypeUpdateCardConfigDTO { get; set; } = new List<ReceiptTypeUpdateConfigDTO>();
    }

    public class ReceiptTypeUpdateConfigDTO : UpdateCardConfigDTO
    {
        [Required]
        public Guid ReceiptId { get; set; } 
    }*/

    public class UpdateReceiptTypeCardDTO 
    {
        public List<UpdateReceiptTypeCardConfigDTO> UpdateReceiptTypeConfigDTO { get; set; } = new List<UpdateReceiptTypeCardConfigDTO>();
    }

    public class UpdateReceiptTypeCardConfigDTO : UpdateCardConfig
    {

        [Required]
        public Guid ReceiptTypeId { get; set; }

    }

    
}
