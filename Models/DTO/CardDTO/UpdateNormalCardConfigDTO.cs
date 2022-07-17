using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
   /* public class UpdateNormalCardConfigDTO
    {
        public List<UpdateCardConfigDTO> UpdateCardConfigDTO { get; set; } = new List<UpdateCardConfigDTO>();
    }
   */
    public class UpdateNormalTypeCardDTO : IValidatableObject
    {
        public string CardName { get; set; }
        public IFormFile Logo { get; set; }
        public List<UpdateCardConfig> UpdateNormalCardTypeDTO { get; set; } = new List<UpdateCardConfig>();

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

}

