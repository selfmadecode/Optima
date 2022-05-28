using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.ReceiptDTO;
using Optima.Services.Implementation;
using Optima.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
        public async Task<IActionResult> Create(CreateReceiptDTO model)
        {
            try
            {
                var result = await _receiptService.CreateReceipt(model);

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
