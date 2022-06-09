using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardSaleDTO
{
    public class UpdateSellCardDTO
    {
        [Required]
        public Guid TransactionId { get; set; }
        public List<UpdateCardSoldDTO> UpdateCardSoldDTOs { get; set; } 

    }

    public class UpdateCardSoldDTO 
    {
        [Required]
        public Guid CardSoldId { get; set; }
        public List<UpdateCardCodeDTO> UpdateCardCodeDTOs { get; set; }  
    }

    public class UpdateCardCodeDTO
    {
        [Required]
        public Guid CardCodeId { get; set; }
        [Required]
        public string CardCode { get; set; }
    }
}

