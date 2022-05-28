using Optima.Models.DTO.CardDTO;
using Optima.Utilities.Helpers;
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
    }
}
