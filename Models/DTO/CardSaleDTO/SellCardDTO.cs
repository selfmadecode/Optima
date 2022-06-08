using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardSaleDTO
{
    public class SellCardDTO
    {
        public List<SellerCardTypeDTO> CardTypeDTOs { get; set; } = new List<SellerCardTypeDTO>();
    }

    public class SellerCardTypeDTO
    {
       
        [Required]
        public Guid CardTypeDenominationId { get; set; } // rate
               
        [Required]
        public List<string> CardCodes { get; set; } = new List<string>();
        public List<IFormFile> CardImages { get; set; } = new List<IFormFile>();
    }
  
}

   


