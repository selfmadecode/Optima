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
        Task<BaseResponse<bool>> CreateRate(CreateRateDTO model);
        Task<BaseResponse<RateDTO>> GetRate(Guid id);
        Task<BaseResponse<bool>> UpdateRate(UpdateRateDTO model);
        Task<BaseResponse<bool>> DeleteRate(Guid id); 
        Task<BaseResponse<List<RateDTO>>> GetAllRates(); 
    }
}
