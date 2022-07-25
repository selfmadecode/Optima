using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.BankAccountDTOs
{
    public class UpdateBankAccountDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string AccountName { get; set; }

        [Required]
        public string BankName { get; set; }

        [Required]
        public string AccountNumber { get; set; }
    }

    public class SetBankAsPrimaryDTO
    {
        [Required]
        public Guid BankId { get; set; }
    }
}
