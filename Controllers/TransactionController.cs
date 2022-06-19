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


        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<TransactionDTO>>), 200)]
        public async Task<IActionResult> UserCreditDebit([FromQuery]BaseSearchViewModel model, Guid userId) 
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

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedList<TransactionDTO>>), 200)]
        public async Task<IActionResult> AllUserCreditDebit([FromQuery]BaseSearchViewModel model)
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
        //
        // retrieve a user transaction, filter by credit, debit, pending, declined, approved
    }
}
