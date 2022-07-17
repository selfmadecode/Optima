using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{

    public class UpdateReceiptTypeCardDTO
    {

        public UpdateReceiptTypeCardDTO()
        {
        }

        public string CardName { get; set; }
        public IFormFile Logo { get; set; }
      
        public List<UpdateReceiptTypeCardConfigDTO> UpdateReceiptTypeConfigDTO { get; set; } = new List<UpdateReceiptTypeCardConfigDTO>();

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

    public class UpdateReceiptTypeCardConfigDTO : UpdateCardConfig
    {

        //[Required]
        public Guid ReceiptTypeId { get; set; }

    }
       
    
}
