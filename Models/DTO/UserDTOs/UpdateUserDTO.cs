using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.UserDTOs
{
    public class UpdateUserDTO : IValidatableObject
    {
        [Required]
        public string PhoneNumber { get; set; }
        public IFormFile ProfilePicture { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!(ProfilePicture is null))
            {
                if (ProfilePicture.Length > 1024 * 1024)
                    yield return new ValidationResult("Logo file size must not exceed 1Mb");


                var allowedExt = new string[] { "jpg", "png", "jpeg" };
                var ext = Path.GetExtension(ProfilePicture.FileName).ToLower();

                var extensionValid = allowedExt.ToList().Any(x => $".{x}".Equals(ext, StringComparison.InvariantCultureIgnoreCase));

                if (!extensionValid)
                    yield return new ValidationResult("Logo file type must be .jpg or .png or .jpeg");
            }
        }
    }
}
