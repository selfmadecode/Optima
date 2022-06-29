using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.CardTransactionDTOs;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
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

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy = "CanAdd")]
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

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedList<CardTransactionDTO>>), 200)]
        //[Authorize(Policy = "CanAdd")]
        public async Task<IActionResult> GetAll([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _cardSaleService.GetAllCardSales(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy = "CanAdd")]
        public async Task<IActionResult> UpdateCardCode([FromBody] UpdateSellCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardSaleService.UpdateCardSales(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        //[Authorize(Policy = "CanAdd")]
        public async Task<IActionResult> UpdateCardTransactionStatus([FromBody] UpdateCardTransactionStatusDTO model)
        {
            try
            {
                return ReturnResponse(await _cardSaleService.UpdateCardTransactionStatus(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<CardTransactionDTO>>), 200)]
        //[Authorize(Policy = "CanAdd")]
        public async Task<IActionResult> GetUserCardTransactions([FromQuery] BaseSearchViewModel model, Guid userId) 
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


    }
}
