using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.BankAccountDTO
{
    public class UpdateBankAccountDTO
    {
        public Guid Id { get; set; } 
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
    }
}
