using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class UpdateNormalCardConfigDTO
    {
        public List<UpdateCardConfigDTO> UpdateCardConfigDTO { get; set; } = new List<UpdateCardConfigDTO>();
    }

}

