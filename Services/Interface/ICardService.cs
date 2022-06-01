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
    }
}
