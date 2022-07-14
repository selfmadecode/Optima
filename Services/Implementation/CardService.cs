using AzureRays.Shared.ViewModels;
using log4net;
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

            //VALIDATES INCOMING CARD CONFIG I.E. THE COUNTRIES IDs, THE CARD TYPE IDs.
            var validateCardConfig =
                await ValidateCardConfig(CardId, model.CardConfigDTO.Select(x => x.CountryId).ToList(), model.CardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
                return new BaseResponse<bool>(validateCardConfig.ResponseMessage, validateCardConfig.Errors);
                        

            //CHECK IF CARD TYPE HASN'T BEEN ALREADY CONFGURED.
            var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => model.CardConfigDTO.Select(x => x.CardTypeId).Contains(x.CardTypeId)).ToListAsync();

            if (checkCardConfig.Any())
            {
                Errors.Add(ResponseMessage.CardTypeConfigured);
                return new BaseResponse<bool>(ResponseMessage.CardTypeConfigured, Errors);
            }
            
            //VALIDATES DENOMINATION
            var validateDenomination = ValidateDenomination(model.CardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

            //CREATES THE CARD TYPE DENOM INATION FOR NORMAL CARD
            await CreateNormalCardTypeDenomination(model.CardConfigDTO, UserId);
           
            return new BaseResponse<bool>(true, ResponseMessage.CardConfigSuccess);
        }

        /// <summary>
        /// CREATE NORMAL CARD TYPE DENOMINATION
        /// </summary>
        /// <param name="CardConfigDTO">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threadings.Tasks.Task</returns>
        private async Task CreateNormalCardTypeDenomination(List<CardConfigDTO> CardConfigDTO, Guid UserId)
        {
            var cardTypeDenominations = new List<CardTypeDenomination>();

            //LOOP THROUGH EACH OF THE INCOMING CARD CONFIG
            foreach (var cardConfig in CardConfigDTO)
            {

                cardTypeDenominations.Add(new CardTypeDenomination
                {
                    CardTypeId = cardConfig.CardTypeId,
                    DenominationId = cardConfig.DenominationId,
                    Rate = cardConfig.Rate,
                    CreatedBy = UserId,
                });
            }

            _dbContext.CardTypeDenomination.AddRange(cardTypeDenominations);

            //UPDATE THE CARD TYPES STATUS
            var cardTypes = await _dbContext.CardTypes
                .Where(x => CardConfigDTO.Select(x => x.CardTypeId).Contains(x.Id)).ToListAsync();

            cardTypes.ForEach(x => x.CardStatus = CardStatus.Approved);

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

            //VALIDATES INCOMING CARD CONFIG I.E. THE COUNTRIES IDs, THE CARD TYPE IDs.
            var validateCardConfig =
                await ValidateCardConfig(CardId, model.ReceiptTypeCardConfigDTO.Select(x => x.CountryId).ToList(), model.ReceiptTypeCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                return new BaseResponse<bool>(validateCardConfig.ResponseMessage, validateCardConfig.Errors);                               
            }

            //CHECK IF CARD TYPE HASN'T BEEN ALREADY CONFGURED.
            var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => model.ReceiptTypeCardConfigDTO.Select(x => x.CardTypeId).Contains(x.CardTypeId)).ToListAsync();

            if (checkCardConfig.Any())
            {
                Errors.Add(ResponseMessage.CardTypeConfigured);
                return new BaseResponse<bool>(ResponseMessage.CardTypeConfigured, Errors);
            };

           //VALIDATES DENOMINATION
            var validateDenomination = ValidateDenomination(model.ReceiptTypeCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

            //VALIDATES RECEIPT TYPE
            var validateReceipt = ValidateReceipt(model.ReceiptTypeCardConfigDTO.Select(x => x.ReceiptTypeId).ToList());

            if (validateReceipt)
            {
                Errors.Add(ResponseMessage.CardReceiptNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardReceiptNotFound, Errors);
            }

            //CONFIGURE RECEIPT TYPE CARD TYPE DENOMINATION
            await CreateReceiptTypeDenomination(model.ReceiptTypeCardConfigDTO, UserId);

            return new BaseResponse<bool>(true, ResponseMessage.CardConfigSuccess);
        }

        /// <summary>
        /// CONFIGURE CREATE RECEIPT CARD TYPE
        /// </summary>
        /// <param name="ReceiptTypeCardConfigDTO">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task CreateReceiptTypeDenomination(List<ReceiptTypeCardConfigDTO> ReceiptTypeCardConfigDTO, Guid UserId)
        {
            //Add CardType Denomination
            var cardTypeDenominations = new List<CardTypeDenomination>();

            //LOOP THROUGH EACH OF THE RECEIPT TYPE CARD TYPE CONFIG MODEL
            foreach (var receiptTypeCardConfigDTO in ReceiptTypeCardConfigDTO)
            {
                cardTypeDenominations.Add(new CardTypeDenomination
                {
                    CardTypeId = receiptTypeCardConfigDTO.CardTypeId,
                    DenominationId = receiptTypeCardConfigDTO.DenominationId,
                    Rate = receiptTypeCardConfigDTO.Rate,
                    ReceiptId = receiptTypeCardConfigDTO.ReceiptTypeId,
                    CreatedBy = UserId
                });
            }

            _dbContext.CardTypeDenomination.AddRange(cardTypeDenominations);

            //UPDATES CARD TYPE STATUS
            var cardTypes = await _dbContext.CardTypes.Where(x => ReceiptTypeCardConfigDTO.Select(x => x.CardTypeId).Contains(x.Id)).ToListAsync();

            cardTypes.ForEach(x => x.CardStatus = CardStatus.Approved);

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

            //VALIDATES INCOMING CARD CONFIG I.E. THE COUNTRIES IDs, THE CARD TYPE IDs.
            var validateCardConfig = 
                await ValidateCardConfig(CardId, model.VisaCardConfigDTO.Select(x => x.CountryId).ToList(), model.VisaCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                return new BaseResponse<bool>(validateCardConfig.ResponseMessage, validateCardConfig.Errors);
            }

            //CHECK IF CARD TYPE HASN'T BEEN ALREADY CONFGURED.
            var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => model.VisaCardConfigDTO.Select(x => x.CardTypeId).Contains(x.CardTypeId)).ToListAsync();

            if (checkCardConfig.Any())
            {
                Errors.Add(ResponseMessage.CardTypeConfigured);
                return new BaseResponse<bool>(ResponseMessage.CardTypeConfigured, Errors);
            };


            //VALIDATES DENOMINATION
            var validateDenomination = ValidateDenomination(model.VisaCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

           //VALIDATES PREFIX
            var validatePrefix = ValidatePrefix(model.VisaCardConfigDTO.Select(x => x.PrefixId).ToList());

            if (validatePrefix)
            {
                Errors.Add(ResponseMessage.CardReceiptNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardReceiptNotFound, Errors);
            }

            //CONFIGURES VISA CARD TYPE DENOMINATION
            await CreateVisaDenomination(model.VisaCardConfigDTO, UserId);


            return new BaseResponse<bool>(true, ResponseMessage.CardConfigSuccess);
        }

        /// <summary>
        /// CONFIGURE CREATE VISA CARD TYPE
        /// </summary>
        /// <param name="VisaCardConfigDTO">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Systems.Threadings.Tasks.Task</returns>
        private async Task CreateVisaDenomination(List<VisaCardConfigDTO> VisaCardConfigDTO, Guid UserId)
        {
            
            var cardTypeDenominations = new List<CardTypeDenomination>();

            //LOOP THROUGH THE VISA CARD CONFIG MODEL
            foreach (var visaCardConfig in VisaCardConfigDTO)
            {

                cardTypeDenominations.Add(new CardTypeDenomination
                {
                    CardTypeId = visaCardConfig.CardTypeId,
                    DenominationId = visaCardConfig.DenominationId,
                    Rate = visaCardConfig.Rate,
                    PrefixId = visaCardConfig.PrefixId,
                    CreatedBy = UserId
                });

            }

            _dbContext.CardTypeDenomination.AddRange(cardTypeDenominations);

           //UPDATE CARD TYPE STATUS
            var cardTypes = await _dbContext.CardTypes.Where(x => VisaCardConfigDTO.Select(x => x.CardTypeId).Contains(x.Id)).ToListAsync();

            cardTypes.ForEach(x => x.CardStatus = CardStatus.Approved);

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
         
            var cards = _dbContext.Cards.AsNoTracking().AsQueryable();

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

            var cards = _dbContext.Cards.AsNoTracking().Where(x => cardTypes.Select(x => x.CardId).Contains(x.Id)).AsQueryable();

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
        /// UPDATE CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateCard(UpdateCardDTO model, Guid UserId, Guid CardId)
        {
            var uploadedFileToDelete = string.Empty;

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
                    return new BaseResponse<bool>(ResponseMessage.CardCreationFailure, countryValidation.Errors);
                };

                //CHECK IF INCOMING UPDATED CARD NAME DOESN'T ALREADY EXISTS.
                if (model.Name.Replace(" ", "").ToLower() != card.Name.Replace(" ", "").ToLower())
                {
                    var checkExistingCard = await _dbContext.Cards.AnyAsync(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

                    if (checkExistingCard)
                    {
                        Errors.Add(ResponseMessage.CardExist);
                        return new BaseResponse<bool>(ResponseMessage.CardExist, Errors);                        
                    }
                }

                var getExistingCards = await _dbContext.Cards.Where(x => x.Id == card.Id).Include(x => x.CardType).FirstOrDefaultAsync();
                var existingsCountryIds = getExistingCards.CardType.Select(x => x.CountryId);
                var getNewCountryIds = countryValidation.Data.CountryIds.Where(x => !existingsCountryIds.Contains(x));

                //CREATES CARD TYPE FOR NEW INCOMING COUNTRY IDs.
                var cardTypes = CreateCardTypes(getNewCountryIds.ToList(), UserId);
                card.CardType.AddRange(cardTypes);
                card.Name = string.IsNullOrWhiteSpace(model.Name) ? card.Name : model.Name;
                card.ModifiedBy = UserId;
                card.ModifiedOn = DateTime.UtcNow;

                //UPDATES CARD LOGO OR CREATES IF DOESN'T EXISTS.
                if (!(model.Logo is null) && !(card.LogoUrl is null))
                {

                    _logger.Info("Preparing to Delete Image From Cloudinary... at ExecutionPoint:UpdateCard");
                    var fullPath = GenerateDeleteUploadedPath(card.LogoUrl);
                    await _cloudinaryServices.DeleteImage(fullPath);
                    _logger.Info("Successfully Deleted Image From Cloudinary... at ExecutionPoint:UpdateCard");

                    _logger.Info("Uploading Image to Cloudinary... at ExecutionPoint:UpdateCard");
                    var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(model.Logo);
                    _logger.Info("Successfully Uploaded to Cloudinary... at ExecutionPoint:UpdateCard");

                    card.LogoUrl = uploadedFile;
                }
                if (!(model.Logo is null) && (card.LogoUrl is null))
                {
                    _logger.Info("Uploading Image to Cloudinary... at ExecutionPoint:UpdateCard");
                    var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(model.Logo);
                    _logger.Info("Successfully Uploaded to Cloudinary... at ExecutionPoint:UpdateCard");

                    card.LogoUrl = uploadedFile;
                }

                _logger.Info("About To Update Card... at Execution:UpdateCard");
                await _dbContext.SaveChangesAsync();
                _logger.Info("Successfully Updated Card... at Execution:UpdateCard");

                return new BaseResponse<bool>(true, ResponseMessage.CardUpdate);

            }
            catch (Exception ex)
            {
                await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(uploadedFileToDelete));
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
        public async Task<BaseResponse<bool>> UpdateVisaCard(UpdateVisaCardConfigDTO model, Guid UserId, Guid CardId)
        {
            var response = new BaseResponse<bool>();

            //VALIDATES CARD ID. 
            var card = await FindCard(CardId);

            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
            }

            //VALIDATES INCOMING CARD CONFIG I.E. THE COUNTRIES IDs, THE CARD TYPE IDs.
            var validateCardConfig =
                await ValidateCardConfig(CardId, model.VisaCardUpdateConfigDTO.Select(x => x.CountryId).ToList(), model.VisaCardUpdateConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                return new BaseResponse<bool>(validateCardConfig.ResponseMessage, validateCardConfig.Errors);
            };

            //VALIDATES DENOMINATION
            var validateDenomination = ValidateDenomination(model.VisaCardUpdateConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

            //VALIDATES PREFIX
            var validatePrefix = ValidatePrefix(model.VisaCardUpdateConfigDTO.Select(x => x.PrefixId).ToList());

            if (validatePrefix)
            {
                Errors.Add(ResponseMessage.CardReceiptNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardReceiptNotFound, Errors);
            }

            //VALIDATES CARD TYPE DENOMINATION 
            var validateCardTypeDenomination = ValidateCardTypeDenomination(model.VisaCardUpdateConfigDTO.Select(x => x.CardTypeDenominationId).ToList());
            
            if (validateCardTypeDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

            //UPDATES THE ALREADY CONFIGURED VISA CARD TYPE DENOMINATION
            await UpdateVisa(model.VisaCardUpdateConfigDTO, UserId);

            return new BaseResponse<bool>(true, ResponseMessage.CardUpdate);
        }

        /// <summary>
        /// UPDATE VISA CARD
        /// </summary>
        /// <param name="VisaCardUpdateConfigDTO">The VisaCardUpdateConfigDTO.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task UpdateVisa(List<VisaCardUpdateConfigDTO> VisaCardUpdateConfigDTO, Guid UserId)
        {
            var newCardTypeDenomination = new List<CardTypeDenomination>();

            //LOOP THROUGH INCOMING VISA CARD UPDATE MODEL
            foreach (var visaCardUpdateConfigDTO in VisaCardUpdateConfigDTO)
            {
                //CHECK IF THE CARD TYPE DENOMINATION ALREADY EXISTS, CONFIGURES A VISA CARD TYPE DENOMINATION IF IT DOESN'T
                var cardTypeDenomination = _dbContext.CardTypeDenomination.FirstOrDefault(x => x.CardTypeId == visaCardUpdateConfigDTO.CardTypeId);

                if (!(cardTypeDenomination is null))
                {
                    //UPDATE
                    cardTypeDenomination.PrefixId = visaCardUpdateConfigDTO.PrefixId;
                    cardTypeDenomination.DenominationId = visaCardUpdateConfigDTO.DenominationId;
                    cardTypeDenomination.Rate = visaCardUpdateConfigDTO.Rate;
                    cardTypeDenomination.ModifiedBy = UserId;
                    cardTypeDenomination.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    //CONFIGURES A VISA CARD TYPE DENOMINATION
                    newCardTypeDenomination.Add(new CardTypeDenomination
                    {
                        CardTypeId = visaCardUpdateConfigDTO.CardTypeId,
                        DenominationId = visaCardUpdateConfigDTO.DenominationId,
                        Rate = visaCardUpdateConfigDTO.Rate,
                        PrefixId = visaCardUpdateConfigDTO.PrefixId,
                        CreatedBy = UserId
                    });

                    //UPDATE CARD TYPE STATUS
                    var cardType = await _dbContext.CardType.FirstOrDefaultAsync(x => x.Id == visaCardUpdateConfigDTO.CardTypeId);
                    cardType.CardStatus = CardStatus.Approved;
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
        public async Task<BaseResponse<bool>> UpdateReceiptCard(UpdateReceiptTypeConfigDTO model, Guid UserId, Guid CardId)
        {
            var response = new BaseResponse<bool>();

            //VALIDATES CARD ID.
            var card = await FindCard(CardId);

            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
            };

            //VALIDATES INCOMING CARD CONFIG I.E. THE COUNTRIES IDs, THE CARD TYPE IDs.
            var validateCardConfig =
                await ValidateCardConfig(CardId, model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.CountryId).ToList(), model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                return new BaseResponse<bool>(validateCardConfig.ResponseMessage, validateCardConfig.Errors);
            };

            //VALIDATES DENOMINATION
            var validateDenomination = ValidateDenomination(model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

            //VALIDATES RECEIPT
            var validateReceipt = ValidateReceipt(model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.ReceiptId).ToList());

            if (validateReceipt)
            {
                Errors.Add(ResponseMessage.CardReceiptNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardReceiptNotFound, Errors);
            }


            //VALIDATES CARD TYPE DENOMINATION
            var validateCardTypeDenomination = ValidateCardTypeDenomination(model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.CardTypeDenominationId).ToList());
            if (validateCardTypeDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

            //UPDATES RECEIPT TYPE CARD CONFIG
            await UpdateReceiptType(model.ReceiptTypeUpdateCardConfigDTO, UserId);

            return new BaseResponse<bool>(true, ResponseMessage.CardUpdate);
        }

        /// <summary>
        /// UPDATE VISA CARD
        /// </summary>
        /// <param name="ReceiptTypeUpdateCardConfigDTO">The ReceiptTypeUpdateCardConfigDTO.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task UpdateReceiptType(List<ReceiptTypeUpdateConfigDTO> ReceiptTypeUpdateCardConfigDTO, Guid UserId)
        {
            var newCardTypeDenomination = new List<CardTypeDenomination>();

            ////LOOP THROUGH INCOMING VISA CARD UPDATE MODEL
            foreach (var receiptTypeUpdateConfigDTO in ReceiptTypeUpdateCardConfigDTO)
            {
                //CHECK IF THE CARD TYPE DENOMINATION ALREADY EXISTS, CONFIGURES A RECEIPT CARD TYPE DENOMINATION IF IT DOESN'T
                var cardTypeDenomination = _dbContext.CardTypeDenomination.FirstOrDefault(x => x.CardTypeId == receiptTypeUpdateConfigDTO.CardTypeId);

                if (!(cardTypeDenomination is null))
                {
                    //UPDATE
                    cardTypeDenomination.ReceiptId = receiptTypeUpdateConfigDTO.ReceiptId;
                    cardTypeDenomination.DenominationId = receiptTypeUpdateConfigDTO.DenominationId;
                    cardTypeDenomination.Rate = receiptTypeUpdateConfigDTO.Rate;
                    cardTypeDenomination.ModifiedBy = UserId;
                    cardTypeDenomination.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    //CONFIGURES A RECEIPT CARD TYPE DENOMINATION
                    newCardTypeDenomination.Add(new CardTypeDenomination
                    {
                        CardTypeId = receiptTypeUpdateConfigDTO.CardTypeId,
                        DenominationId = receiptTypeUpdateConfigDTO.DenominationId,
                        Rate = receiptTypeUpdateConfigDTO.Rate,
                        ReceiptId = receiptTypeUpdateConfigDTO.ReceiptId,
                        CreatedBy = UserId
                    });

                    //UPDATE CARD TYPE STATUS
                    var cardType = await _dbContext.CardType.FirstOrDefaultAsync(x => x.Id == receiptTypeUpdateConfigDTO.CardTypeId);
                    cardType.CardStatus = CardStatus.Approved;
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
        public async Task<BaseResponse<bool>> UpdateNormalCard(UpdateNormalCardConfigDTO model, Guid UserId, Guid CardId)
        {
            var response = new BaseResponse<bool>();

            //VALIDATES CARD ID.
            var card = await FindCard(CardId);

            if (card is null)
            {
                Errors.Add(ResponseMessage.CardNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
            };

            //VALIDATES INCOMING CARD CONFIG I.E. THE COUNTRIES IDs, THE CARD TYPE IDs.
            var validateCardConfig =
                await ValidateCardConfig(CardId, model.UpdateCardConfigDTO.Select(x => x.CountryId).ToList(), model.UpdateCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                return new BaseResponse<bool>(validateCardConfig.ResponseMessage, validateCardConfig.Errors);
            };

            //VALIDATES DENOMINATION
            var validateDenomination = ValidateDenomination(model.UpdateCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

            //VALIDATES CARD TYPE DENOMINATION
            var validateCardTypeDenomination = ValidateCardTypeDenomination(model.UpdateCardConfigDTO.Select(x => x.CardTypeDenominationId).ToList());
            if (validateCardTypeDenomination)
            {
                Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
            }

            //UPDATES NORMAL CARD 
            await UpdateNormalCard(model.UpdateCardConfigDTO, UserId);

            return new BaseResponse<bool>(true, ResponseMessage.CardUpdate);
        }

        /// <summary>
        /// UPDATE NORMAL CARD
        /// </summary>
        /// <param name="UpdateCardConfigDTO">The UpdateCardConfigDTO.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task UpdateNormalCard(List<UpdateCardConfigDTO> UpdateCardConfigDTO, Guid UserId)
        {
            var newCardTypeDenomination = new List<CardTypeDenomination>();

            ////LOOP THROUGH INCOMING VISA CARD UPDATE MODEL
            foreach (var updateNormalCardConfigDTO in UpdateCardConfigDTO)
            {
                //CHECK IF THE CARD TYPE DENOMINATION ALREADY EXISTS, CONFIGURES A RECEIPT CARD TYPE DENOMINATION IF IT DOESN'T
                var cardTypeDenomination = _dbContext.CardTypeDenomination.FirstOrDefault(x => x.CardTypeId == updateNormalCardConfigDTO.CardTypeId);

                if (!(cardTypeDenomination is null))
                {
                    //UPDATE
                    cardTypeDenomination.DenominationId = updateNormalCardConfigDTO.DenominationId;
                    cardTypeDenomination.Rate = updateNormalCardConfigDTO.Rate;
                    cardTypeDenomination.ModifiedBy = UserId;
                    cardTypeDenomination.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    //CONFIGURE A NORMAL CARD
                    newCardTypeDenomination.Add(new CardTypeDenomination
                    {
                        CardTypeId = updateNormalCardConfigDTO.CardTypeId,
                        DenominationId = updateNormalCardConfigDTO.DenominationId,
                        Rate = updateNormalCardConfigDTO.Rate,
                        CreatedBy = UserId
                    });

                    //UPDATE CARD STATUS
                    var cardType = await _dbContext.CardType.FirstOrDefaultAsync(x => x.Id == updateNormalCardConfigDTO.CardTypeId);
                    cardType.CardStatus = CardStatus.Approved;
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
        private bool ValidateCardTypes(Guid id, List<Guid> cardTypesIds)
        {
            var cardTypes = _dbContext.CardType.Where(x => x.CardId == id && cardTypesIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

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
        /// VALIDATE CARD CONFIG
        /// </summary>
        /// <param name="CardId">The CardId.</param>
        /// <param name="CountryIds">The countryIds.</param>
        /// <param name="CardTypeIds">The CardTypeIds.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        private async Task<BaseResponse<bool>> ValidateCardConfig(Guid CardId, List<Guid> CountryIds, List<Guid> CardTypeIds)
        {
            if (CountryIds == null || CountryIds == null || CardTypeIds == null)
            {
                Errors.Add("DATA CANNOT BE NULL");
                return new BaseResponse<bool>("DATA CANNOT BE NULL", Errors);
            }

            var card = await _dbContext.Cards
                .Include(x => x.CardType)
                .Where(x => x.Id == CardId)
                .FirstOrDefaultAsync();

            // RETURNS THE CARD TYPES FOR THIS CARD WHERE THE ID EXISTS FOR THE SPECIFIED CARD
            var cardTypes = card.CardType.Where(x => CardTypeIds.Contains(x.Id)).ToList();

            // IF THE COUNT OF CARDTYPE RETURNED IS NOT EQUAL TO THE CARDTYPEIDs RETURNED
            // THEN ONE ID IS WRONG
            if (cardTypes.Count != CardTypeIds.Count())
            {
                Errors.Add(ResponseMessage.CardTypeNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTypeNotFound, Errors);
            }


            var countryCardType = cardTypes.Where(x => CountryIds.Contains(x.CountryId)).ToList();

            // CHECK THE COUNTRIES RETURNED FOR THE CARDTYPE AGAINST THE NUMBER SENT
            // IF THE COUNTRIES RETURNED IS 2 AND THE COUNTRYIDs IS NOT EQUAL, THEN ONE ID IS WRONG
            if (countryCardType.Count != CountryIds.Distinct().Count())
            {
                Errors.Add(ResponseMessage.CardCountryTypeNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardCountryTypeNotFound, Errors);
            }

            return new BaseResponse<bool>();
        }


        /// <summary>
        /// VALIDATE CARD TYPE DENOMINATION
        /// </summary>
        /// <param name="cardTypeDenominationIds">The prefixIds.</param>
        /// <returns>System.boolean</returns>
        private bool ValidateCardTypeDenomination(List<Guid> cardTypeDenominationIds)
        {
            var cardTypeDenomination = _dbContext.CardTypeDenomination.Where(x => cardTypeDenominationIds.Contains(x.Id)).ToListAsync().Result.Select(x => x.Id);

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
            await _dbContext.Cards.FirstOrDefaultAsync(x => x.Id == id);


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

        
    }
}
