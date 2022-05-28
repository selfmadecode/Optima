using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.BankAccountDTO;
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
    public class BankAccountController : BaseController
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly ILog _logger;

        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
            _logger = LogManager.GetLogger(typeof(BankAccountController));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Create([FromBody] CreateBankAccountDTO model)
        {
            try
            {
                var result = await _bankAccountService.CreateBankAccount(model, UserId);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
               _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }
           
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<BankAccountDTO>), 200)]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _bankAccountService.GetBankAccount(id, UserId);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<BankAccountDTO>>), 200)]

        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _bankAccountService.GetAllBankAccount(UserId);

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
        public async Task<IActionResult> Update([FromBody]UpdateBankAccountDTO model, Guid UserId) 
        {
            try
            {
                var result = await _bankAccountService.UpdateBankAccount(model, UserId); 

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
                var result = await _bankAccountService.DeleteBankAccount(id, UserId); 

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
