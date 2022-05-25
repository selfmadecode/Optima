using Optima.Models.DTO.TransactionDTO;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface ITransactionService
    {
        Task<BaseResponse<BalanceInquiryDTO>> GetUserAccountBalance(Guid userId);
        Task<BaseResponse<bool>> Withdraw(WithdrawDTO model, Guid userId);
    }
}
