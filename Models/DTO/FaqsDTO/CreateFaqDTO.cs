using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.FaqsDTO
{
    public class CreateFaqDTO
    {
        [Required]
        public string Question { get; set; }
        [Required]
        public string Answer { get; set; }
    }
}
