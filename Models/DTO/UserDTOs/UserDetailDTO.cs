using Optima.Models.DTO.BankAccountDTOs;
using Optima.Models.DTO.CardTransactionDTOs;
using Optima.Models.Entities;
using Optima.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.UserDTOs
{
    public class UserDetailDTO
    {
        public UserDTO UserDTO { get; set; }
        public List<BankAccountDTO> BankAccountDTOs { get; set; }
        public List<AllTransactionDTO> CardTransactionDTOs { get; set; }      

    }
}
