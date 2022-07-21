using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.CardDTO;
using Optima.Models.DTO.CountryDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class CardService : BaseService, ICardService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly ILog _logger;

        public CardService(ApplicationDbContext dbContext,
            ICloudinaryServices cloudinaryServices)
        {
            _dbContext = dbContext;
            _cloudinaryServices = cloudinaryServices;
            _logger = LogManager.GetLogger(typeof(ICardService));
        }


        /// <summary>
        /// GET ALL ACTIVE CARDS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CardDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> AllActiveCards(BaseSearchViewModel model)
        {
            var cardsQuery = _dbContext.Cards
                .Where(x => x.CardStatus == CardStatus.Approved).OrderByDescending(x => x.CreatedOn).AsQueryable();

            var pagedCards = await EntityFilter(cardsQuery, model).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {cardsDto.Count} CARD(s)." };
        }

        /// <summary>
        /// GET ALL IN ACTIVE CARDS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CardDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> AllInActiveCards(BaseSearchViewModel model)
        {
            var cardsQuery = _dbContext.Cards
                .Where(x => x.CardStatus == CardStatus.Pending).OrderByDescending(x => x.CreatedOn).AsQueryable();

            var pagedCards = await EntityFilter(cardsQuery, model).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {cardsDto.Count} CARD(s)." };
        }

        /// <summary>
        /// GET ALL BLOCKED CARDS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CardDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> AllBlockedCards(BaseSearchViewModel model)
        {
            var cardsQuery = _dbContext.Cards
                 .Where(x => x.CardStatus == CardStatus.Blocked).OrderByDescending(x => x.CreatedOn).AsQueryable();

            var pagedCards = await EntityFilter(cardsQuery, model).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {cardsDto.Count} CARD(s)." };
        }

        /// <summary>
        /// CARD STATUS UPDATE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CardStatusUpdate(UpdateCardStatusDTO model, Guid UserId, Guid CardId)
        {
            //VALIDATES THE CARD ID.
            var card = await FindCard(CardId);

            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
            }

            switch (model.CardStatus)
            {
                case CardStatus.Pending:
                    {
                        card.CardStatus = CardStatus.Pending;
                        card.ModifiedBy = UserId;
                        card.ModifiedOn = DateTime.UtcNow;
                    }
                    break;
                case CardStatus.Approved:
                    {
                        card.CardStatus = CardStatus.Approved;
                        card.ModifiedBy = UserId;
                        card.ModifiedOn = DateTime.UtcNow;
                    }
                    break;
                case CardStatus.Blocked:
                    {
                        card.CardStatus = CardStatus.Blocked;
                        card.ModifiedBy = UserId;
                        card.ModifiedOn = DateTime.UtcNow;
                    }
                    break;
                default:
                    break;
            }

            _dbContext.Cards.Update(card);
            await _dbContext.SaveChangesAsync();
            return new BaseResponse<bool>(true, ResponseMessage.UpdateCardStatus);
        }

        /// <summary>
        /// CREATE CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<CreatedCardDTO>> CreateCard(CreateCardDTO model, Guid UserId)
        {
            var uploadedFileToDelete = string.Empty;

            try
            {
                //VALIDATES COUNTRY IDs
                var countryValidation = ValidateCountry(model.CountryIds);

                if (countryValidation.Errors.Any())
                {
                    return new BaseResponse<CreatedCardDTO>(countryValidation.ResponseMessage, countryValidation.Errors);
                }

                //CHECK IF CARD DOESN'T ALREADY EXISTS.
                var card = _dbContext.Cards
                    .FirstOrDefault(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

                if (card != null)
                {
                    Errors.Add(ResponseMessage.CardExist);
                    return new BaseResponse<CreatedCardDTO>(ResponseMessage.CardExist, Errors);
                }

                //UPLOAD TO CLOUDINARY
                _logger.Info("Uploading Image to Cloudinary... at ExecutionPoint:CreateCard");
                var (uploadedFile, hasUploadError, responseMessage) = 
                    _cloudinaryServices.UploadImage(model.Logo).Result;

                //SET THIS PROPERTY TO DELETE THE UPLOADED FILE IF AN EXCEPTION OCCUR
                uploadedFileToDelete = uploadedFile;

                // IF ERROR OCCURRED WHILE UPLOADING THE FILE TO CLOUDINARY
                if (hasUploadError == true)
                {
                    _logger.Info("Error uploading file to Cloudinary... at ExecutionPoint:CreateCard");

                    Errors.Add(ResponseMessage.ErrorMessage999);
                    return new BaseResponse<CreatedCardDTO>(ResponseMessage.ErrorMessage999, Errors);
                }

                _logger.Info("Successfully Uploaded to Cloudinary... at ExecutionPoint:CreateCard");


                var newCard = new Card()
                {
                    Name = model.Name,
                    LogoUrl = uploadedFile,
                    CreatedBy = UserId,
                    CardStatus = CardStatus.Pending,
                    BaseCardType = model.BaseCardType
                };

                // CREATED E-CODE AND PHYSICAL CARD TYPE FOR THE COUNTRIES.
                var cardTypes = CreateCardTypes(countryValidation.Data.CountryIds, UserId);

                newCard.CardType.AddRange(cardTypes);

                await _dbContext.Cards.AddAsync(newCard);

                _logger.Info("About to save Card and CardsType... at ExecutionPoint:CreateCard");
                await _dbContext.SaveChangesAsync();
                _logger.Info("Saved Card and CardsType... at ExecutionPoint:CreateCard");

                var data = new CreatedCardDTO { Id = newCard.Id, Name = newCard.Name };

                return new BaseResponse<CreatedCardDTO>(data, ResponseMessage.CardCreation);
            }
            catch (Exception ex)
            {
                await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(uploadedFileToDelete));
                _logger.Error(ex.Message, ex);

                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<CreatedCardDTO>(ResponseMessage.ErrorMessage999, Errors);
            }
            
        }


        /// <summary>
        /// CONFIGURE NORMAL CARD TYPE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> ConfigureNormalCard(ConfigureNormalCardDTO model, Guid UserId, Guid CardId)
        {
            var response = new BaseResponse<bool>();

            //VALIDATES THE CARD ID.
            var card = await FindCard(CardId);

            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
            }

            //VALIDATES THE CREATED CARD IF IT IS A REGULAR TYPE
            if (card.BaseCardType != BaseCardType.REGULAR)
            {
                Errors.Add(ResponseMessage.CardNotRegular);
                return new BaseResponse<bool>(ResponseMessage.CardNotRegular, Errors);
            }

            //VALIDATES COUNTRYID AGAINST CARDTYPEID
            var result = await ValidateNormalCardConfigMain(model);

            if (result.Errors.Any())
            {
                return new BaseResponse<bool>(result.ResponseMessage, result.Errors);
            }

            // VALIDATE IF CARDTYPE DEMONINATION TABLE CONTAINS ANY DATA FOR THIS CARDTYPE ID
            var data = await ValidateThatDenominationForCardDoesNotExist(model);

            if (data.Errors.Any())
            {
                return new BaseResponse<bool>(data.ResponseMessage, data.Errors);
            }

            foreach (var rates in model.NormalCardConfigDTO.Select(x => x.CardRates))
            {
                var validateDenomination = ValidateDenominationIds(rates.Select(x => x.DenominationId).ToList());

                if (validateDenomination)
                {
                    Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                }
            }

            //CREATES THE CARD TYPE DENOM INATION FOR NORMAL CARD
            await CreateNormalCardTypeDenomination(model, UserId, CardId);
           
            return new BaseResponse<bool>(true, ResponseMessage.CardConfigSuccess);
        }

        /// <summary>
        /// CREATE NORMAL CARD TYPE DENOMINATION
        /// </summary>
        /// <param name="CardConfigDTO">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threadings.Tasks.Task</returns>
        private async Task CreateNormalCardTypeDenomination(ConfigureNormalCardDTO model, Guid UserId, Guid CardId)
        {
            var cardTypeDenominations = new List<CardTypeDenomination>();
            var allCardTypes = new List<Guid>();

            foreach (var receiptCardType in model.NormalCardConfigDTO)
            {
                var CardTypeId = receiptCardType.CardTypeId;
                allCardTypes.Add(receiptCardType.CardTypeId);

                receiptCardType.CardRates.ForEach(x => cardTypeDenominations
                    .Add(new CardTypeDenomination
                    {
                        Rate = x.Rate,
                        DenominationId = x.DenominationId,
                        CardTypeId = CardTypeId,
                        CreatedBy = UserId,
                        CreatedOn = DateTime.UtcNow,
                        IsActive = true
                    }));
            }

            _dbContext.CardTypeDenomination.AddRange(cardTypeDenominations);
            //UPDATES CARD TYPE STATUS
            var card = _dbContext.Cards.FirstOrDefault(x => x.Id == CardId);
            card.CardStatus = CardStatus.Approved;


            var cardTypes = await _dbContext.CardTypes.Where(x => allCardTypes.Contains(x.Id)).ToListAsync();
            cardTypes.ForEach(x => x.CardStatus = CardStatus.Approved);

            _dbContext.SaveChanges();

            _logger.Info("About to Save CardType Denomination For NormalCard Config... at ExecutionPoint:ConfigureNormalCard");
            await _dbContext.SaveChangesAsync();
            _logger.Info("Successfully Saved CardType Denomination For NormalCard Config... at ExecutionPoint:ConfigureNormalCard");
        }


        /// <summary>
        /// CONFIGURE RECEIPT CARD TYPE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> ConfigureReceiptTypeCard(ConfigureReceiptTypeCardDTO model, Guid UserId, Guid CardId)
        {
            //VALIDATES THE CARD ID.
            var card = await FindCard(CardId);

            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);                
            }

            //VALIDATES THE CREATED CARD IF IT IS AN AMAZON TYPE
            if (card.BaseCardType != BaseCardType.AMAZON)
            {
                Errors.Add(ResponseMessage.CardNotAmazon);
                return new BaseResponse<bool>(ResponseMessage.CardNotAmazon, Errors);
            }

            //VALIDATES COUNTRYID AGAINST CARDTYPEID
            var result = await ValidateReceiptTypeCardConfigMain(model);

            if (result.Errors.Any())
            {
                return new BaseResponse<bool>(result.ResponseMessage, result.Errors);
            }

            // VALIDATE IF CARDTYPE DEMONINATION TABLE CONTAINS ANY DATA FOR THIS CARDTYPE ID
            var data = await ValidateThatDenominationForCardDoesNotExist(model);

            if (data.Errors.Any())
            {
                return new BaseResponse<bool>(data.ResponseMessage, data.Errors);
            }
            
            foreach (var rates in model.ReceiptTypeConfig.Select(x => x.CardRates))
            {
                var validateDenomination = ValidateDenominationIds(rates.Select(x => x.DenominationId).ToList());

                if (validateDenomination)
                {
                    Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                }
            }

            //VALIDATES RECEIPT TYPEID
            var validateReceipt = ValidateReceipt(model.ReceiptTypeConfig.Select(x => x.ReceiptTypeId).ToList());

            if (validateReceipt)
            {
                Errors.Add(ResponseMessage.CardReceiptNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardReceiptNotFound, Errors);
            }

            //CONFIGURE RECEIPT TYPE CARD TYPE DENOMINATION
            await CreateReceiptTypeDenomination(model, UserId, CardId);

            return new BaseResponse<bool>(true, ResponseMessage.CardConfigSuccess);
        }

        private async Task<BaseResponse<bool>> ValidateReceiptTypeCardConfigMain(ConfigureReceiptTypeCardDTO model)
        {
            foreach (var item in model.ReceiptTypeConfig)
            {
                // GET ALL CARD TYPES FOR THIS COUNTRY, THIS SHOULD RETURN THE E-CODE AND PHYSICAL VARIANT
                var cardtype = await _dbContext.CardType
                    .Where(x => x.CountryId == item.CountryId
                    && x.Id == item.CardTypeId
                    ).ToListAsync();

                if (!cardtype.Any())
                {
                    Errors.Add(ResponseMessage.CardTypeNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeNotFound, Errors);
                }                
            }

            return new BaseResponse<bool>();
        }
        private async Task<BaseResponse<bool>> ValidateNormalCardConfigMain(ConfigureNormalCardDTO model)
        {
            foreach (var item in model.NormalCardConfigDTO)
            {
                // GET ALL CARD TYPES FOR THIS COUNTRY, THIS SHOULD RETURN THE E-CODE AND PHYSICAL VARIANT
                var cardtype = await _dbContext.CardType
                    .Where(x => x.CountryId == item.CountryId
                    && x.Id == item.CardTypeId
                    ).ToListAsync();

                if (!cardtype.Any())
                {
                    Errors.Add(ResponseMessage.CardTypeNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeNotFound, Errors);
                }
            }

            return new BaseResponse<bool>();
        }
        private async Task<BaseResponse<bool>> ValidateThatDenominationForCardDoesNotExist(ConfigureReceiptTypeCardDTO model)
        {
            foreach (var item in model.ReceiptTypeConfig)
            {
                // VALIDATE IF CARDTYPE DEMONINATION CONATAINS ANY DATA FOR THIS CARDTYPE

                var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => x.CardTypeId == item.CardTypeId).ToListAsync();

                if (checkCardConfig.Any())
                {
                    Errors.Add(ResponseMessage.CardTypeConfigured);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeConfigured, Errors);
                };
            }
            return new BaseResponse<bool>();
        }
        private async Task<BaseResponse<bool>> ValidateThatDenominationForCardDoesNotExist(ConfigureNormalCardDTO model)
        {
            foreach (var item in model.NormalCardConfigDTO)
            {
                // VALIDATE IF CARDTYPE DEMONINATION CONATAINS ANY DATA FOR THIS CARDTYPE

                var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => x.CardTypeId == item.CardTypeId).ToListAsync();

                if (checkCardConfig.Any())
                {
                    Errors.Add(ResponseMessage.CardTypeConfigured);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeConfigured, Errors);
                };
            }
            return new BaseResponse<bool>();
        }
        private async Task<BaseResponse<bool>> ValidateThatDenominationForCardDoesNotExist(ConfigureVisaCardDTO model)
        {
            foreach (var item in model.VisaCardConfigDTO)
            {
                // VALIDATE IF CARDTYPE DEMONINATION CONATAINS ANY DATA FOR THIS CARDTYPE

                var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => x.CardTypeId == item.CardTypeId).ToListAsync();

                if (checkCardConfig.Any())
                {
                    Errors.Add(ResponseMessage.CardTypeConfigured);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeConfigured, Errors);
                };
            }
            return new BaseResponse<bool>();
        }

        /// <summary>
        /// CONFIGURE CREATE RECEIPT CARD TYPE
        /// </summary>
        /// <param name="ReceiptTypeCardConfigDTO">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task CreateReceiptTypeDenomination(ConfigureReceiptTypeCardDTO model, Guid UserId, Guid CardId)
        {
            var cardTypeDenominations = new List<CardTypeDenomination>();
            var allCardTypes = new List<Guid>();

            foreach (var receiptCardType in model.ReceiptTypeConfig)
            {
                var CardTypeId = receiptCardType.CardTypeId;
                var ReceiptId = receiptCardType.ReceiptTypeId;
                allCardTypes.Add(receiptCardType.CardTypeId);

                receiptCardType.CardRates.ForEach(x => cardTypeDenominations
                    .Add(new CardTypeDenomination
                    {
                        Rate = x.Rate,
                        DenominationId = x.DenominationId,
                        CardTypeId = CardTypeId,
                        ReceiptId = ReceiptId,
                        CreatedBy = UserId,
                        CreatedOn = DateTime.UtcNow,
                        IsActive = true
                    }));                
            }

            _dbContext.CardTypeDenomination.AddRange(cardTypeDenominations);

            //UPDATES CARD TYPE STATUS
            var card = _dbContext.Cards.FirstOrDefault(x => x.Id == CardId);

            card.CardStatus = CardStatus.Approved;

            var cardTypes = await _dbContext.CardTypes.Where(x => allCardTypes.Contains(x.Id)).ToListAsync();
            cardTypes.ForEach(x => x.CardStatus = CardStatus.Approved);

            _dbContext.SaveChanges();

            _logger.Info("About to Save CardType Denomination For Create Receipt ype Card Config... at ExecutionPoint:ConfigureReceiptTypeCard");
            await _dbContext.SaveChangesAsync();
            _logger.Info("Successfully Saved CardType Denomination For Create Receipt Type Card Config... at ExecutionPoint:ConfigureReceiptTypeCard");
        }


        /// <summary>
        /// CONFIGURE VISA CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> ConfigureVisaCard(ConfigureVisaCardDTO model, Guid UserId, Guid CardId)
        {
            //VALIDATES CARD ID
            var card = await FindCard(CardId);

            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
            }

            //VALIDATES THE CREATED CARD IF IT IS NOT VISA
            if (card.BaseCardType != BaseCardType.SPECIAL)
            {
                Errors.Add(ResponseMessage.CardNotSpecial);
                return new BaseResponse<bool>(ResponseMessage.CardNotSpecial, Errors);
            }

            //VALIDATES COUNTRYID AGAINST CARDTYPEID
            var result = await ValidateVisaTypeCardConfigMain(model);

            if (result.Errors.Any())
            {
                return new BaseResponse<bool>(result.ResponseMessage, result.Errors);
            }

            // VALIDATE IF CARDTYPE DEMONINATION TABLE CONTAINS ANY DATA FOR THIS CARDTYPE ID
            var data = await ValidateThatDenominationForCardDoesNotExist(model);

            if (data.Errors.Any())
            {
                return new BaseResponse<bool>(data.ResponseMessage, data.Errors);
            }


            foreach (var rates in model.VisaCardConfigDTO.Select(x => x.CardRates))
            {
                var validateDenomination = ValidateDenominationIds(rates.Select(x => x.DenominationId).ToList());

                if (validateDenomination)
                {
                    Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                }
            }

            //VALIDATES PREFIX
            var validatePrefix = ValidatePrefix(model.VisaCardConfigDTO.Select(x => x.PrefixId).ToList());

            if (validatePrefix)
            {
                Errors.Add(ResponseMessage.CardReceiptNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardReceiptNotFound, Errors);
            }

            //CONFIGURES VISA CARD TYPE DENOMINATION
            await CreateVisaDenomination(model, UserId, CardId);


            return new BaseResponse<bool>(true, ResponseMessage.CardConfigSuccess);
        }

        private async Task<BaseResponse<bool>> ValidateVisaTypeCardConfigMain(ConfigureVisaCardDTO model)
        {
            foreach (var item in model.VisaCardConfigDTO)
            {
                // GET ALL CARD TYPES FOR THIS COUNTRY, THIS SHOULD RETURN THE E-CODE AND PHYSICAL VARIANT
                var cardtype = await _dbContext.CardType
                    .Where(x => x.CountryId == item.CountryId
                    && x.Id == item.CardTypeId
                    ).ToListAsync();

                if (!cardtype.Any())
                {
                    Errors.Add(ResponseMessage.CardTypeNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeNotFound, Errors);
                }
            }

            return new BaseResponse<bool>();
        }

        /// <summary>
        /// CONFIGURE CREATE VISA CARD TYPE
        /// </summary>
        /// <param name="VisaCardConfigDTO">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Systems.Threadings.Tasks.Task</returns>
        private async Task CreateVisaDenomination(ConfigureVisaCardDTO model, Guid UserId, Guid CardId)
        {
            var cardTypeDenominations = new List<CardTypeDenomination>();
            var allCardTypes = new List<Guid>();

            foreach (var receiptCardType in model.VisaCardConfigDTO)
            {
                var CardTypeId = receiptCardType.CardTypeId;
                var prefix = receiptCardType.PrefixId;
                allCardTypes.Add(receiptCardType.CardTypeId);

                receiptCardType.CardRates.ForEach(x => cardTypeDenominations
                    .Add(new CardTypeDenomination
                    {
                        Rate = x.Rate,
                        DenominationId = x.DenominationId,
                        CardTypeId = CardTypeId,
                        PrefixId = prefix,
                        CreatedBy = UserId,
                        CreatedOn = DateTime.UtcNow,
                        IsActive = true
                    }));
            }

            _dbContext.CardTypeDenomination.AddRange(cardTypeDenominations);

            //UPDATES CARD TYPE STATUS
            var card = _dbContext.Cards.FirstOrDefault(x => x.Id == CardId);
            card.CardStatus = CardStatus.Approved;

            var cardTypes = await _dbContext.CardTypes.Where(x => allCardTypes.Contains(x.Id)).ToListAsync();
            cardTypes.ForEach(x => x.CardStatus = CardStatus.Approved);
            _dbContext.SaveChanges();
            
            _logger.Info("About to Save CardType Denomination For Create Visa Card Type Config... at ExecutionPoint:ConfigureVisaCard");
            await _dbContext.SaveChangesAsync();
            _logger.Info("Successfully Saved CardType Denomination For Create Visa Card Type Config... at ExecutionPoint:ConfigureVisaCard");
        }

        /// <summary>
        /// GET CARD
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;CardDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<CardDTO>> GetCard(Guid id)
        {

            var card = await _dbContext.Cards.Where(x => x.Id == id)
                .Include(x => x.CardType).ThenInclude(x => x.Country)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .FirstOrDefaultAsync();

            if (card is null)
            {                
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<CardDTO>(ResponseMessage.CardNotFound, Errors);
            }

            var cardTypesDTO = card.CardType.OrderByDescending(x => x.CreatedOn).Select(x => (CardTypeDTO)x).ToList();

            CardDTO cardDto = card;
            cardDto.CardTypeDTOs = cardTypesDTO;

            return new BaseResponse<CardDTO>(cardDto, ResponseMessage.SuccessMessage000);
        }


        /// <summary>
        /// GET ALL CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;CardDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> GetAllCard(BaseSearchViewModel model) 
        {
         
            var cardsQuery = _dbContext.Cards
                .Include(x => x.CardType).ThenInclude(x => x.Country)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .AsNoTracking()
                .AsQueryable();

            var pagedCards = await EntityFilter(cardsQuery, model).OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {cardsDto.Count} CARD(s)." };

        }


        /// <summary>
        /// GET ALL PENDING CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CardDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> GetAllPendingCardConfig(BaseSearchViewModel model)
        {

            var cardTypes = await _dbContext.CardType
                .Where(x => x.CardStatus == CardStatus.Pending)
                .Include(x => x.Country)
                .AsNoTracking()
                .ToListAsync();
         
            var cards = _dbContext.Cards.Where(x => x.CardStatus == CardStatus.Pending).AsNoTracking().AsQueryable();

            var pagedCards = await cards.OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            cardsDto.ForEach(x =>
            {
                x.CardTypeDTOs = cardTypes.Where(c => c.CardId == x.Id).Select(x => (CardTypeDTO)x).ToList();
            });

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {cardsDto.Count} CARD(s)." };
        }


        /// <summary>
        /// GET ALL APPROVED CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CardDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> GetAllApprovedCardConfig(BaseSearchViewModel model)
        {
            var cardTypes = await _dbContext.CardType
                .Where(x => x.CardStatus == CardStatus.Approved)
                .Include(x => x.Country)
                .Include(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .Include(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .AsNoTracking()
                .ToListAsync();

            var cards = _dbContext.Cards.AsNoTracking().Where(x => cardTypes.Select(x => x.CardId).Contains(x.Id) && x.CardStatus == CardStatus.Approved).AsQueryable();

            var pagedCards = await cards.OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            cardsDto.ForEach(x =>
            {
                x.CardTypeDTOs = cardTypes.Where(c => c.CardId == x.Id).Select(x => (CardTypeDTO)x).ToList();
            });

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {cardsDto.Count} CARD(s)." };
        }


        /// <summary>
        /// ADD COUNTRies TO CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> AddCountryToCard(AddCountryToCardDTO model, Guid UserId, Guid CardId)
        {
            try
            {   
                //VALIDATES CARD ID.
                var card = await FindCard(CardId);

                if (card is null)
                {
                    Errors.Add(ResponseMessage.CardNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
                }

                //VALIDATES COUNTRY IDs.
                var countryValidation = ValidateCountry(model.CountryIds);

                if (countryValidation.Errors.Any())
                {
                    return new BaseResponse<bool>(countryValidation.ResponseMessage, countryValidation.Errors);
                };
                               

               // var getExistingCards = await _dbContext.Cards.Where(x => x.Id == card.Id).Include(x => x.CardType).FirstOrDefaultAsync();
                var existingsCountryIds = card.CardType.Select(x => x.CountryId);

                // VALIDATE IF CARD HAS ANY OF THE SELECTED ID
                var countryExist = existingsCountryIds.Any(x => countryValidation.Data.CountryIds.Contains(x));

                if (countryExist)
                {
                    return new BaseResponse<bool>(ResponseMessage.CardCountryExist, countryValidation.Errors);
                }


                var newCountries = countryValidation.Data.CountryIds.Where(x => !existingsCountryIds.Contains(x));

                //CREATES CARD TYPE FOR NEW INCOMING COUNTRY IDs.
                var cardTypes = CreateCardTypes(newCountries.ToList(), UserId);
                card.CardType.AddRange(cardTypes);

                card.ModifiedBy = UserId;
                card.ModifiedOn = DateTime.UtcNow;

                _logger.Info($"About To Update Card... at Execution: {nameof(AddCountryToCard)}");
                await _dbContext.SaveChangesAsync();
                _logger.Info($"Successfully Updated Card... at Execution: {nameof(AddCountryToCard)}");

                return new BaseResponse<bool>(true, ResponseMessage.CardUpdate);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);

                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage999, Errors);
            }
            
        }

       
        /// <summary>
        /// UPDATE VISA CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateVisaCard(UpdateVisaTypeCardDTO model, Guid UserId, Guid CardId)
        {
            var uploadedFileToDelete = string.Empty;

            try
            {
                var response = new BaseResponse<bool>();

                //VALIDATES CARD ID. 
                var card = await FindCard(CardId);

                if (card is null)
                {
                    Errors.Add(ResponseMessage.CardNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
                }

                //VALIDATES THE CREATED CARD IF IT IS NOT VISA
                if (card.BaseCardType != BaseCardType.SPECIAL)
                {
                    Errors.Add(ResponseMessage.CardNotSpecial);
                    return new BaseResponse<bool>(ResponseMessage.CardNotSpecial, Errors);
                }

                //VALIDATES INCOMING CARD NAME
                var cardNameValidation = await ValidateCardName(model.CardName, card);
                if (cardNameValidation)
                {
                    Errors.Add(ResponseMessage.CardExist);
                    return new BaseResponse<bool>(ResponseMessage.CardExist, Errors);
                }

                //VALIDATE CARD TYPES
                var validateCardType = ValidateCardTypes(CardId, model.UpdateVisaTypeConfigDTO.Select(x => x.CardTypeId).ToList());

                if (validateCardType)
                {
                    Errors.Add(ResponseMessage.CardTypeNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeNotFound, Errors);
                }

                //VALIDATE INCOMING CARD TYPE DENOMINATION
                var validateCardTypeDenomination
                    = ValidateCardTypeDenomination(model.UpdateVisaTypeConfigDTO.SelectMany(x => x.UpdateCardRateDenominationConfigDTO).Select(x => x.CardRateId).ToList());

                if (validateCardTypeDenomination)
                {
                    Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                }

                //VALIDATES INCOMING DENOMINATION NOT DUPLICATED OR DOESN'T EXISTS.
                foreach (var cardRates in model.UpdateVisaTypeConfigDTO.Select(x => x.UpdateCardRateDenominationConfigDTO))
                {
                    var validateDenomination = ValidateDenominationIds(cardRates.Select(x => x.DenominationId).ToList());

                    if (validateDenomination)
                    {
                        Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                        return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                    }
                }

                //VALIDATES PREFIX
                var validatePrefix = ValidatePrefix(model.UpdateVisaTypeConfigDTO.Select(x => x.PrefixId).ToList());
                if (validatePrefix)
                {
                    Errors.Add(ResponseMessage.VisaPrefixNotFound);
                    return new BaseResponse<bool>(ResponseMessage.VisaPrefixNotFound, Errors);
                }

                //UPDATE OR CREATES CARD IMAGE IF IT DOESN'T EXISTS.
                uploadedFileToDelete = await CreatesOrUpdatesImage(model.Logo, card);


                //UPDATES THE ALREADY CONFIGURED VISA CARD TYPE DENOMINATION
                await UpdateVisa(model.UpdateVisaTypeConfigDTO, UserId);

                //UPDATES CARD 
                _dbContext.Cards.Update(card);
                await _dbContext.SaveChangesAsync();

                return new BaseResponse<bool>(true, ResponseMessage.CardUpdate);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(uploadedFileToDelete))
                {
                    await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(uploadedFileToDelete));
                    _logger.Error(ex.Message, ex);
                }

                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage999, Errors);
            }
           
        }

        /// <summary>
        /// UPDATE VISA CARD
        /// </summary>
        /// <param name="VisaCardUpdateConfigDTO">The VisaCardUpdateConfigDTO.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task UpdateVisa(List<UpdateVisaTypeCardConfigDTO> VisaCardUpdateConfigDTO, Guid UserId)
        {
            var newCardTypeDenomination = new List<CardTypeDenomination>();
            //SELECT ALL CARD TYPE DENOMINATION IDs
            var allCardTypeDenominationsIds = VisaCardUpdateConfigDTO.SelectMany(x => x.UpdateCardRateDenominationConfigDTO).Select(x => x.CardRateId);
            //GET ALL CARD TYPE DENOMINATIONS IDs.
            var allCardTypeDenominations = await _dbContext.CardTypeDenomination.Where(x => allCardTypeDenominationsIds.Contains(x.Id)).ToListAsync();

            //LOOP THROUGH INCOMING VISA CARD UPDATE MODEL
            foreach (var visaCardUpdateConfigDTO in VisaCardUpdateConfigDTO)
            {
               
                //GET THE CARD TYPE DENOMINATION IDs TO BE UPDATED
                var cardTypeDenominationIds = visaCardUpdateConfigDTO.UpdateCardRateDenominationConfigDTO.Select(x => x.CardRateId);
                var cardTypeDenominationsToBeUpdated = allCardTypeDenominations.Where(x => cardTypeDenominationIds.Contains(x.Id)).ToList();

                //LOOP THROUGH THE VISA CARD RATE DENOMINATION -> UPDATE CARD RATE DENOMINATION CONFIGDTO
                foreach (var updateCardRateDenominationDTO in visaCardUpdateConfigDTO.UpdateCardRateDenominationConfigDTO)
                {
                    //CHECK IF A CARD TYPE DENOMINATION DOESN'T ALREADY HAVE THE DENOMINATION ID.
                    var acardTypeToBeUpdated = cardTypeDenominationsToBeUpdated
                        .FirstOrDefault(x => x.Id == updateCardRateDenominationDTO.CardRateId && x.DenominationId == updateCardRateDenominationDTO.DenominationId);

                    if (!(acardTypeToBeUpdated is null))
                    {
                        //UPDATE CARD TYPE DENOMINATION
                        acardTypeToBeUpdated.PrefixId = visaCardUpdateConfigDTO.PrefixId;
                        acardTypeToBeUpdated.Rate = updateCardRateDenominationDTO.Rate;
                        acardTypeToBeUpdated.ModifiedBy = UserId;
                        acardTypeToBeUpdated.ModifiedOn = DateTime.UtcNow;
                    }
                    else 
                    {
                        //CREATE NEW CARD TYPE DENOMINATION
                        newCardTypeDenomination.Add(new CardTypeDenomination
                        {
                            CardTypeId = visaCardUpdateConfigDTO.CardTypeId,
                            DenominationId = updateCardRateDenominationDTO.DenominationId,
                            Rate = updateCardRateDenominationDTO.Rate,
                            PrefixId = visaCardUpdateConfigDTO.PrefixId,
                            CreatedBy = UserId
                        });

                    }

                }
                
            }

            await _dbContext.CardTypeDenomination.AddRangeAsync(newCardTypeDenomination);

            _logger.Info("About to Save CardType Denomination For Update Visa Card Type Config... at ExecutionPoint:UpdateVisa");
            await _dbContext.SaveChangesAsync();
            _logger.Info("Successfully Saved CardType Denomination For Update Visa Card Type Config... at ExecutionPoint:UpdateVisa");
        }

        /// <summary>
        /// UPDATE RECEIPT TYPE CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateReceiptCard(UpdateReceiptTypeCardDTO model, Guid UserId, Guid CardId)
        {
            var uploadedFileToDelete = string.Empty;

            try
            {
                var response = new BaseResponse<bool>();

                //VALIDATES CARD ID.
                var card = await FindCard(CardId);

                if (card is null)
                {
                    Errors.Add(ResponseMessage.CardNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
                };

                //VALIDATES THE CREATED CARD IF IT IS AN AMAZON TYPE
                if (card.BaseCardType != BaseCardType.AMAZON)
                {
                    Errors.Add(ResponseMessage.CardNotAmazon);
                    return new BaseResponse<bool>(ResponseMessage.CardNotAmazon, Errors);
                }

                //VALIDATES INCOMING CARD NAME
                var cardNameValidation = await ValidateCardName(model.CardName, card);
                if (cardNameValidation)
                {
                    Errors.Add(ResponseMessage.CardExist);
                    return new BaseResponse<bool>(ResponseMessage.CardExist, Errors);
                }

                //VALIDATE CARD TYPES
                var validateCardType = ValidateCardTypes(CardId, model.UpdateReceiptTypeConfigDTO.Select(x => x.CardTypeId).ToList());
                if (validateCardType)
                {
                    Errors.Add(ResponseMessage.CardTypeNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeNotFound, Errors);
                }

                //VALIDATE INCOMING CARD TYPE DENOMINATION
                var validateCardTypeDenomination
                    = ValidateCardTypeDenomination(model.UpdateReceiptTypeConfigDTO.SelectMany(x => x.UpdateCardRateDenominationConfigDTO).Select(x => x.CardRateId).ToList());

                if (validateCardTypeDenomination)
                {
                    Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                }

                //VALIDATES INCOMING DENOMINATION IDs NOT DUPLICATED OR DOESN'T EXISTS.
                foreach (var cardRates in model.UpdateReceiptTypeConfigDTO.Select(x => x.UpdateCardRateDenominationConfigDTO))
                {
                    var validateDenomination = ValidateDenominationIds(cardRates.Select(x => x.DenominationId).ToList());

                    if (validateDenomination)
                    {
                        Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                        return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                    }
                }

                //VALIDATES RECEIPT TYPE
                var validateReceipt = ValidateReceipt(model.UpdateReceiptTypeConfigDTO.Select(x => x.ReceiptTypeId).ToList());
                if (validateReceipt)
                {
                    Errors.Add(ResponseMessage.CardReceiptNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardReceiptNotFound, Errors);
                }


                //UPDATE OR CREATES CARD IMAGE IF IT DOESN'T EXISTS.
                uploadedFileToDelete = await CreatesOrUpdatesImage(model.Logo, card);

                //UPDATES RECEIPT TYPE CARD CONFIG
                await UpdateReceiptType(model.UpdateReceiptTypeConfigDTO, UserId);

                //UPDATES CARD 
                _dbContext.Cards.Update(card);
                await _dbContext.SaveChangesAsync();

                return new BaseResponse<bool>(true, ResponseMessage.CardUpdate);
            }
            catch (Exception ex)
            {

                if (!string.IsNullOrWhiteSpace(uploadedFileToDelete))
                {
                    await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(uploadedFileToDelete));
                    _logger.Error(ex.Message, ex);
                }

                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage999, Errors);
            }
           
        }

        /// <summary>
        /// UPDATE VISA CARD
        /// </summary>
        /// <param name="ReceiptTypeUpdateCardConfigDTO">The ReceiptTypeUpdateCardConfigDTO.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task UpdateReceiptType(List<UpdateReceiptTypeCardConfigDTO> ReceiptTypeUpdateCardConfigDTO, Guid UserId)
        {

            var newCardTypeDenomination = new List<CardTypeDenomination>();
            //SELECT ALL CARD TYPE DENOMINATION IDs
            var allCardTypeDenominationsIds = ReceiptTypeUpdateCardConfigDTO.SelectMany(x => x.UpdateCardRateDenominationConfigDTO).Select(x => x.CardRateId);
            //GET ALL CARD TYPE DENOMINATIONS IDs.
            var allCardTypeDenominations = await _dbContext.CardTypeDenomination.Where(x => allCardTypeDenominationsIds.Contains(x.Id)).ToListAsync();

            //LOOP THROUGH THE INCOMING RECEIPT CARD TYPE UPDATE MODEL
            foreach (var receiptTypeCardConfigDTO in ReceiptTypeUpdateCardConfigDTO)
            {

                //GET THE CARD TYPE DENOMINATION IDs TO BE UPDATED
                var cardTypeDenominationIds = receiptTypeCardConfigDTO.UpdateCardRateDenominationConfigDTO.Select(x => x.CardRateId);
                var cardTypeDenominationsToBeUpdated = allCardTypeDenominations.Where(x => cardTypeDenominationIds.Contains(x.Id)).ToList();

                //LOOP THROUGH THE RECEIPT CARD RATE DENOMINATION CONFIG  
                foreach (var updateCardRateDenominationDTO in receiptTypeCardConfigDTO.UpdateCardRateDenominationConfigDTO)
                {
                    //CHECK IF A CARD TYPE DENOMINATION DOESN'T ALREADY HAVE THE DENOMINATION ID.
                    var acardTypeToBeUpdated = cardTypeDenominationsToBeUpdated
                        .FirstOrDefault(x => x.Id == updateCardRateDenominationDTO.CardRateId && x.DenominationId == updateCardRateDenominationDTO.DenominationId);

                    if (!(acardTypeToBeUpdated is null))
                    {
                        //UPDATE CARD TYPE DENOMINATION
                        acardTypeToBeUpdated.PrefixId = receiptTypeCardConfigDTO.ReceiptTypeId;
                        acardTypeToBeUpdated.Rate = updateCardRateDenominationDTO.Rate;
                        acardTypeToBeUpdated.ModifiedBy = UserId;
                        acardTypeToBeUpdated.ModifiedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        //CREATE NEW CARD TYPE DENOMINATION
                        newCardTypeDenomination.Add(new CardTypeDenomination
                        {
                            CardTypeId = receiptTypeCardConfigDTO.CardTypeId,
                            DenominationId = updateCardRateDenominationDTO.DenominationId,
                            Rate = updateCardRateDenominationDTO.Rate,
                            ReceiptId = receiptTypeCardConfigDTO.ReceiptTypeId,
                            CreatedBy = UserId
                        });

                    }

                }

            }


            await _dbContext.CardTypeDenomination.AddRangeAsync(newCardTypeDenomination);

            _logger.Info("About to Save CardType Denomination For Update Receipt Card Type Config... at ExecutionPoint:UpdateReceiptType");
            await _dbContext.SaveChangesAsync();
            _logger.Info("Successfully Saved CardType Denomination For Update Receipt Card Type Config... at ExecutionPoint:UpdateReceiptType");
        }


        /// <summary>
        /// UPDATE NORMAL CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateNormalCard(UpdateNormalTypeCardDTO model, Guid UserId, Guid CardId)
        {
            var uploadedFileToDelete = string.Empty;

            try
            {
                var response = new BaseResponse<bool>();

                //VALIDATES CARD ID.
                var card = await FindCard(CardId);

                if (card is null)
                {
                    Errors.Add(ResponseMessage.CardNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
                };


                //VALIDATES THE CREATED CARD IF IT IS A REGULAR TYPE
                if (card.BaseCardType != BaseCardType.REGULAR)
                {
                    Errors.Add(ResponseMessage.CardNotRegular);
                    return new BaseResponse<bool>(ResponseMessage.CardNotRegular, Errors);
                }

                //VALIDATES INCOMING CARD NAME
                var cardNameValidation = await ValidateCardName(model.CardName, card);
                if (cardNameValidation)
                {
                    Errors.Add(ResponseMessage.CardExist);
                    return new BaseResponse<bool>(ResponseMessage.CardExist, Errors);
                }

                //VALIDATE CARD TYPES
                var validateCardType = ValidateCardTypes(CardId, model.UpdateNormalCardTypeConfigDTO.Select(x => x.CardTypeId).ToList());

                if (validateCardType)
                {
                    Errors.Add(ResponseMessage.CardTypeNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeNotFound, Errors);
                }

                //VALIDATE INCOMING CARD TYPE DENOMINATION
                var validateCardTypeDenomination
                    = ValidateCardTypeDenomination(model.UpdateNormalCardTypeConfigDTO.SelectMany(x => x.UpdateCardRateDenominationConfigDTO).Select(x => x.CardRateId).ToList());

                if (validateCardTypeDenomination)
                {
                    Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                }

                //VALIDATES INCOMING DENOMINATION IDs NOT DUPLICATED OR DOESN'T EXISTS.
                foreach (var cardRates in model.UpdateNormalCardTypeConfigDTO.Select(x => x.UpdateCardRateDenominationConfigDTO))
                {
                    var validateDenomination = ValidateDenominationIds(cardRates.Select(x => x.DenominationId).ToList());

                    if (validateDenomination)
                    {
                        Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                        return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                    }
                }


                //UPDATE OR CREATES CARD IMAGE IF IT DOESN'T EXISTS.
                uploadedFileToDelete = await CreatesOrUpdatesImage(model.Logo, card);

                //UPDATES NORMAL CARD 
                await UpdateNormalCard(model.UpdateNormalCardTypeConfigDTO, UserId);

                //UPDATES CARD 
                _dbContext.Cards.Update(card);
                await _dbContext.SaveChangesAsync();

                return new BaseResponse<bool>(true, ResponseMessage.CardUpdate);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(uploadedFileToDelete))
                {
                    await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(uploadedFileToDelete));
                    _logger.Error(ex.Message, ex);
                }
               
                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage999, Errors);

            }
            
        }

        /// <summary>
        /// UPDATE NORMAL CARD
        /// </summary>
        /// <param name="UpdateCardConfigDTO">The UpdateCardConfigDTO.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task UpdateNormalCard(List<UpdateCardConfig> UpdateCardConfigDTO, Guid UserId)
        {
            
            var newCardTypeDenomination = new List<CardTypeDenomination>();
            //SELECT ALL CARD TYPE DENOMINATION IDs
            var allCardTypeDenominationsIds = UpdateCardConfigDTO.SelectMany(x => x.UpdateCardRateDenominationConfigDTO).Select(x => x.CardRateId);
            //GET ALL CARD TYPE DENOMINATIONS IDs.
            var allCardTypeDenominations = await _dbContext.CardTypeDenomination.Where(x => allCardTypeDenominationsIds.Contains(x.Id)).ToListAsync();


            //LOOP THROUGH INCOMING NORMAL CARD UPDATE MODEL
            foreach (var normalCardConfigDTO in UpdateCardConfigDTO)
            {

                //GET THE CARD TYPE DENOMINATION IDs TO BE UPDATED
                var cardTypeDenominationIds = normalCardConfigDTO.UpdateCardRateDenominationConfigDTO.Select(x => x.CardRateId);
                var cardTypeDenominationsToBeUpdated = allCardTypeDenominations.Where(x => cardTypeDenominationIds.Contains(x.Id)).ToList();

                //LOOP THROUGH THE NORMAL CARD RATE DENOMINATION MODEL
                foreach (var updateCardRateDenominationDTO in normalCardConfigDTO.UpdateCardRateDenominationConfigDTO)
                {
                    //CHECK IF A CARD TYPE DENOMINATION DOESN'T ALREADY HAVE THE DENOMINATION ID.
                    var acardTypeToBeUpdated = cardTypeDenominationsToBeUpdated
                        .FirstOrDefault(x => x.Id == updateCardRateDenominationDTO.CardRateId && x.DenominationId == updateCardRateDenominationDTO.DenominationId);

                    if (!(acardTypeToBeUpdated is null))
                    {
                        //UPDATE CARD TYPE DENOMINATION
                        acardTypeToBeUpdated.Rate = updateCardRateDenominationDTO.Rate;
                        acardTypeToBeUpdated.ModifiedBy = UserId;
                        acardTypeToBeUpdated.ModifiedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        //CREATE NEW CARD TYPE DENOMINATION
                        newCardTypeDenomination.Add(new CardTypeDenomination
                        {
                            CardTypeId = normalCardConfigDTO.CardTypeId,
                            DenominationId = updateCardRateDenominationDTO.DenominationId,
                            Rate = updateCardRateDenominationDTO.Rate,
                            CreatedBy = UserId
                        });

                    }

                }

            }
            

            await _dbContext.CardTypeDenomination.AddRangeAsync(newCardTypeDenomination);
            _logger.Info("About to Save CardType Denomination For Update Normal Card Type Config... at ExecutionPoint:UpdateNormalCard");
            await _dbContext.SaveChangesAsync();
            _logger.Info("Successfully Saved CardType Denomination For Update Normal Card Type Config... at ExecutionPoint:UpdateNormalCard");
        }


        /// <summary>
        /// DELETE CARD TYPE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteCardType(DeleteCardTypeDTO model, Guid CardId)
        {
            //VALIDATES CARD ID
            var card = await FindCard(CardId);

            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
            };

            //VALIDATE CARD TYPES
            var validateCardType = ValidateCardTypes(CardId, model.CardTypeIds);

            if (validateCardType)
            {
                Errors.Add(ResponseMessage.CardTypeNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeNotFound, Errors);
            }

            //CANNOT DELETE CARDTYPE IF IT HAS ALREADY BEEN CONFIGURED
            var getCardType = await _dbContext.CardType.Where(x => x.CardId == card.Id && model.CardTypeIds.Contains(x.Id)).ToListAsync();
            
            if (await _dbContext.CardTypeDenomination.AnyAsync(x => getCardType.Select(x => x.Id).Contains(x.CardTypeId)))
            {
                Errors.Add(ResponseMessage.CannotDeleteCard);
                return new BaseResponse<bool>(ResponseMessage.CannotDeleteCard, Errors);
            }

            _dbContext.CardType.RemoveRange(getCardType);
            await _dbContext.SaveChangesAsync();

            return new BaseResponse<bool>(true, ResponseMessage.DeleteCardType);
        }


        /// <summary>
        /// VALIDATE DENOMINATION
        /// </summary>
        /// <param name="denominationIds">The denominationIds.</param>
        /// <returns>System.boolean</returns>
        private bool ValidateDenominationIds(List<Guid> denominationIds)
        {
     
            var denominations = _dbContext.Denominations.Where(x => denominationIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

            if (denominations.Count() != denominationIds.Count())
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
        /// VALIDATE RECEIPT
        /// </summary>
        /// <param name="receiptIds">The receiptIds.</param>
        /// <returns>System.boolean</returns>
        private bool ValidateReceipt(List<Guid> receiptIds)
        {
            var receipts = _dbContext.Receipts.Where(x => receiptIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

            if (receipts.Count() != receiptIds.Distinct().Count())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// VALIDATE CARD TYPES
        /// </summary>
        /// <param name="id" >the Ids</param>   
        /// <param name="cardTypesIds">The cardType Ids.</param>
        /// <returns>System.boolean</returns>
        private bool ValidateCardTypes(Guid cardId, List<Guid> cardTypesIds)
        {
            // VALIDATE THAT THE CARDTYPE BELONGS TO THIS CARD
            var cardTypes = _dbContext.CardType
                .Where(x => x.CardId == cardId 
                    && cardTypesIds.Contains(x.Id))
                .ToListAsync().Result.Select(x => x.Id);

            if (cardTypes.Count() != cardTypesIds.Count())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// CREATE CARD TYPES
        /// </summary>
        /// <param name="CountryIds">The countryIds.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;ValidateCountryDTO&gt;&gt;.</returns>
        private List<Models.Entities.CardType> CreateCardTypes(List<Guid> CountryIds, Guid UserId)
        {
            List<Models.Entities.CardType> cardTypes = new List<Models.Entities.CardType>();

            //CREATES THE CARD TYPE; E-CODE AND PHYSICAL FOR EACH OF THE COUNTRIES
            foreach (var countryId in CountryIds)
            {
                var newCardEcodeType = new Models.Entities.CardType
                {
                    CountryId = countryId,
                    CardCategory = CardCategory.E_CODE,
                    CreatedBy = UserId,
                    CardStatus = CardStatus.Pending
                };

                cardTypes.Add(newCardEcodeType);

                var newCardPhysicalType = new Models.Entities.CardType
                {
                    CountryId = countryId,
                    CardCategory = CardCategory.PHYSICAL,
                    CreatedBy = UserId,
                    CardStatus = CardStatus.Pending
                };

                cardTypes.Add(newCardPhysicalType);
            }

            _logger.Info("Configuring CardTypes...");
            return cardTypes;
        }


        /// <summary>
        /// VALIDATE COUNTRY
        /// </summary>
        /// <param name="countryIds">The countryIds.</param>
        /// <returns>Task&lt;BaseResponse&lt;ValidateCountryDTO&gt;&gt;.</returns>
        private BaseResponse<ValidateCountryDTO> ValidateCountry(List<Guid> countryIds)
        {
            var countries = _dbContext.Countries
                .Where(x => countryIds.Contains(x.Id))
                .ToListAsync().Result.Select(x => x.Id);

            if (countryIds.Count != countries.Count())
            {
                Errors.Add(ResponseMessage.CountryNotDistinct);
                return new BaseResponse<ValidateCountryDTO>(ResponseMessage.CountryNotDistinct, Errors);
            }
            else
            {
                var data = new ValidateCountryDTO()
                {
                    CountryIds = countries.ToList()
                };

                return new BaseResponse<ValidateCountryDTO>(data);                
            }
        }


        /// <summary>
        /// VALIDATE CARD TYPE DENOMINATION
        /// </summary>
        /// <param name="cardTypeDenominationIds">The prefixIds.</param>
        /// <returns>System.boolean</returns>
        private bool ValidateCardTypeDenomination(List<Guid> cardTypeDenominationIds)
        {
            var cardTypeDenomination = _dbContext.CardTypeDenomination
                .Where(x => cardTypeDenominationIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

            if (cardTypeDenomination.Count() != cardTypeDenominationIds.Where(x => x != Guid.Empty).Distinct().Count()) 
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// FIND CARD
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private async Task<Card> FindCard(Guid id) =>
            await _dbContext.Cards.Include(x => x.CardType).FirstOrDefaultAsync(x => x.Id == id);


        /// <summary>
        /// GENERATE DELETE UPLOADED PATH
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>BaseResponse&lt;bool&gt;.</returns>
        private string GenerateDeleteUploadedPath(string value)
        {
            //Split Image Url From Cloudinary
            var splittedLogoUrl = value.Split("/");

            //get the cloudinary PublicId
            var LogoPublicId = splittedLogoUrl[8];
            var splittedLogoPublicId = LogoPublicId.Split(".");

            //Get the Full Asset Path
            var fullPath = $"Optima/{splittedLogoPublicId[0]}";

            return fullPath;
        }

        /// <summary>
        /// ENTITY FILTER
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="query">The query.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private IQueryable<Card> EntityFilter(IQueryable<Card> query, BaseSearchViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Keyword) && !string.IsNullOrEmpty(model.Filter))
            {
                switch (model.Filter)
                {
                    case "CardName":
                        {
                            return query.Where(x => x.Name.ToLower().Contains(model.Keyword.ToLower()));
                        }

                    default:
                        break;                   
                }
            }

            return query;
        }

        /// <summary>
        /// VALIDATES CARD NAME
        /// </summary>
        /// <param name="cardName">the CardName</param>
        /// <param name="card">the Card</param>
        /// <returns></returns>
        private async Task<bool> ValidateCardName(string cardName, Card card)
        {
            if (String.IsNullOrWhiteSpace(cardName))
            {
                return false;
            }
            if (cardName.Replace(" ", "").ToLower() != card.Name.Replace(" ", "").ToLower())
            {
                var checkExistingCard = await _dbContext.Cards.AnyAsync(x => x.Name.ToLower().Replace(" ", "") == cardName.ToLower().Replace(" ", ""));
                if (checkExistingCard)
                    return true;
                
                card.Name = cardName;
            }
           
            return false;
        }

        /// <summary>
        /// CREATES OR UPDATES A CARD IMAGE
        /// </summary>
        /// <param name="Logo"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private async Task<string> CreatesOrUpdatesImage(IFormFile Logo, Card card)
        {
            //UPDATES CARD LOGO IF IT ALREADY EXISTS
            if (!(Logo is null) && !(card.LogoUrl is null))
            {

                _logger.Info("Preparing to Delete Image From Cloudinary... at ExecutionPoint:UpdateCard");
                var fullPath = GenerateDeleteUploadedPath(card.LogoUrl);
                await _cloudinaryServices.DeleteImage(fullPath);
                _logger.Info("Successfully Deleted Image From Cloudinary... at ExecutionPoint:UpdateCard");

                _logger.Info("Uploading Image to Cloudinary... at ExecutionPoint:UpdateCard");
                var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(Logo);
                _logger.Info("Successfully Uploaded to Cloudinary... at ExecutionPoint:UpdateCard");

                card.LogoUrl = uploadedFile;
                return uploadedFile;
            }

            //CREATES A CARD LOGO I.E.IT DOESN'T EXISTS.
            if (!(Logo is null) && (card.LogoUrl is null))
            {
                _logger.Info("Uploading Image to Cloudinary... at ExecutionPoint:UpdateCard");
                var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(Logo);
                _logger.Info("Successfully Uploaded to Cloudinary... at ExecutionPoint:UpdateCard");

                card.LogoUrl = uploadedFile;
                return uploadedFile;
            }

            return null;
        }

        public async Task<BaseResponse<MainCardDTO>> GetCard_Ordered_By_Country(Guid id)
        {
            var card = await _dbContext.Cards.Where(x => x.Id == id)
               .Include(x => x.CardType).ThenInclude(x => x.Country)
               .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
               .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
               .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
               .FirstOrDefaultAsync();

            //VALIDATES IF CARD EXIST
            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<MainCardDTO>(ResponseMessage.CardNotFound, Errors);
            }

            //GROUP CARD BY COUNTRIES
            var groupedCards = card.CardType.GroupBy(x => x.Country).ToList();

            MainCardDTO cardDTO = card;

            switch (card.BaseCardType)
            {
                case BaseCardType.REGULAR:
                    {                   
                        cardDTO.MainCardTypeDTOs = groupedCards.Select(x => new MainCardTypeDTO
                        {
                            CountryId = x.Key.Id,
                            CountryName = x.Key.Name,
                            Logo = x.Key.LogoUrl,
                            CreatedOn = x.Key.CreatedOn,
                            CardTypesDTO = x.Select(x => (MainCardTypes)x).ToList()                          
                        }).ToList();
                    }
                    break;
                case BaseCardType.AMAZON:
                    {
                        var groupedCardTypeDenomination = card.CardType.SelectMany(x => x.CardTypeDenomination).GroupBy(x => x.Receipt).FirstOrDefault();

                        cardDTO.MainCardTypeDTOs = groupedCards.Select(x => new MainCardTypeDTO
                        {
                            CountryId = x.Key.Id,
                            CountryName = x.Key.Name,
                            Logo = x.Key.LogoUrl,
                            CreatedOn = x.Key.CreatedOn,
                            CardTypesDTO = x.Select(x => new MainCardTypes
                            {
                                Id = x.Id,
                                CardStatus = x.CardStatus,
                                CardType = x.CardCategory,
                                CreatedOn = x.CreatedOn,
                                Receipt = new ReceiptCardTypeDenomination
                                {
                                    ReceiptId = groupedCardTypeDenomination.Key.Id,
                                    ReceiptType = groupedCardTypeDenomination.Key.Name,
                                    MainCardTypeDenominationDTO = groupedCardTypeDenomination.Where(y => y.CardTypeId == x.Id).Select(x => (MainCardTypeDenominationDTO)x).ToList()
                                }

                            }).ToList()
                        }).ToList();                       
                    }
                    break;
                case BaseCardType.SPECIAL:
                    {
                        var groupedCardTypeDenomination = card.CardType.SelectMany(x => x.CardTypeDenomination).GroupBy(x => x.Prefix).FirstOrDefault();

                        cardDTO.MainCardTypeDTOs = groupedCards.Select(x => new MainCardTypeDTO
                        {
                            CountryId = x.Key.Id,
                            CountryName = x.Key.Name,
                            Logo = x.Key.LogoUrl,
                            CreatedOn = x.Key.CreatedOn,
                            CardTypesDTO = x.Select(x => new MainCardTypes
                            {
                                Id = x.Id,                               
                                CardType = x.CardCategory,
                                CardStatus = x.CardStatus,
                                CreatedOn = x.CreatedOn,
                                Prefix = new PrefixCardTypeDenomination
                                {
                                    PrefixId = groupedCardTypeDenomination.Key.Id,
                                    PrefixNumber = groupedCardTypeDenomination.Key.PrefixNumber,
                                    MainCardTypeDenominationDTO = groupedCardTypeDenomination.Where(y => y.CardTypeId == x.Id).Select(x => (MainCardTypeDenominationDTO)x).ToList()
                                },                           
                            }).ToList()
                        }).ToList();
                    }
                    break;
                default:
                    break;
            }
          
            return new BaseResponse<MainCardDTO>(cardDTO, ResponseMessage.SuccessMessage000);
        }
    }
}
