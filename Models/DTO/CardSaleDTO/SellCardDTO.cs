using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardSaleDTO
{
    public class SellCardDTO
    {
        public List<SellerCardTypeDTO> CardTypeDTOs { get; set; } = new List<SellerCardTypeDTO>();
    }

    public class SellerCardTypeDTO : IValidatableObject
    {
       
        [Required]
        public Guid CardTypeDenominationId { get; set; }
               
        [Required]
        public List<string> CardCodes { get; set; } = new List<string>();
        public List<IFormFile> CardImages { get; set; } = new List<IFormFile>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var cardImage in CardImages)
            {
                if (!(cardImage is null))
                {
                    if (cardImage.Length > 1024 * 1024)
                        yield return new ValidationResult("Logo file size must not exceed 1Mb");


                    var allowedExt = new string[] { "jpg", "png", "jpeg" };
                    var ext = Path.GetExtension(cardImage.FileName).ToLower();

                    var extensionValid = allowedExt.ToList().Any(x => $".{x}".Equals(ext, StringComparison.InvariantCultureIgnoreCase));

                    if (!extensionValid)
                        yield return new ValidationResult("Logo file type must be .jpg or .png or .jpeg");
                }
            }
           
        }
    }
  
}

   


