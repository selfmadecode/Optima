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
    public class SellCardDTO : IValidatableObject
    {

        [Required]
        public Guid CardId { get; set; }

        [Required]
        public Guid CardTypeId { get; set; }

        [Required]
        public Guid CountryId { get; set; }

        public Guid? SubTypeId { get; set; }
        // SubTypeId Can be receiptId or prefixId, validate what it should be using the cardId
        // Get Card by Id, if card is receiptType card, then the value of subtypeid should be a receipt typeid

        public List<SelectedCardDenominationDTO> SelectedCardDenominationDTO { get; set; }
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

    public class SelectedCardDenominationDTO
    {
        public Guid DenominationId { get; set; }
        public string GiftCardCode { get; set; }
    }  
}

   


