using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class ConfigureReceiptTypeCardDTO
    {
        public List<ReceiptTypeCardConfigDTO> ReceiptTypeConfig { get; set; } = new List<ReceiptTypeCardConfigDTO>();
    }
    public class ReceiptTypeCardConfigDTO
    {
        [Required]
        public Guid CountryId { get; set; }

        [Required]
        public Guid CardTypeId { get; set; }

        [Required]
        public Guid ReceiptTypeId { get; set; }
        public List<CardRateConfigDTO> CardRates { get; set; }
    }

}
