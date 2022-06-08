using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.CardTransactionDTOs;
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
                var result = await _cardSaleService.CreateCardSales(model, UserId);

                return ReturnResponse(result);
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
                var result = await _cardSaleService.GetAllCardSales(model);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        // Create CardSale

        // Return all CardSales(Filter by (approved, pending, PartialApproved, declined))

        // Approve or decline Transaction(SuperAdmin)
    }
}
