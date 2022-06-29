using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.TransactionDTO;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TransactionController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// GETS A USER BANK ACCOUNT BALANCE
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<BalanceInquiryDTO>), 200)]
        public async Task<IActionResult> Balance()
        {
            try
            {
                return ReturnResponse(await _transactionService.GetUserAccountBalance(UserId));
            }
            catch (Exception ex)
            {

                return HandleError(ex);
            }
        }

        /// <summary>
        /// USER WITHDRAW FROM BANK ACCOUNT
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Withdraw(WithdrawDTO model)
        {
            try
            {
                return ReturnResponse(await _transactionService.Withdraw(model, UserId));
            }
            catch (Exception ex)
            {

                return HandleError(ex);
            }
        }

        /// <summary>
        /// GETS A USER CREDIT DEBIT TRANSACTION
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<TransactionDTO>>), 200)]
        public async Task<IActionResult> CreditDebit([FromQuery]BaseSearchViewModel model, Guid userId) 
        {
            try
            {
                return ReturnResponse(await _transactionService.GetUserCreditDebit(model, userId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET ALL USERS CREDIT DEBIT TRANSACTION
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedList<TransactionDTO>>), 200)]
        public async Task<IActionResult> CreditDebit([FromQuery] BaseSearchViewModel model) 
        {
            try
            {
                return ReturnResponse(await _transactionService.GetAllUserCreditDebit(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// UPDATE DEBIT STATUS I.E. APPROVE OR REJECT A USER DEBIT REQUEST
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{creditDebitId}")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<TransactionDTO>>), 200)]
        public async Task<IActionResult> Action(Guid creditDebitId, [FromBody]UpdateDebitStatus model)
        {
            try
            {
                return ReturnResponse(await _transactionService.UpdateDebitStatus(creditDebitId, model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
       
    }
}
