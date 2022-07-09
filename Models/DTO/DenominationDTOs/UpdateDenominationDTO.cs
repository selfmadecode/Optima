using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.DenominationDTOs
{
    public class UpdateDenominationDTO
    {
        public Guid Id { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
 