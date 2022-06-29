using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.ReceiptDTOs;
using Optima.Services.Implementation;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReceiptController : BaseController
    {
        private readonly IReceiptService _receiptService;

        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;            
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Create(CreateReceiptDTO model)
        {
            try
            {
                return ReturnResponse(await _receiptService.CreateReceipt(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<ReceiptDTO>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return ReturnResponse(await _receiptService.GetAllReceipt());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Update(UpdateReceiptDTO model)
        {
            try
            {
                return ReturnResponse(await _receiptService.UpdateReceipt(model));
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
                return ReturnResponse(await _receiptService.DeleteReceipt(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
