using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
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

    public class UpdateReceiptTypeCardDTO : IValidatableObject
    {
        public string CardName { get; set; }
        public IFormFile Logo { get; set; }
        public List<UpdateReceiptTypeCardConfigDTO> UpdateReceiptTypeConfigDTO { get; set; } = new List<UpdateReceiptTypeCardConfigDTO>();

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

    public class UpdateReceiptTypeCardConfigDTO : UpdateCardConfig
    {

        [Required]
        public Guid ReceiptTypeId { get; set; }

    }
    
}
