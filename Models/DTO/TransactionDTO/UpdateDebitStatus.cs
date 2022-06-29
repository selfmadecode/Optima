using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.TransactionDTO
{
    public class UpdateDebitStatus
    {
        public CreditDebitStatus CreditDebitStatus { get; set; }
    }

    public enum CreditDebitStatus
    {
        Approved = 1,
        Declined
    };
}
