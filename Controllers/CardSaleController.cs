using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.CardTransactionDTOs;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CardSaleController : BaseController
    {
        private readonly ICardSaleService _cardSaleService;

        public CardSaleController(ICardSaleService cardSaleService)
        {
            _cardSaleService = cardSaleService;
        }

        /// <summary>
        /// CREATES A CARD FOR SALE; ITS VALIDATES AGAINST THE CONFIGURED CARD TYPE DENOMINATION ID,
        /// <see cref="SellCardDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Create([FromForm] SellCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardSaleService.CreateCardSales(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET A CARD SALE
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CardTransactionDTO), 200)]
        public async Task<IActionResult> Get([FromBody] GetTransactionByIdDTO model)
        {
            try
            {
                return ReturnResponse(await _cardSaleService.GetCardSale(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// UPDATE A USER CARD CODES BEING CREATED AT <see cref="Create(SellCardDTO)"/>
        /// <see cref="UpdateSellCardDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{transactionId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> UpdateCardCodes(Guid transactionId, [FromBody] UpdateSellCardDTO model)   
        {
            try
            {
                return ReturnResponse(await _cardSaleService.UpdateCardSales(transactionId, model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// ACTION ON A USER CARD SALE BEING CREATED AT <see cref="Create(SellCardDTO)"/>
        /// (OPTIMA ADMIN) CAN EITHER APPROVE, DECLINE OR REJECT THE TRANSACTION.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{transactionId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        public async Task<IActionResult> Action(Guid transactionId, [FromBody] UpdateCardTransactionStatusDTO model) 
        {
            try
            {
                return ReturnResponse(await _cardSaleService.UpdateCardTransactionStatus(transactionId, model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GETS A USER AND HIS CARD SALE TRANSACTIONS
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<CardTransactionDTO>>), 200)]
        public async Task<IActionResult> CardSaleTransaction([FromQuery] BaseSearchViewModel model, Guid userId)    
        {
            try
            {
                return ReturnResponse(await _cardSaleService.GetUserCardTransactions(model, userId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GETS TRANSACTION BY STATUS
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        [ProducesResponseType(typeof(BaseResponse<PagedList<AllTransactionDTO>>), 200)]
        public async Task<IActionResult> Pending([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _cardSaleService.GetPendingTransaction(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GETS TRANSACTION BY STATUS
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        [ProducesResponseType(typeof(BaseResponse<PagedList<AllTransactionDTO>>), 200)]
        public async Task<IActionResult> Approved([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _cardSaleService.GetApproved_PartialApproved_Transaction(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        [ProducesResponseType(typeof(BaseResponse<PagedList<AllTransactionDTO>>), 200)]
        public async Task<IActionResult> Declined([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _cardSaleService.GetDeclinedTransaction(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        //GET LOGGED-IN USER RECENT TRANSACTION
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<AllTransactionDTO>>), 200)]
        public async Task<IActionResult> Recent()
        {
            try
            {
                return ReturnResponse(await _cardSaleService.GetUserRecentTransactions(UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<AllTransactionDTO>>), 200)]
        public async Task<IActionResult> SeeAll()
        {
            try
            {
                return ReturnResponse(await _cardSaleService.SeeAllTransactions(UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
