using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.CountryDTO;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CountryController : BaseController
    {
        private readonly ICountryService _countryService;
        private readonly ILog _logger;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
            _logger = LogManager.GetLogger(typeof(CountryController));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy = "CanAdd")]
        public async Task<IActionResult> Create([FromBody]CreateCountryDTO model)
        {
            try
            {
                var result = await _countryService.CreateCountry(model);

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
        [ProducesResponseType(typeof(BaseResponse<CountryDTO>), 200)]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _countryService.GetCountry(id);

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
        [ProducesResponseType(typeof(BaseResponse<PagedList<CountryDTO>>), 200)]

        public async Task<IActionResult> GetAll([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                var result = await _countryService.GetAllCountry(model);

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
        public async Task<IActionResult> GetAllNp() 
        {
            try
            {
                var result = await _countryService.GetAllCountry();

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Update([FromBody] UpdateCountryDTO model)
        {
            try
            {
                var result = await _countryService.UpdateCountry(model);

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
        //[Authorize(Policy ="CanDelete")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _countryService.DeleteCountry(id);

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
