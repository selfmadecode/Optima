using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.PrefixDTOs
{
    public class UpdatePrefixDTO
    {
        public Guid Id { get; set; }
        public string PrefixNumber { get; set; } 
    }
}
