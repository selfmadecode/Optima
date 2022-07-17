using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class ValidateLogoHelperDTO
    {
        private readonly IConfiguration _config;

        public ValidateLogoHelperDTO(IConfiguration config)
        {
            _config = config;
        }
        public IFormFile Logo { get; set; }

        public async Task<BaseResponse<bool>> Validate()
        {
            var result = new BaseResponse<bool>();
            var fileSize = _config.GetValue<int>("FileSize");

            if (!(Logo is null))
            {
                if (Logo.Length > fileSize * fileSize)
                {
                    result.ResponseMessage = "Logo file size must not exceed 1Mb";
                    result.Errors.Add("Logo file size must not exceed 1Mb");
                    return result;
                }


                var allowedExt = new string[] { "jpg", "png", "jpeg" };
                var ext = Path.GetExtension(Logo.FileName).ToLower();

                var extensionValid = allowedExt.ToList().Any(x => $".{x}".Equals(ext, StringComparison.InvariantCultureIgnoreCase));

                if (!extensionValid)
                {
                    result.ResponseMessage = "Logo file type must be .jpg or .png or .jpeg";
                    result.Errors.Add("Logo file type must be .jpg or .png or .jpeg");
                    return result;
                }
                // yield return new ValidationResult("Logo file type must be .jpg or .png or .jpeg");
            }
            return result;
        }
    }
}
