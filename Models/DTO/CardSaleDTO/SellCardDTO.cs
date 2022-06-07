using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardSaleDTO
{
    public class SellCardDTO
    {
        public List<CardTypeDTO> CardTypeDTO { get; set; }
        public List<IFormFile> CardImages { get; set; }
    }

    public class CardTypeDTO
    {
        [Required]
        public Guid CardTypeDenominationId { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
