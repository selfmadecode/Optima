﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.BankAccountDTO
{
    public class CreateBankAccountDTO
    {
        [Required]
        public string AccountName { get; set; }
        [Required]
        public string BankName { get; set; }
        [Required]
        public string AccountNumber { get; set; }
    }
}
