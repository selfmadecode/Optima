using Optima.Models.DTO.BankAccountDTOs;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface 
{
    public interface IBankAccountService
    {
        Task<BaseResponse<bool>> CreateBankAccount(CreateBankAccountDTO model, Guid UserId);
        Task<BaseResponse<BankAccountDTO>> GetBankAccount(Guid id, Guid UserId);
        Task<BaseResponse<bool>> UpdateBankAccount(UpdateBankAccountDTO model, Guid UserId);
        Task<BaseResponse<bool>> DeleteBankAccount(Guid id, Guid UserId);
        Task<BaseResponse<List<BankAccountDTO>>> GetAllBankAccount(Guid UserId); 

    }
}
