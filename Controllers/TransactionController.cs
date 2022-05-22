using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.TransactionDTO;
using Optima.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]")]
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
        [ProducesResponseType(typeof(BalanceInquiryDTO), 200)]
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
    }
}
