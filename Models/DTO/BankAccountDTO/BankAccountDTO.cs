using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.BankAccountDTO
{
    public class BankAccountDTO
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }


        public static implicit operator BankAccountDTO(BankAccount model)
        {
            return model is null ? null
               : new BankAccountDTO
               {
                   Id = model.Id,
                   AccountName = model.AccountName,
                   AccountNumber = model.AccountNumber,
                   BankName = model.BankName,
               };
        } 
    }   
}
