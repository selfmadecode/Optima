using Optima.Models.DTO.TermsDTO;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface ITermsService
    {
        Task<BaseResponse<bool>> AcceptTermsAndCondition(Guid UserId);
        Task<BaseResponse<GetRateDTO>> GetTermsAndCondition();
        Task<BaseResponse<Guid>> UpdateTermsAndCondition(Guid modifiedBy, UpdateTermsDTO model);
    }
}
