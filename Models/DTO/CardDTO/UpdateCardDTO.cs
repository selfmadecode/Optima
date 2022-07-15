using Microsoft.AspNetCore.Http;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class UpdateCardDTO : IValidatableObject
    {
        [Required]
        public string Name { get; set; }
        public IFormFile Logo { get; set; }
        [Required]
        public List<Guid> CountryIds { get; set; } = new List<Guid>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!(Logo is null))
            {
                if (Logo.Length > 1024 * 1024)
                    yield return new ValidationResult("Logo file size must not exceed 1Mb");


                var allowedExt = new string[] { "jpg", "png", "jpeg" };
                var ext = Path.GetExtension(Logo.FileName).ToLower();

                var extensionValid = allowedExt.ToList().Any(x => $".{x}".Equals(ext, StringComparison.InvariantCultureIgnoreCase));

                if (!extensionValid)
                    yield return new ValidationResult("Logo file type must be .jpg or .png or .jpeg");
            }
        }
    }

    public class UpdateCardConfigDTO
    {     
        public Guid CardTypeDenominationId { get; set; }
        [Required]
        public Guid DenominationId { get; set; }

        [Required]
        public Decimal Rate { get; set; }

        public Guid CountryId { get; set; }

        public Guid CardTypeId { get; set; }
    }

    public class UpdateCardStatusDTO 
    {
        public CardStatus CardStatus { get; set; } 
    }

    /*public enum UpdateCardStatus
    {
        Activate = 1,
        Deactivate
    }*/
}
