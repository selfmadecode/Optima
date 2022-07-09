using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.CardDTO;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Threading.Tasks;
using static Optima.Utilities.Helpers.PermisionProvider;

namespace Optima.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = nameof(Permission.CARD))]
    public class CardController : BaseController
    {
        private readonly ICardService _cardService;

        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }


        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(BaseResponse<CreatedCardDTO>), 200)] 
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
        
        [HttpGet]
        [Route("Active")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<CardDTO>>), 200)]
        public async Task<IActionResult> AllActiveCards([FromQuery]BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _cardService.AllActiveCards(model)); 
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [Route("Inactive")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<CardDTO>>), 200)]
        public async Task<IActionResult> AllInActiveCards([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _cardService.AllInActiveCards(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPut]
        [Route("Activate-Deactivate/{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)] 
        public async Task<IActionResult> UpdateCardStatus(Guid CardId, [FromBody]UpdateCardStatusDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.CardStatusUpdate(model, UserId, CardId)); 
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost]
        [Route("Configure-Visa/{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)] 
        public async Task<IActionResult> Visa(Guid CardId, [FromBody] ConfigureVisaCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.ConfigureVisaCard(model, UserId, CardId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }       
        
        [HttpPost]
        [Route("Configure-ReceiptType/{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)] 
        public async Task<IActionResult> ReceiptType(Guid CardId, [FromBody] ConfigureReceiptTypeCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.ConfigureReceiptTypeCard(model, UserId, CardId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
           
        }

        [HttpPost]
        [Route("Configure-Normal/{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)] 
        public async Task<IActionResult> Normal(Guid CardId, [FromBody] ConfigureNormalCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.ConfigureNormalCard(model, UserId, CardId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
           
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(BaseResponse<CardDTO>), 200)] 
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                return ReturnResponse(await _cardService.GetCard(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<CardDTO>>), 200)] 
        public async Task<IActionResult> GetAllCard([FromQuery]BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _cardService.GetAllCard(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpGet]
        [Route("Pending")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<CardDTO>>), 200)] 
        public async Task<IActionResult> GetAllPendingCard([FromQuery] BaseSearchViewModel model) 
        {
            try
            {
                return ReturnResponse(await _cardService.GetAllPendingCardConfig(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpGet]
        [Route("Approved")]
        [ProducesResponseType(typeof(BaseResponse<PagedList<CardDTO>>), 200)]
        public async Task<IActionResult> GetAllApprovedCard([FromQuery] BaseSearchViewModel model) 
        {
            try
            {
                return ReturnResponse(await _cardService.GetAllApprovedCardConfig(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex); 
            }
        }

        [HttpPut]
        [Route("Add-Countries/{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> AllCountriesToCard(Guid CardId, [FromForm] UpdateCardDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.UpdateCard(model, UserId, CardId));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpPut]
        [Route("Update-Normal/{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> UpdateNormalCard(Guid CardId, [FromBody] UpdateNormalCardConfigDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.UpdateNormalCard(model, UserId, CardId));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpPut]
        [Route("Update-ReceiptType{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> UpdateReceiptCard(Guid CardId, [FromBody] UpdateReceiptTypeConfigDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.UpdateReceiptCard(model, UserId, CardId));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpPut]
        [Route("Update-Visa/{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> UpdateVisaCard(Guid CardId, [FromBody] UpdateVisaCardConfigDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.UpdateVisaCard(model, UserId, CardId));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }


        [HttpDelete]
        [Route("CardType/{CardId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> DeleteCardType(Guid CardId, [FromBody] DeleteCardTypeDTO model)
        {
            try
            {
                return ReturnResponse(await _cardService.DeleteCardType(model, CardId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
