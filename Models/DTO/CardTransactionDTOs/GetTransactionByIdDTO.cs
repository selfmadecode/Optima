using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardTransactionDTOs
{
    public class GetTransactionByIdDTO
    {
        [Required]
        public Guid Id { get; set; }
    }
}
