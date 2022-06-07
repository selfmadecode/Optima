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
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ReceiptController : BaseController
    {
        private readonly IReceiptService _receiptService;
        private readonly ILog _logger;

        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
            _logger = LogManager.GetLogger(typeof(ReceiptController));
            
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Create(CreateReceiptDTO model)
        {
            try
            {
                var result = await _receiptService.CreateReceipt(model, UserId);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<ReceiptDTO>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _receiptService.GetAllReceipt();

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
        public async Task<IActionResult> Update(UpdateReceiptDTO model)
        {
            try
            {
                var result = await _receiptService.UpdateReceipt(model);

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
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _receiptService.DeleteReceipt(id);

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
