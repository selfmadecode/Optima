using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.DenominationDTOs;
using Optima.Services.Implementation;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DenominationController : BaseController
    {
        private readonly IDenominationService _rateService; 

        public DenominationController(IDenominationService rateService)
        {
            _rateService = rateService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy ="CanAdd")]
        public async Task<IActionResult> Create([FromBody] CreateDenominationDTO model)
        {
            try
            {
                return ReturnResponse(await _rateService.CreateDenomination(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<DenominationDTO>), 200)]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                return ReturnResponse(await _rateService.GetDenomination(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<DenominationDTO>>), 200)]

        public async Task<IActionResult> GetAll()
        {
            try
            {
                return ReturnResponse(await _rateService.GetAllDenominations());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Update([FromBody] UpdateDenominationDTO model)
        {
            try
            {
                return ReturnResponse(await _rateService.UpdateDenomination(model, UserId));
            }
            catch (Exception ex)
            {
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
                return ReturnResponse(await _rateService.DeleteDenomination(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

    }
}
