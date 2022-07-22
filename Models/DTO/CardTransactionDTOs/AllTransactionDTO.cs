using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardTransactionDTOs
{
    public class AllTransactionDTO
    {
        public Guid Id { get; set; }
        public string TransactionRefId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserName { get; set; }
        public string CardName { get; set; }
        public decimal TotalAmount { get; set; }
        public TransactionStatus Status { get; set; }

    }
}
