using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.BankAccountDTOs;
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

        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
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
                return HandleError(ex);
            }

        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(BaseResponse<List<BankAccountDTO>>), 200)]

        public async Task<IActionResult> GetUserBankAccount(Guid userId)
        {
            try
            {
                var result = await _bankAccountService.GetUserBankAccounts(userId);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
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
                return HandleError(ex);
            }

        }
    }
}
