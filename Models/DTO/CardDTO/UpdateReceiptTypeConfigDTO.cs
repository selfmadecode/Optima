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

    public class UpdateReceiptTypeCardDTO : ValidateLogoHelperDTO
    {
        public UpdateReceiptTypeCardDTO(IConfiguration _config)
            : base(_config)
        {
        }

        //[Required]
        public string CardName { get; set; }
        public List<UpdateReceiptTypeCardConfigDTO> UpdateReceiptTypeConfigDTO { get; set; } = new List<UpdateReceiptTypeCardConfigDTO>();
    }

    public class UpdateReceiptTypeCardConfigDTO : UpdateCardConfig
    {

        [Required]
        public Guid ReceiptTypeId { get; set; }

    }
       
    
}
