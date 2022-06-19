using Optima.Models.DTO.DashboardDTOs;
using Optima.Models.Enums;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IDashboardService
    {
        Task<BaseResponse<DashboardDTO>> Dashboard();
        Task<BaseResponse<DashboardFilterDTO>> Dashboard(DateRangeQueryType range);
    }
}
