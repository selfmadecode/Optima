using AzureRays.Shared.ViewModels;
using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.CardTransactionDTOs;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Optima.Services.Interface
{
    public interface ICardSaleService
    {
        Task<BaseResponse<bool>> CreateCardSales(SellCardDTO model, Guid UserId);
        Task<BaseResponse<CardTransactionDTO>> GetCardSale(GetTransactionByIdDTO model);
        Task<BaseResponse<bool>> UpdateCardSales(Guid transactionId, UpdateSellCardDTO model, Guid UserId);
        Task<BaseResponse<bool>> UpdateCardTransactionStatus(Guid transactionId, UpdateCardTransactionStatusDTO model, Guid UserId);
        Task<BaseResponse<PagedList<CardTransactionDTO>>> GetUserCardTransactions(BaseSearchViewModel model, Guid UserId);
        //Task<BaseResponse<PagedList<AllTransactionDTO>>> GetTransactionByStatus(BaseSearchViewModel model, List<TransactionStatus> status);
        Task<BaseResponse<PagedList<AllTransactionDTO>>> GetPendingTransaction(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<AllTransactionDTO>>> GetApproved_PartialApproved_Transaction(BaseSearchViewModel model);
        Task<BaseResponse<List<AllTransactionDTO>>> GetUserRecentTransactions(Guid UserId);
        Task<BaseResponse<PagedList<AllTransactionDTO>>> GetDeclinedTransaction(BaseSearchViewModel model);
    }
}
