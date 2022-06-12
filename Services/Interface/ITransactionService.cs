using AzureRays.Shared.ViewModels;
using Optima.Models.DTO.TransactionDTO;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
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
        Task<BaseResponse<PagedList<TransactionDTO>>> GetUserCreditDebit(BaseSearchViewModel model, Guid UserId);
        Task<BaseResponse<PagedList<TransactionDTO>>> GetAllUserCreditDebit(BaseSearchViewModel model); 

    }
}
