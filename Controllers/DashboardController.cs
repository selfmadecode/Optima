using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.DashboardDTOs;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Optima.Utilities.Helpers.PermisionProvider;

namespace Optima.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = nameof(Permission.DASHBOARD))]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<DashboardDTO>), 200)] 
        public async Task<IActionResult> Get()
        {
            try
            {
                return ReturnResponse(await _dashboardService.Dashboard());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<DashboardFilterDTO>), 200)]
        public async Task<IActionResult> Performance([FromQuery] DateRangeQueryType range)
        {
            try
            {
                return ReturnResponse(await _dashboardService.Dashboard(range));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<DashboardGraphDTO>), 200)]
        public async Task<IActionResult> Graph([FromQuery] int year)
        {
            try
            {
                return ReturnResponse(await _dashboardService.Dashboard(year));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
