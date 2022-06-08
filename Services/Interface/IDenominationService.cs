using AzureRays.Shared.ViewModels;
using Optima.Models.DTO.DenominationDTOs;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public interface IDenominationService
    {
        Task<BaseResponse<bool>> CreateDenomination(CreateDenominationDTO model, Guid UserId);
        Task<BaseResponse<DenominationDTO>> GetDenomination(Guid id);
        Task<BaseResponse<bool>> UpdateDenomination(UpdateDenominationDTO model, Guid UserId);
        Task<BaseResponse<bool>> DeleteDenomination(Guid id); 
        Task<BaseResponse<List<DenominationDTO>>> GetAllDenominations(); 
    }
}
