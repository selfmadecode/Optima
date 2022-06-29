using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardSaleDTO
{
    public class UpdateCardTransactionStatusDTO
    {
        public decimal Amount { get; set; }
        public TransactionStatus TransactionStatus { get; set; } 
    }
}
