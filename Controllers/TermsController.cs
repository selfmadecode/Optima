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
    [Authorize]
    public class TermsController : BaseController
    {
        private readonly ITermsService _termsService;

        public TermsController(ITermsService termsService)
        {
            _termsService = termsService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Accept()
        {
            return ReturnResponse(await _termsService.AcceptTermsAndCondition(UserId));
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<GetRateDTO>), 200)]
        public async Task<IActionResult> Get()
        {
            return ReturnResponse(await _termsService.GetTermsAndCondition());
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResponse<Guid>), 200)]
        public async Task<IActionResult> Update([FromBody] UpdateTermsDTO model)
        {
            return ReturnResponse(await _termsService.UpdateTermsAndCondition(UserId, model));
        }
    }
}
