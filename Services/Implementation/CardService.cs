using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.DTO.CardDTO;
using Optima.Models.DTO.CountryDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
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
            }

            var card = _dbContext.Cards.FirstOrDefault(x => x.Name.ToLower() == model.Name.ToLower());

            if (card != null)
            {
                result.ResponseMessage = "Card name already exist";
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
            return result;
        }

        
        public Task<BaseResponse<bool>> ConfigureNormalCard(ConfigureNormalCardDTO model, Guid UserId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponse<bool>> ConfigureReceiptTypeCard(ConfigureReceiptTypeCardDTO model, Guid UserId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<bool>> ConfigureVisaCard(ConfigureVisaCardDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            //Validate Card Config
            var validateCardConfig = 
                await ValidateCardConfig(model.CardId, model.VisaCardConfigDTO.Select(x => x.CountryId).ToList(), model.VisaCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                response.ResponseMessage = validateCardConfig.ResponseMessage;
                response.Errors = validateCardConfig.Errors;
                return response;

            }

            //Validate Denomination
            var validateDenomination = ValidateDenomination(model.VisaCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                response.ResponseMessage = "Denomination doesn't exists";
                response.Errors = new List<string> { "Denomination doesn't exists" };
                return response;
            }

            //Validate Prefix
            var validatePrefix = ValidatePrefix(model.VisaCardConfigDTO.Select(x => x.PrefixId).ToList());

            if (validatePrefix)
            {
                response.ResponseMessage = "Prefix doesn't exists";
                response.Errors = new List<string> { "Prefix doesn't exists" };
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
                    PrefixId = item.PrefixId
                });
                         
            }
            _dbContext.CardTypeDenomination.AddRange(cardTypeDenominations);
            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Created";

            return response;

        }

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


        private async Task<BaseResponse<bool>> ValidateCardConfig(Guid CardId, List<Guid> CountryIds, List<Guid> CardTypeIds) 
        {
            var card = await _dbContext.Cards
                .Include(x => x.CardType)
                .Where(x => x.Id == CardId)
                .FirstOrDefaultAsync();

            //CheckCardTypes
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

        private bool ValidateDenomination(List<Guid> denominationIds)
        {
            var denominations = _dbContext.Denominations.Where(x => denominationIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

            if (denominations.Count() != denominationIds.Distinct().Count())
            {
                return true;
            }

            return false;
        }

        private bool ValidatePrefix(List<Guid> prefixIds)
        {
            var prefixs = _dbContext.VisaPrefixes.Where(x => prefixIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

            if (prefixs.Count() != prefixIds.Distinct().Count())
            {
                return true;
            }

            return false;
        }

    }
}
