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
  
    public class UpdateNormalTypeCardDTO
    {
  
        public string CardName { get; set; }

        public List<UpdateCardConfig> UpdateNormalCardTypeConfigDTO { get; set; } = new List<UpdateCardConfig>();
       
    }

}

