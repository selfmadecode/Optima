using AzureRays.Shared.ViewModels;
using Optima.Models.DTO.CardDTO;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface ICardService
    {
        Task<BaseResponse<CreatedCardDTO>> CreateCard(CreateCardDTO model, Guid UserId);
        Task<BaseResponse<bool>> ConfigureVisaCard(ConfigureVisaCardDTO model, Guid UserId, Guid CardId);
        Task<BaseResponse<bool>> ConfigureReceiptTypeCard(ConfigureReceiptTypeCardDTO model, Guid UserId, Guid CardId);
        Task<BaseResponse<bool>> ConfigureNormalCard(ConfigureNormalCardDTO model, Guid UserId, Guid CardId);
        Task<BaseResponse<CardDTO>> GetCard(Guid id);
        Task<BaseResponse<PagedList<CardDTO>>> GetAllCard(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<CardDTO>>> GetAllPendingCardConfig(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<CardDTO>>> GetAllApprovedCardConfig(BaseSearchViewModel model);
        Task<BaseResponse<bool>> AddCountryToCard(AddCountryToCardDTO model, Guid UserId, Guid CardId);
        Task<BaseResponse<bool>> UpdateVisaCard(UpdateVisaTypeCardDTO model, Guid UserId, Guid CardId);
        Task<BaseResponse<bool>> UpdateReceiptCard(UpdateReceiptTypeCardDTO model, Guid UserId, Guid CardId);
        Task<BaseResponse<bool>> UpdateNormalCard(UpdateNormalTypeCardDTO model, Guid UserId, Guid CardId);
        Task<BaseResponse<bool>> DeleteCardType(DeleteCardTypeDTO model, Guid CardId);
        Task<BaseResponse<bool>> CardStatusUpdate(UpdateCardStatusDTO model, Guid UserId, Guid CardId);
        Task<BaseResponse<PagedList<CardDTO>>> AllActiveCards(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<CardDTO>>> AllInActiveCards(BaseSearchViewModel model); 
        Task<BaseResponse<PagedList<CardDTO>>> AllBlockedCards(BaseSearchViewModel model);
        Task<BaseResponse<MainCardDTO>> GetCard_Ordered_By_Country(Guid id);
    }
}
