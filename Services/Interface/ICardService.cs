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
        Task<BaseResponse<bool>> ConfigureVisaCard(ConfigureVisaCardDTO model, Guid UserId);
        Task<BaseResponse<bool>> ConfigureReceiptTypeCard(ConfigureReceiptTypeCardDTO model, Guid UserId);
        Task<BaseResponse<bool>> ConfigureNormalCard(ConfigureNormalCardDTO model, Guid UserId);
        Task<BaseResponse<CardDTO>> GetCard(Guid id);
        Task<BaseResponse<PagedList<CardDTO>>> GetAllCard(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<CardDTO>>> GetAllPendingCardConfig(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<CardDTO>>> GetAllApprovedCardConfig(BaseSearchViewModel model);
        Task<BaseResponse<bool>> UpdateCard(UpdateCardDTO model, Guid UserId);
        Task<BaseResponse<bool>> UpdateVisaCard(UpdateVisaCardConfigDTO model, Guid UserId);
        Task<BaseResponse<bool>> UpdateReceiptCard(UpdateReceiptTypeConfigDTO model, Guid UserId);
        Task<BaseResponse<bool>> UpdateNormalCard(UpdateNormalCardConfigDTO model, Guid UserId);
        Task<BaseResponse<bool>> DeleteCardType(DeleteCardTypeDTO model);
        Task<BaseResponse<bool>> CardStatusUpdate(UpdateCardStatusDTO model, Guid UserId);
        Task<BaseResponse<PagedList<CardDTO>>> AllActiveCards(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<CardDTO>>> AllInActiveCards(BaseSearchViewModel model); 
    }
}
