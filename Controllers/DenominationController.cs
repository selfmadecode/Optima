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
    public class DenominationController : BaseController
    {
        private readonly IDenominationService _rateService; 
        private readonly ILog _logger;

        public DenominationController(IDenominationService rateService)
        {
            _rateService = rateService;
            _logger = LogManager.GetLogger(typeof(DenominationController));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy ="CanAdd")]
        public async Task<IActionResult> Create([FromBody] CreateDenominationDTO model)
        {
            try
            {
                var result = await _rateService.CreateDenomination(model, UserId);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<DenominationDTO>), 200)]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _rateService.GetDenomination(id);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<DenominationDTO>>), 200)]

        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _rateService.GetAllDenominations();

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
        public async Task<IActionResult> Update([FromBody] UpdateDenominationDTO model)
        {
            try
            {
                var result = await _rateService.UpdateDenomination(model, UserId);

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
                var result = await _rateService.DeleteDenomination(id);

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
