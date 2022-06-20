using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.TermsDTO;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TermsController : BaseController
    {
        private readonly ITermsService _termsService;

        public TermsController(ITermsService termsService)
        {
            _termsService = termsService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Accept(AcceptTerms model)
        {
            try
            {
                return ReturnResponse(await _termsService.AcceptTermsAndCondition(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<GetTermsDTO>), 200)]
        public async Task<IActionResult> Get()
        {
            try
            {
                return ReturnResponse(await _termsService.GetTermsAndCondition());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }            
        }

        [HttpPut]
        [Authorize] // TODO : ONLY SUPER ADMIN
        [ProducesResponseType(typeof(BaseResponse<Guid>), 200)]
        public async Task<IActionResult> Update([FromBody] UpdateTermsDTO model)
        {
            try
            {
                return ReturnResponse(await _termsService.UpdateTermsAndCondition(UserId, CurrentDateTime, model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }            
        }
    }
}
