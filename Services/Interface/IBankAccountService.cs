using Optima.Models.DTO.BankAccountDTO;
using Optima.Ultilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface 
{
    public interface IBankAccountService
    {
        Task<BaseResponse<bool>> CreateBankAccount(List<CreateBankAccountDTO> model, Guid UserId);
    }
}
