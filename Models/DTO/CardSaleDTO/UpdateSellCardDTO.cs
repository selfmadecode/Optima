using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardSaleDTO
{
    public class UpdateSellCardDTO
    {
        public List<UpdateCardSoldDTO> UpdateCardSoldDTOs { get; set; } = new List<UpdateCardSoldDTO>();

    }

    public class UpdateCardSoldDTO 
    {
        [Required]
        public Guid CardSoldId { get; set; }
        public List<UpdateCardCodeDTO> UpdateCardCodeDTOs { get; set; } = new List<UpdateCardCodeDTO>();
    }

    public class UpdateCardCodeDTO
    {
        [Required]
        public Guid CardCodeId { get; set; }
        [Required]
        public string CardCode { get; set; }
    }
}

