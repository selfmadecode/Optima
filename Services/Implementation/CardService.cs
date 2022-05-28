using Optima.Context;
using Optima.Models.DTO.CardDTO;
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

        public Task<BaseResponse<bool>> ConfigureNormalCard(ConfigureNormalCardDTO model, Guid UserId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponse<bool>> ConfigureReceiptTypeCard(ConfigureReceiptTypeCardDTO model, Guid UserId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponse<bool>> ConfigureVisaCard(ConfigureVisaCardDTO model, Guid UserId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<CreatedCardDTO>> CreateCard(CreateCardDTO model, Guid UserId)
        {
            var result = new BaseResponse<CreatedCardDTO>();
            // TODO
            // WRAP ALL THIS IN A TRANSACTION

            // Todo
            // verify country Id
            // verify card name
            // upload card to cloud and get the url
            // validate that a countryId didnt appear twice -- should we return an error message or make it distinct


            // foreach countryId in the model
            // create two card (ecode, physical)

            // Create the Card
            var card = _dbContext.Cards.FirstOrDefault(x => x.Name.ToLower() == model.Name.ToLower());

            if(card != null)
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

            var cardTypes = CreateCardTypes(model.CountryIds, UserId);
            newCard.CardType.AddRange(cardTypes);           

            await _dbContext.Cards.AddAsync(newCard);
            _dbContext.SaveChanges();
            return result;
        }

        public List<CardType> CreateCardTypes(List<Guid> CountryIds, Guid UserId)
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

    }
}
