using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.FaqsDTO;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FaqController : BaseController
    {
        private readonly IFaqService _faqService;

        public FaqController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Create([FromBody] CreateFaqDTO model)
        {
            try
            {
                return ReturnResponse(await _faqService.Create(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(BaseResponse<FaqDTO>), 200)]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                return ReturnResponse(await _faqService.Get(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<FaqDTO>>), 200)]
        public async Task<IActionResult> Get()
        {
            try
            {
                return ReturnResponse(await _faqService.Get());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFaqDTO model)
        {
            try
            {
                return ReturnResponse(await _faqService.Update(id, model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                return ReturnResponse(await _faqService.Delete(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }
    }
}
