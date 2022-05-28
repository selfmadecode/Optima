using AzureRays.Shared.ViewModels;
using Optima.Models.DTO.RateDTO;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public interface IDenominationService
    {
        Task<BaseResponse<bool>> CreateDenomination(CreateRateDTO model);
        Task<BaseResponse<RateDTO>> GetDenomination(Guid id);
        Task<BaseResponse<bool>> UpdateDenomination(UpdateRateDTO model);
        Task<BaseResponse<bool>> DeleteDenomination(Guid id); 
        Task<BaseResponse<List<RateDTO>>> GetAllDenominations(); 
    }
}
