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

        /// <summary>
        /// CREATES A CARD TYPE WITH COUNTRY(COUNTRY IDs) FOR BOTH E-CODE AND PHYSICAL,
        /// A STATUS FOR THE BASECARDTYPE BEING CREATED IS ALSO SET <see cref="CreateCardDTO"/> FOR THE PAYLOAD IT ACCEPTS,
        /// FOR EXAMPLE A CARD TO BE CREATED WITH 2 COUNTRIES CREATES 4 CARD TYPES FOR BOTH PHYSICAL AND E-CODE. 
        /// I.E. COUNTRY A PHYSICAL COUNTRY A E-CODE,  COUNTRY B PHYSICAL COUNTRY B E-CODE, --> 4 CARD TYPES.
        /// ALL THIS CARD TYPES ARE WAITING TO BE CONFIGURED TO EITHER NORMAL, VISA OR RECEIPT TYPE.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// GETS ALL ACTIVE CARDS, THE STATUS "IS ACTIVE" IS SET AT THE POINT OF CREATING 
        /// THE CARD FROM THE CREATE CARD ENDPOINT <see cref="Create(CreateCardDTO)"/>.
        /// THE RESPONSE BODY CAN ALSO BE FILTERED BY THE PROPERTY CARD NAME
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GETS ALL IN-ACTIVE CARDS, 
        /// THE STATUS ACTIVE AND IS ACTIVE IS SET BY THE ADMIN AT THIS ENDPOINT<see cref="UpdateCardStatus(Guid, UpdateCardStatusDTO)"/>
        /// IT CAN BE FILTERED BY THE PROPERTY CARD NAME
        /// THE RESPONSE BODY CAN ALSO BE FILTERED BY THE PROPERTY CARD NAME
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// ADMIN CAN ACTIVATE AND DEACTIVATE THE STATUS OF A CARD <see cref="UpdateCardDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// CONFIGURES A CARD TYPE BEING CREATED AT <see cref="Create(CreateCardDTO)"/> 
        /// TO A VISA TYPE <see cref="ConfigureVisaCardDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// CONFIGURES A CARD TYPE BEING CREATED AT <see cref="Create(CreateCardDTO)"/> 
        /// TO A RECEIPT TYPE <see cref="ConfigureReceiptTypeCardDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// CONFIGURES A CARD TYPE BEING CREATED AT <see cref="Create(CreateCardDTO)"/>
        /// TO A NORMAL TYPE <see cref="ConfigureNormalCardDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GETS A CARD AND INCLUDES ITS CARDTYPE AND CARDTYPE DENOMINATION I.E IF THE CARDTYPE DENOMINATION HAS PREFIX CONFIGURED,
        /// THE PREFIX WOULD ALSO BE INCLUDED AS PART OF THE RESPONSE, ELSE IT WOULDN'T. ETC.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GETS ALL CARD AND INCLUDES ITS CARD TYPE AND CARD TYPE DENOMINATION.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GETS ALL PENDING CARD AND INCLUDES ITS  CARD TYPE AND CARD TYPE DENOMINATION.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GETS ALL APPROVED CARD AND INCLUDES ITS CARD TYPE AND CARD TYPE DENOMINATION.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// ADD COUNTRIES TO ALREADY EXISTING CARD, I.E. CREATES NEW CARD TYPE, IF DOESN'T EXIST.
        /// THIS CREATES NEW CARD TYPES FOR THE NEWLY ADDED COUNTRIES. <see cref="UpdateCardDTO">FOR THE PAYLOAD.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// THIS UPDATES NORMAL CARD TYPE I.E A CASE WHEN THE ADMIN WANTS TO UPDATE THE DENOMINATION AS IN THE CASE FOR A NORMAL CARD ALREADY CONFIGURED
        /// OR CREATES A NEW CARD TYPE DENOMINATION IF IT DOESN'T EXIST. <see cref="UpdateNormalCardConfigDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// THIS UPDATES RECEIPT CARD TYPE I.E A CASE WHEN THE ADMIN WANTS TO UPDATE THE DENOMINATION OR RECEIPT TYPE 
        /// AS IN THE CASE FOR A RECEIPT TYPE CARD ALREADY CONFIGURED
        /// OR CREATES A NEW CARD TYPE DENOMINATION IF IT DOESN'T EXIST. <see cref="UpdateReceiptTypeConfigDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// THIS UPDATES VISA CARD TYPE I.E A CASE WHEN THE ADMIN WANTS TO UPDATE THE DENOMINATION OR VISA PREFIX TYPE 
        /// AS IN THE CASE FOR A VISA CARD TYPE ALREADY CONFIGURED
        /// OR IT CREATES A NEW CARD TYPE DENOMINATION IF IT DOESN'T EXIST. <see cref="UpdateVisaCardConfigDTO"/> FOR THE PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// DELETES A CARD TYPE OF A CARD 
        /// NOTE:THIS CAN ONLY DELETE THE CARD TYPE IF IT HASN'T BEEN CONFIGURED. <see cref="DeleteCardTypeDTO"/> FOR THe PAYLOAD IT ACCEPTS.
        /// </summary>
        /// <param name="CardId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
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
