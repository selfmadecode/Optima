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

    public class UpdateVisaTypeCardDTO
    {

        public UpdateVisaTypeCardDTO()
        {
        }

        public string CardName { get; set; }

        public List<UpdateVisaTypeCardConfigDTO> UpdateVisaTypeConfigDTO { get; set; } = new List<UpdateVisaTypeCardConfigDTO>();

    }

    public class UpdateVisaTypeCardConfigDTO : UpdateCardConfig
    {

        [Required]
        public Guid PrefixId { get; set; }

    }
}
