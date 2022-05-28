using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.CardDTO;
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
    public class CardController : BaseController
    {
        private readonly ICardService _cardService;

        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<CreatedCardDTO>), 200)] // return the created model
        public async Task<IActionResult> Create([FromForm]CreateCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.CreateCard(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)] // return the created model
        public async Task<IActionResult> Visa([FromBody] ConfigureVisaCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.ConfigureVisaCard(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
          
        

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)] // return the created model
        public async Task<IActionResult> ReceiptType([FromBody] ConfigureReceiptTypeCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.ConfigureReceiptTypeCard(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
           
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)] // return the created model
        public async Task<IActionResult> Normal([FromBody] ConfigureNormalCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.ConfigureNormalCard(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
           
        }
    }
}
