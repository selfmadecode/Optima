using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.BankAccountDTO;
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
    [Authorize]
    public class BankAccountController : BaseController
    {
        private readonly IBankAccountService _bankAccountService;
        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBankAccount([FromBody] List<CreateBankAccountDTO> model)
        {
            try
            {
                var result = await _bankAccountService.CreateBankAccount(model, UserId);

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
               // _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }
           
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBankAccount(Guid id)
        {
            try
            {
                var result = await _bankAccountService.GetBankAccount(id, UserId);

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateBankAccount([FromBody]UpdateBankAccountDTO model, Guid UserId) 
        {
            try
            {
                var result = await _bankAccountService.UpdateBankAccount(model, UserId); 

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBankAccount(Guid id, Guid UserId) 
        {
            try
            {
                var result = await _bankAccountService.DeleteBankAccount(id, UserId); 

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }
    }
}
