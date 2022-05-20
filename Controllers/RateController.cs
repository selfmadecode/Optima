using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.RateDTO;
using Optima.Services.Implementation;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RateController : BaseController
    {
        private readonly IRateService _rateService; 
        private readonly ILog _logger;

        public RateController(IRateService rateService)
        {
            _rateService = rateService;
            _logger = LogManager.GetLogger(typeof(RateController));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy ="CanAdd")]
        public async Task<IActionResult> Create([FromBody] CreateRateDTO model)
        {
            try
            {
                var result = await _rateService.CreateRate(model);

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<RateDTO>), 200)]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _rateService.GetRate(id);

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<RateDTO>>), 200)]

        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _rateService.GetAllRates();

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Update([FromBody] UpdateRateDTO model)
        {
            try
            {
                var result = await _rateService.UpdateRate(model);

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy ="CanDelete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _rateService.DeleteRate(id);

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

    }
}
