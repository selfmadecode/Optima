using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.PrefixDTOs
{
    public class CreatePrefixDTO
    {
        [Required]
        public string PrefixNumber { get; set; } 
    }
}
