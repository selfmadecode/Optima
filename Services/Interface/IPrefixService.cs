using Optima.Models.DTO.PrefixDTOs;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IPrefixService
    {
        Task<BaseResponse<bool>> CreatePrefix(CreatePrefixDTO model, Guid UserId);
        Task<BaseResponse<List<PrefixDTO>>> GetAllPrefix();
        Task<BaseResponse<bool>> UpdatePrefix(UpdatePrefixDTO model);
        Task<BaseResponse<bool>> DeletePrefix(Guid id);
    }
}
