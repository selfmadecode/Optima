using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.CountryDTOs;
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
    [Authorize]
    public class CountryController : BaseController
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy = "CanAdd")]
        public async Task<IActionResult> Create([FromForm]CreateCountryDTO model)
        {
            try
            {
                var result = await _countryService.CreateCountry(model, UserId);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
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

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
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

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
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
                return HandleError(ex);
            }

        }

        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Update([FromForm] UpdateCountryDTO model)
        {
            try
            {
                var result = await _countryService.UpdateCountry(model, UserId);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
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

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }
    }
}
