using AzureRays.Shared.ViewModels;
using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.CardTransactionDTOs;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface ICardSaleService
    {
        Task<BaseResponse<bool>> CreateCardSales(SellCardDTO model, Guid UserId);
        Task<BaseResponse<PagedList<CardTransactionDTO>>> GetAllCardSales(BaseSearchViewModel model);
        Task<BaseResponse<bool>> UpdateCardSales(Guid transactionId, UpdateSellCardDTO model, Guid UserId);
        Task<BaseResponse<bool>> UpdateCardTransactionStatus(Guid transactionId, UpdateCardTransactionStatusDTO model, Guid UserId);
        Task<BaseResponse<PagedList<CardTransactionDTO>>> GetUserCardTransactions(BaseSearchViewModel model, Guid UserId);

    }
}
