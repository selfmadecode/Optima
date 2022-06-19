using Optima.Models.DTO.BankAccountDTOs;
using Optima.Models.DTO.UserDTOs;
using Optima.Models.Entities;
using Optima.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.TransactionDTO
{
    public class CreditDebitDTO
    {

        public Guid Id { get; set; }
        public Guid WalletBalanceId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string TransactionStatus { get; set; }
        public UserDTO ActionByUserDTO { get; set; }
        public BankAccountDTO BankAccountDTO { get; set; }
        public DateTime CreatedOn { get; set; }


        public static implicit operator CreditDebitDTO(CreditDebit model)
        {
            return model is null ? null
               : new CreditDebitDTO
               {
                   Id = model.Id,
                   WalletBalanceId = model.WalletBalanceId,
                   Amount = model.Amount,
                   TransactionType = model.TransactionType.GetDescription(),
                   TransactionStatus = model.TransactionStatus.GetDescription(),
                   ActionByUserDTO = model.ActionedByUser,
                   BankAccountDTO = model.BankAccount,
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
