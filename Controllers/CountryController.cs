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
    [Route("api/[controller]")]
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
                return ReturnResponse(await _countryService.CreateCountry(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet]
        [Route("{id:Guid}")]
        [ProducesResponseType(typeof(BaseResponse<CountryDTO>), 200)]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
               return ReturnResponse(await _countryService.GetCountry(id));
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
                return ReturnResponse(await _countryService.GetAllCountry(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet]
        [Route("No-Pagination")]
        public async Task<IActionResult> GetAllNp() 
        {
            try
            {
                return ReturnResponse(await _countryService.GetAllCountry());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpPut]
        [Route("{CountryId:Guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Update(Guid CountryId, [FromForm] UpdateCountryDTO model)
        {
            try
            {
                return ReturnResponse(await _countryService.UpdateCountry(model, UserId, CountryId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpDelete]
        [Route("{id:Guid}")]
        //[Authorize(Policy ="CanDelete")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                return ReturnResponse(await _countryService.DeleteCountry(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }
    }
}
