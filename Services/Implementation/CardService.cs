﻿using AzureRays.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.DTO.CardDTO;
using Optima.Models.DTO.CountryDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class CardService : ICardService
    {
        private readonly ApplicationDbContext _dbContext;

        public CardService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        /// <summary>
        /// CREATE CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<CreatedCardDTO>> CreateCard(CreateCardDTO model, Guid UserId)
        {
            var result = new BaseResponse<CreatedCardDTO>();
            // TODO
            // WRAP ALL THIS IN A TRANSACTION

            // Todo

            // verify card name
            // upload card to cloud and get the url
            // validate that a countryId didnt appear twice -- should we return an error message or make it distinct


            // foreach countryId in the model
            // create two card (ecode, physical)

            // verify country Id(s).
            var countryValidation = ValidateCountry(model.CountryIds);

            if (countryValidation.Errors.Any())
            {
                result.ResponseMessage = countryValidation.ResponseMessage;
                result.Errors = countryValidation.Errors;
                result.Status = RequestExecution.Failed;
                return result;
            }

            var card = _dbContext.Cards.FirstOrDefault(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

            if (card != null)
            {
                result.ResponseMessage = "Card name already exist";
                result.Errors.Add("Card name already exist");
                result.Status = RequestExecution.Failed;
                return result;
            }

            var newCard = new Card()
            {
                Name = model.Name,
                LogoUrl = "",
                CardStatus = CardStatus.Pending,
                CreatedBy = UserId
            };

            var cardTypes = CreateCardTypes(countryValidation.Data.CountryIds, UserId);
            newCard.CardType.AddRange(cardTypes);

            await _dbContext.Cards.AddAsync(newCard);
            _dbContext.SaveChanges();

            result.Data = new CreatedCardDTO { Id = newCard.Id, Name = newCard.Name };
            result.ResponseMessage = "Card Created Successfully";
            return result;
        }


        /// <summary>
        /// CONFIGURE NORMAL CARD TYPE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public Task<BaseResponse<bool>> ConfigureNormalCard(ConfigureNormalCardDTO model, Guid UserId)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// CONFIGURE RECEIPT CARDDD TYPE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public Task<BaseResponse<bool>> ConfigureReceiptTypeCard(ConfigureReceiptTypeCardDTO model, Guid UserId)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// CONFIGURE VISA CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> ConfigureVisaCard(ConfigureVisaCardDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            var card = await FindCard(model.CardId);

            if (card is null)
            {
                response.ResponseMessage = "Card doesn't exists.";
                response.Status = RequestExecution.Failed;
                response.Errors.Add("Card doesn't exists.");
                return response;
            }

          
            //Validate Card Config
            var validateCardConfig = 
                await ValidateCardConfig(model.CardId, model.VisaCardConfigDTO.Select(x => x.CountryId).ToList(), model.VisaCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                response.ResponseMessage = validateCardConfig.ResponseMessage;
                response.Errors = validateCardConfig.Errors;
                response.Status = RequestExecution.Failed;
                return response;

            }

            //Check if CardType has already been configured
            var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => model.VisaCardConfigDTO.Select(x => x.CardTypeId).Contains(x.CardTypeId)).ToListAsync();

            if (checkCardConfig.Any())
            {
                response.ResponseMessage = "Card Type has already been configured";
                response.Errors.Add("Card Type has already been configured");
                response.Status = RequestExecution.Failed;
                return response;
            };


            //Validate Denomination
            var validateDenomination = ValidateDenomination(model.VisaCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                response.ResponseMessage = "Denomination doesn't exists";
                response.Errors = new List<string> { "Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Validate Prefix
            var validatePrefix = ValidatePrefix(model.VisaCardConfigDTO.Select(x => x.PrefixId).ToList());

            if (validatePrefix)
            {
                response.ResponseMessage = "Prefix doesn't exists";
                response.Errors = new List<string> { "Prefix doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Add CardType Denomination
            var cardTypeDenominations = new List<CardTypeDenomination>();
            foreach (var item in model.VisaCardConfigDTO)
            {
                
                cardTypeDenominations.Add(new CardTypeDenomination
                {
                    CardTypeId = item.CardTypeId,
                    DenominationId = item.DenominationId,
                    Rate = item.Rate,
                    PrefixId = item.PrefixId,
                    CreatedBy = UserId
                });
                         
            }

            card.CardStatus = CardStatus.Approved;
            _dbContext.CardTypeDenomination.AddRange(cardTypeDenominations);
            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Configured Visa Card";

            return response;
        }


        /// <summary>
        /// CREATE CARD TYPES
        /// </summary>
        /// <param name="CountryIds">The countryIds.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;ValidateCountryDTO&gt;&gt;.</returns>
        private List<CardType> CreateCardTypes(List<Guid> CountryIds, Guid UserId)
        {
            List<CardType> cardTypes = new List<CardType>();

            foreach (var countryId in CountryIds)
            {

                var newCardEcodeType = new CardType
                {
                    CountryId = countryId,
                    CardCategory = CardCategory.E_CODE,
                    CreatedBy = UserId
                };

                cardTypes.Add(newCardEcodeType);

                var newCardPhysicalType = new CardType
                {
                    CountryId = countryId,
                    CardCategory = CardCategory.PHYSICAL,
                    CreatedBy = UserId
                };
                // Add Log

                cardTypes.Add(newCardPhysicalType);
            }

            return cardTypes;
        }


        /// <summary>
        /// VALIDATE COUNTRY
        /// </summary>
        /// <param name="countryIds">The countryIds.</param>
        /// <returns>Task&lt;BaseResponse&lt;ValidateCountryDTO&gt;&gt;.</returns>
        private BaseResponse<ValidateCountryDTO> ValidateCountry(List<Guid> countryIds)
        {
            var countries = _dbContext.Countries.Where(x => countryIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);
            var data = new ValidateCountryDTO();

            if (countryIds.Count != countries.Distinct().Count())
            {
                return new BaseResponse<ValidateCountryDTO>
                {
                    ResponseMessage = "Country doesn't Exist",
                    Errors = new List<string> { "Country doesn't Exist" }
                };

            }
            else
            {
                data.CountryIds = countries.ToList();
                return new BaseResponse<ValidateCountryDTO>
                {
                    Data = data,
                };
            }
        }

        /// <summary>
        /// VALIDATE CARD CONFIG
        /// </summary>
        /// <param name="CardId">The CardId.</param>
        /// <param name="CountryIds">The countryIds.</param>
        /// <param name="CardTypeIds">The CardTypeIds.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        private async Task<BaseResponse<bool>> ValidateCardConfig(Guid CardId, List<Guid> CountryIds, List<Guid> CardTypeIds) 
        {
            var card = await _dbContext.Cards
                .Include(x => x.CardType)
                .Where(x => x.Id == CardId)
                .FirstOrDefaultAsync();

            //Check CardTypes
            var cardTypes = card.CardType.Where(x => CardTypeIds.Contains(x.Id)).ToList();

            if (cardTypes.Count != CardTypeIds.Count())
                return new BaseResponse<bool>
                {
                    ResponseMessage = "Card Type doesn't exists for this Card",
                    Errors = new List<string> { "Card Type doesn't exists for this Card" }
                };
           

            //Check Country in CardType
            var countryCardType = cardTypes.Where(x => CountryIds.Contains(x.CountryId)).ToList();
            if (countryCardType.Count != CountryIds.Distinct().Count())
                return new BaseResponse<bool>
                {
                    ResponseMessage = "Country doesn't exists for this Card Type",
                    Errors = new List<string> { "Country doesn't exists for this Card Type" }
                };

            return new BaseResponse<bool> { };

        }


        /// <summary>
        /// VALIDATE DENOMINATION
        /// </summary>
        /// <param name="denominationIds">The denominationIds.</param>
        /// <returns>System.boolean</returns>
        private bool ValidateDenomination(List<Guid> denominationIds)
        {
            var denominations = _dbContext.Denominations.Where(x => denominationIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

            if (denominations.Count() != denominationIds.Distinct().Count())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// VALIDATE PREFIX
        /// </summary>
        /// <param name="prefixIds">The prefixIds.</param>
        /// <returns>System.boolean</returns>
        private bool ValidatePrefix(List<Guid> prefixIds)
        {
            var prefixs = _dbContext.VisaPrefixes.Where(x => prefixIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

            if (prefixs.Count() != prefixIds.Distinct().Count())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// GET CARD
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;CardDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<CardDTO>> GetCard(Guid id)
        {
            var response = new BaseResponse<CardDTO>();

            var card = await _dbContext.Cards.Where(x => x.Id == id)
                .Include(x => x.CardType).ThenInclude(x => x.Country)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .FirstOrDefaultAsync();

            if (card is null)
            {
                response.ResponseMessage = "Card doesn't Exists";
                response.Errors.Add("Card doesn't Exists");
                response.Status = RequestExecution.Failed;
                return response;
            }

            var cardTypesDTO = card.CardType.OrderByDescending(x => x.CreatedOn).Select(x => (CardTypeDTO)x).ToList();

            CardDTO cardDto = card;
            cardDto.CardTypeDTOs = cardTypesDTO;

            response.Data = cardDto;
            response.ResponseMessage = "Success";

            return response;
        }


        /// <summary>
        /// GET ALL CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;CardDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> GetAllCard(BaseSearchViewModel model) 
        {
            //TO-DO RETURNS BOTH PENDING AND APPROVED.
            var cards = _dbContext.Cards.AsNoTracking()
                .Where(x => x.CardStatus == CardStatus.Approved)
                .Include(x => x.CardType).ThenInclude(x => x.Country)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .AsQueryable();

            var pagedCards = await cards.OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, ResponseMessage = $"Found {cardsDto.Count} Card(s)." };

        }



        /// <summary>
        /// GET ALL PENDING CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CardDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> GetAllPendingCardConfig(BaseSearchViewModel model)
        {

            var cards = _dbContext.Cards.AsNoTracking()
                .Where(x => x.CardStatus == CardStatus.Pending)
                .Include(x => x.CardType).ThenInclude(x => x.Country)
                .AsQueryable();

            var pagedCards = await cards.OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, ResponseMessage = $"Found {cardsDto.Count} Card(s)." };
        }


        /// <summary>
        /// FIND CARD
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private async Task<Card> FindCard(Guid id) =>
            await _dbContext.Cards.FirstOrDefaultAsync(x => x.Id == id); 
    }
}
