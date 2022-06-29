using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.PrefixDTOs;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrefixController : BaseController
    {
        private readonly IPrefixService _prefixService;

        public PrefixController(IPrefixService prefixService)
        {
            _prefixService = prefixService;

        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Create(CreatePrefixDTO model)
        {
            try
            {
                return ReturnResponse(await _prefixService.CreatePrefix(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<PrefixDTO>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return ReturnResponse(await _prefixService.GetAllPrefix());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Update(UpdatePrefixDTO model)
        {
            try
            {
                return ReturnResponse(await _prefixService.UpdatePrefix(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                return ReturnResponse(await _prefixService.DeletePrefix(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
