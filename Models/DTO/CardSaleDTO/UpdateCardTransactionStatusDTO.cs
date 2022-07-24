using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardSaleDTO
{
    public class UpdateCardTransactionStatusDTO
    {
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public TransactionStatus TransactionStatus { get; set; }
        public string Comment { get; set; }
    }
}
