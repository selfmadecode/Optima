using Optima.Models.DTO.FaqsDTO;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IFaqService
    {
        Task<BaseResponse<bool>> Create(CreateFaqDTO model, Guid UserId);
        Task<BaseResponse<bool>> Update(Guid Id, UpdateFaqDTO model, Guid UserId);
        Task<BaseResponse<bool>> Delete(Guid id);
        Task<BaseResponse<FaqDTO>> Get(Guid id);
        Task<BaseResponse<List<FaqDTO>>> Get();
    }
}
