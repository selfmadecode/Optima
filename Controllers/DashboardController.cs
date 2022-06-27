using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)] 
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
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
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
    }
}
