using Optima.Models.DTO.UserDTOs;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.TransactionDTO
{
    public class TransactionDTO
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }    
        public UserDTO UserDTO { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<CreditDebitDTO> CreditDebitDTOs { get; set; }

        public static implicit operator TransactionDTO(WalletBalance model)
        {
            return model is null ? null
               : new TransactionDTO
               {
                   Id = model.Id,
                   Balance = model.Balance,
                   UserDTO = model.User,
                   CreditDebitDTOs = model.CreditDebit.Select(x=>(CreditDebitDTO)x).ToList(),
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
