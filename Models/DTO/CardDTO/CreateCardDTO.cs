using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class CreateCardDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public BaseCardType BaseCardType { get; set; }

        [Required]
        public IFormFile Logo { get; set; }
        [Required]
        public List<Guid> CountryIds { get; set; } = new List<Guid>();

        public async Task<BaseResponse<bool>> Validate(IConfiguration configuration)
        {

            var fileSize = configuration.GetValue<int>("FileSize");

            if (!(Logo is null))
            {
                if (Logo.Length > fileSize * fileSize)
                {
                    return new BaseResponse<bool>("Logo file size must not exceed 1Mb", new List<string> { "Logo file size must not exceed 1Mb" });
                }


                var allowedExt = new string[] { "jpg", "png", "jpeg" };
                var ext = Path.GetExtension(Logo.FileName).ToLower();

                var extensionValid = allowedExt.ToList().Any(x => $".{x}".Equals(ext, StringComparison.InvariantCultureIgnoreCase));

                if (!extensionValid)
                {
                    return new BaseResponse<bool>("Logo file type must be .jpg or .png or .jpeg", new List<string> { "Logo file type must be .jpg or .png or .jpeg" });
                }
            }

            return new BaseResponse<bool>();
        }
    }
}
