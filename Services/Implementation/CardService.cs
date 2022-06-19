using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
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
    public class CardService : ICardService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILog _logger;

        public CardService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = LogManager.GetLogger(typeof(ICardService));
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
                var result = new BaseResponse<CreatedCardDTO>();
                // TODO
                // WRAP ALL THIS IN A TRANSACTION

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

                var response = ValidateFile(model.Logo);

                if (response.Errors.Any())
                {
                    result.ResponseMessage = response.ResponseMessage;
                    result.Errors = response.Errors;
                    result.Status = RequestExecution.Failed;
                    return result;
                }

                //Upload to Cloudinary
                var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(model.Logo, _configuration);
                //Set this Property to delete uploaded cloudinary file if an exception occur
                uploadedFileToDelete = uploadedFile;

                var newCard = new Card()
                {
                    Name = model.Name,
                    LogoUrl = uploadedFile,
                    CreatedBy = UserId
                };

                var cardTypes = CreateCardTypes(countryValidation.Data.CountryIds, UserId);
                newCard.CardType.AddRange(cardTypes);

                await _dbContext.Cards.AddAsync(newCard);
                await _dbContext.SaveChangesAsync();

                result.Data = new CreatedCardDTO { Id = newCard.Id, Name = newCard.Name };
                result.ResponseMessage = "Card Created Successfully";
                return result;
            }
            catch (Exception ex)
            {
                CloudinaryUploadHelper.DeleteImage(_configuration, GenerateDeleteUploadedPath(uploadedFileToDelete));
                _logger.Error(ex.Message, ex);

                throw;
            }
            
        }


        /// <summary>
        /// CONFIGURE NORMAL CARD TYPE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> ConfigureNormalCard(ConfigureNormalCardDTO model, Guid UserId)
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
                await ValidateCardConfig(model.CardId, model.CardConfigDTO.Select(x => x.CountryId).ToList(), model.CardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                response.ResponseMessage = validateCardConfig.ResponseMessage;
                response.Errors = validateCardConfig.Errors;
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Check if CardType has already been configured
            var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => model.CardConfigDTO.Select(x => x.CardTypeId).Contains(x.CardTypeId)).ToListAsync();

            if (checkCardConfig.Any())
            {
                response.ResponseMessage = "Card Type has already been configured";
                response.Errors.Add("Card Type has already been configured");
                response.Status = RequestExecution.Failed;
                return response;
            };

            //Validate Denomination
            var validateDenomination = ValidateDenomination(model.CardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                response.ResponseMessage = "Denomination doesn't exists";
                response.Errors = new List<string> { "Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Add CardType Denomination
            var cardTypeDenominations = new List<CardTypeDenomination>();
            foreach (var cardConfig in model.CardConfigDTO)
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

            //Update CardTypes Status
            var cardTypes = await _dbContext.CardTypes.Where(x => model.CardConfigDTO.Select(x => x.CardTypeId).Contains(x.Id)).ToListAsync();

            foreach (var cardType in cardTypes)
            {
                cardType.CardStatus = CardStatus.Approved;
            }
           

            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Configured Normal Card";

            return response;

        }


        /// <summary>
        /// CONFIGURE RECEIPT CARD TYPE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> ConfigureReceiptTypeCard(ConfigureReceiptTypeCardDTO model, Guid UserId)
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
                await ValidateCardConfig(model.CardId, model.ReceiptTypeCardConfigDTO.Select(x => x.CountryId).ToList(), model.ReceiptTypeCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                response.ResponseMessage = validateCardConfig.ResponseMessage;
                response.Errors = validateCardConfig.Errors;
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Check if CardType has already been configured
            var checkCardConfig = await _dbContext.CardTypeDenomination.Where(x => model.ReceiptTypeCardConfigDTO.Select(x => x.CardTypeId).Contains(x.CardTypeId)).ToListAsync();

            if (checkCardConfig.Any())
            {
                response.ResponseMessage = "Card Type has already been configured";
                response.Errors.Add("Card Type has already been configured");
                response.Status = RequestExecution.Failed;
                return response;
            };

            //Validate Denomination
            var validateDenomination = ValidateDenomination(model.ReceiptTypeCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                response.ResponseMessage = "Denomination doesn't exists";
                response.Errors = new List<string> { "Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Validate Receipt Type
            var validatePrefix = ValidateReceipt(model.ReceiptTypeCardConfigDTO.Select(x => x.ReceiptTypeId).ToList());

            if (validatePrefix)
            {
                response.ResponseMessage = "Receipt doesn't exists";
                response.Errors = new List<string> { "Receipt doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Add CardType Denomination
            var cardTypeDenominations = new List<CardTypeDenomination>();
            foreach (var receiptTypeCardConfigDTO in model.ReceiptTypeCardConfigDTO)
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

            //Update CardTypes Status
            var cardTypes = await _dbContext.CardTypes.Where(x => model.ReceiptTypeCardConfigDTO.Select(x => x.CardTypeId).Contains(x.Id)).ToListAsync();

            foreach (var cardType in cardTypes)
            {
                cardType.CardStatus = CardStatus.Approved;
            }

            
            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Configured the Receipt Type Card";

            return response;
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
            foreach (var visaCardConfig in model.VisaCardConfigDTO)
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

            //Update CardTypes Status
            var cardTypes = await _dbContext.CardTypes.Where(x => model.VisaCardConfigDTO.Select(x => x.CardTypeId).Contains(x.Id)).ToListAsync();

            foreach (var cardType in cardTypes)
            {
                cardType.CardStatus = CardStatus.Approved;
            }
          
            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Configured the Visa Card";

            return response;
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
            response.ResponseMessage = "Successfully found the Card";

            return response;
        }


        /// <summary>
        /// GET ALL CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;CardDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardDTO>>> GetAllCard(BaseSearchViewModel model) 
        {
         
            var cards = _dbContext.Cards
                .Include(x => x.CardType).ThenInclude(x => x.Country)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .AsNoTracking()
                .AsQueryable();

            var pagedCards = await cards.OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardsDto = pagedCards.Select(x => (CardDTO)x).ToList();

            var data = new PagedList<CardDTO>(cardsDto, model.PageIndex, model.PageSize, pagedCards.TotalItemCount);

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"Found {cardsDto.Count} Card(s)." };

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

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"Found {cardsDto.Count} Card(s)." };
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

            return new BaseResponse<PagedList<CardDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"Found {cardsDto.Count} Card(s)." };
        }


        /// <summary>
        /// UPDATE CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateCard(UpdateCardDTO model, Guid UserId)
        {
            var uploadedFileToDelete = string.Empty;

            try
            {
                var response = new BaseResponse<bool>();

                var result = ValidateFile(model.Logo);

                if (response.Errors.Any())
                {
                    result.ResponseMessage = response.ResponseMessage;
                    result.Errors = response.Errors;
                    result.Status = RequestExecution.Failed;
                    return result;
                }

                var card = await FindCard(model.Id);

                if (card is null)
                {
                    response.ResponseMessage = "Card doesn't Exists";
                    response.Errors.Add("Card doesn't Exists");
                    response.Status = RequestExecution.Failed;
                    return response;
                };

                var countryValidation = ValidateCountry(model.CountryIds);

                if (countryValidation.Errors.Any())
                {
                    response.ResponseMessage = countryValidation.ResponseMessage;
                    response.Errors = countryValidation.Errors;
                    response.Status = RequestExecution.Failed;
                    return response;
                };

                //Check If Incoming Updated Card doesn't Exists.
                if (model.Name.Replace(" ", "").ToLower() != card.Name.Replace(" ", "").ToLower())
                {
                    var checkExistingCard = await _dbContext.Cards.AnyAsync(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

                    if (checkExistingCard)
                    {
                        response.Data = false;
                        response.ResponseMessage = "Card already Exists.";
                        response.Errors.Add("Card already Exists.");
                        response.Status = RequestExecution.Failed;
                        return response;
                    }
                }

                var getExistingCards = await _dbContext.Cards.Where(x => x.Id == card.Id).Include(x => x.CardType).FirstOrDefaultAsync();
                var existingsCountryIds = getExistingCards.CardType.Select(x => x.CountryId);
                var getNewCountryIds = countryValidation.Data.CountryIds.Where(x => !existingsCountryIds.Contains(x));


                var cardTypes = CreateCardTypes(getNewCountryIds.ToList(), UserId);
                card.CardType.AddRange(cardTypes);
                card.Name = string.IsNullOrWhiteSpace(model.Name) ? card.Name : model.Name;
                card.ModifiedBy = UserId;
                card.ModifiedOn = DateTime.UtcNow;

                //Update Card Logo
                if (!(model.Logo is null) && !(card.LogoUrl is null))
                {

                    var fullPath = GenerateDeleteUploadedPath(card.LogoUrl);
                    CloudinaryUploadHelper.DeleteImage(_configuration, fullPath);

                    var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(model.Logo, _configuration);

                    card.LogoUrl = uploadedFile;
                }

                if (!(model.Logo is null) && (card.LogoUrl is null))
                {
                    var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(model.Logo, _configuration);

                    card.LogoUrl = uploadedFile;
                }

                await _dbContext.SaveChangesAsync();

                result.Data = true;
                result.ResponseMessage = "Card Updated Successfully";
                return result;
            }
            catch (Exception ex)
            {
                CloudinaryUploadHelper.DeleteImage(_configuration, GenerateDeleteUploadedPath(uploadedFileToDelete));
                _logger.Error(ex.Message, ex);

                throw;
            }
            
        }


        /// <summary>
        /// UPDATE VISA CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateVisaCard(UpdateVisaCardConfigDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            var card = await FindCard(model.CardId);

            if (card is null)
            {
                response.ResponseMessage = "Card doesn't Exists";
                response.Errors.Add("Card doesn't Exists");
                response.Status = RequestExecution.Failed;
                return response;
            };

            //Validate Card Config
            var validateCardConfig =
                await ValidateCardConfig(model.CardId, model.VisaCardUpdateConfigDTO.Select(x => x.CountryId).ToList(), model.VisaCardUpdateConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                response.ResponseMessage = validateCardConfig.ResponseMessage;
                response.Errors = validateCardConfig.Errors;
                response.Status = RequestExecution.Failed;
                return response;

            };

            //Validate Denomination
            var validateDenomination = ValidateDenomination(model.VisaCardUpdateConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                response.ResponseMessage = "Denomination doesn't exists";
                response.Errors = new List<string> { "Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Validate Prefix
            var validatePrefix = ValidatePrefix(model.VisaCardUpdateConfigDTO.Select(x => x.PrefixId).ToList());

            if (validatePrefix)
            {
                response.ResponseMessage = "Prefix doesn't exists";
                response.Errors = new List<string> { "Prefix doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Validate Card Type Denomination
            var validateCardTypeDenomination = ValidateCardTypeDenomination(model.VisaCardUpdateConfigDTO.Select(x => x.CardTypeDenominationId).ToList());
            if (validateCardTypeDenomination)
            {
                response.ResponseMessage = "Card Type Denomination doesn't exists";
                response.Errors = new List<string> { "Card Type Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            var newCardTypeDenomination = new List<CardTypeDenomination>();

            foreach (var visaCardUpdateConfigDTO in model.VisaCardUpdateConfigDTO) 
            {
                var cardTypeDenomination = _dbContext.CardTypeDenomination.FirstOrDefault(x => x.CardTypeId == visaCardUpdateConfigDTO.CardTypeId);

                if (!(cardTypeDenomination is null))
                {
                    //Update
                    cardTypeDenomination.PrefixId = visaCardUpdateConfigDTO.PrefixId;
                    cardTypeDenomination.DenominationId = visaCardUpdateConfigDTO.DenominationId;
                    cardTypeDenomination.Rate = visaCardUpdateConfigDTO.Rate;
                    cardTypeDenomination.ModifiedBy = UserId;
                    cardTypeDenomination.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    //Create new 
                    newCardTypeDenomination.Add(new CardTypeDenomination
                    {
                        CardTypeId = visaCardUpdateConfigDTO.CardTypeId,
                        DenominationId = visaCardUpdateConfigDTO.DenominationId,
                        Rate = visaCardUpdateConfigDTO.Rate,
                        PrefixId = visaCardUpdateConfigDTO.PrefixId,
                        CreatedBy = UserId
                    });

                    //Update CardType 
                    var cardType = await _dbContext.CardType.FirstOrDefaultAsync(x => x.Id == visaCardUpdateConfigDTO.CardTypeId);
                    cardType.CardStatus = CardStatus.Approved;
                }

            }

            await _dbContext.CardTypeDenomination.AddRangeAsync(newCardTypeDenomination);
            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Updated the Visa Card";

            return response;

        }


        /// <summary>
        /// UPDATE RECEIPT TYPE CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateReceiptCard(UpdateReceiptTypeConfigDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            var card = await FindCard(model.CardId);

            if (card is null)
            {
                response.ResponseMessage = "Card doesn't Exists";
                response.Errors.Add("Card doesn't Exists");
                response.Status = RequestExecution.Failed;
                return response;
            };

            //Validate Card Config
            var validateCardConfig =
                await ValidateCardConfig(model.CardId, model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.CountryId).ToList(), model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                response.ResponseMessage = validateCardConfig.ResponseMessage;
                response.Errors = validateCardConfig.Errors;
                response.Status = RequestExecution.Failed;
                return response;

            };

            //Validate Denomination
            var validateDenomination = ValidateDenomination(model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                response.ResponseMessage = "Denomination doesn't exists";
                response.Errors = new List<string> { "Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Validate Prefix
            var validateReceipt = ValidateReceipt(model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.ReceiptId).ToList());

            if (validateReceipt)
            {
                response.ResponseMessage = "Receipt Type doesn't exists";
                response.Errors = new List<string> { "Receipt Type doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }


            //Validate Card Type Denomination
            var validateCardTypeDenomination = ValidateCardTypeDenomination(model.ReceiptTypeUpdateCardConfigDTO.Select(x => x.CardTypeDenominationId).ToList());
            if (validateCardTypeDenomination)
            {
                response.ResponseMessage = "Card Type Denomination doesn't exists";
                response.Errors = new List<string> { "Card Type Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            var newCardTypeDenomination = new List<CardTypeDenomination>();

            foreach (var receiptTypeUpdateConfigDTO in model.ReceiptTypeUpdateCardConfigDTO)
            {
                var cardTypeDenomination = _dbContext.CardTypeDenomination.FirstOrDefault(x => x.CardTypeId == receiptTypeUpdateConfigDTO.CardTypeId);

                if (!(cardTypeDenomination is null))
                {
                    //Update
                    cardTypeDenomination.ReceiptId = receiptTypeUpdateConfigDTO.ReceiptId;
                    cardTypeDenomination.DenominationId = receiptTypeUpdateConfigDTO.DenominationId;
                    cardTypeDenomination.Rate = receiptTypeUpdateConfigDTO.Rate;
                    cardTypeDenomination.ModifiedBy = UserId;
                    cardTypeDenomination.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    //Create new 
                    newCardTypeDenomination.Add(new CardTypeDenomination
                    {
                        CardTypeId = receiptTypeUpdateConfigDTO.CardTypeId,
                        DenominationId = receiptTypeUpdateConfigDTO.DenominationId,
                        Rate = receiptTypeUpdateConfigDTO.Rate,
                        ReceiptId = receiptTypeUpdateConfigDTO.ReceiptId,
                        CreatedBy = UserId
                    });

                    //Update CardType 
                    var cardType = await _dbContext.CardType.FirstOrDefaultAsync(x => x.Id == receiptTypeUpdateConfigDTO.CardTypeId);
                    cardType.CardStatus = CardStatus.Approved;
                }

            }

            await _dbContext.CardTypeDenomination.AddRangeAsync(newCardTypeDenomination);
            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Updated the Receipt Card";

            return response;

        }


        /// <summary>
        /// UPDATE NORMAL CARD
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateNormalCard(UpdateNormalCardConfigDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            var card = await FindCard(model.CardId);

            if (card is null)
            {
                response.ResponseMessage = "Card doesn't Exists";
                response.Errors.Add("Card doesn't Exists");
                response.Status = RequestExecution.Failed;
                return response;
            };

            //Validate Card Config
            var validateCardConfig =
                await ValidateCardConfig(model.CardId, model.UpdateCardConfigDTO.Select(x => x.CountryId).ToList(), model.UpdateCardConfigDTO.Select(x => x.CardTypeId).ToList());

            if (validateCardConfig.Errors.Any())
            {
                response.ResponseMessage = validateCardConfig.ResponseMessage;
                response.Errors = validateCardConfig.Errors;
                response.Status = RequestExecution.Failed;
                return response;

            };

            //Validate Denomination
            var validateDenomination = ValidateDenomination(model.UpdateCardConfigDTO.Select(x => x.DenominationId).ToList());

            if (validateDenomination)
            {
                response.ResponseMessage = "Denomination doesn't exists";
                response.Errors = new List<string> { "Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            //Validate Card Type Denomination
            var validateCardTypeDenomination = ValidateCardTypeDenomination(model.UpdateCardConfigDTO.Select(x => x.CardTypeDenominationId).ToList());
            if (validateCardTypeDenomination)
            {
                response.ResponseMessage = "Card Type Denomination doesn't exists";
                response.Errors = new List<string> { "Card Type Denomination doesn't exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            var newCardTypeDenomination = new List<CardTypeDenomination>();

            foreach (var updateNormalCardConfigDTO in model.UpdateCardConfigDTO)
            {
                var cardTypeDenomination = _dbContext.CardTypeDenomination.FirstOrDefault(x => x.CardTypeId == updateNormalCardConfigDTO.CardTypeId);

                if (!(cardTypeDenomination is null))
                {
                    //Update
                    cardTypeDenomination.DenominationId = updateNormalCardConfigDTO.DenominationId;
                    cardTypeDenomination.Rate = updateNormalCardConfigDTO.Rate;
                    cardTypeDenomination.ModifiedBy = UserId;
                    cardTypeDenomination.ModifiedOn = DateTime.UtcNow;
                }
                else
                {
                    //Create new 
                    newCardTypeDenomination.Add(new CardTypeDenomination
                    {
                        CardTypeId = updateNormalCardConfigDTO.CardTypeId,
                        DenominationId = updateNormalCardConfigDTO.DenominationId,
                        Rate = updateNormalCardConfigDTO.Rate,
                        CreatedBy = UserId
                    });

                    //Update CardType 
                    var cardType = await _dbContext.CardType.FirstOrDefaultAsync(x => x.Id == updateNormalCardConfigDTO.CardTypeId);
                    cardType.CardStatus = CardStatus.Approved;
                }

            }

            await _dbContext.CardTypeDenomination.AddRangeAsync(newCardTypeDenomination);
            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Updated the Normal Card";

            return response;
        }


        /// <summary>
        /// DELETE CARD TYPE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteCardType(DeleteCardTypeDTO model)
        {
            var response = new BaseResponse<bool>();

            var card = await FindCard(model.CardId);

            if (card is null)
            {
                response.ResponseMessage = "Card doesn't Exists";
                response.Errors.Add("Card doesn't Exists");
                response.Status = RequestExecution.Failed;
                return response;
            };

            var validateCardType = ValidateCardTypes(model.CardId, model.CardTypeIds);

            if (validateCardType)
            {
                response.ResponseMessage = "Card Type doesn't exists for this Card";
                response.Errors.Add("Card Type doesn't exists for this Card");
                response.Status = RequestExecution.Failed;
                return response;
            }

            var getCardType = await _dbContext.CardType.Where(x => x.CardId == card.Id && model.CardTypeIds.Contains(x.Id)).ToListAsync();
            
            if (await _dbContext.CardTypeDenomination.AnyAsync(x => getCardType.Select(x => x.Id).Contains(x.CardTypeId)))
            {
                response.ResponseMessage = "You Cannot delete this Card Type";
                response.Errors.Add("You Cannot delete this Card Type");
                response.Status = RequestExecution.Failed;
                return response;
            }

            _dbContext.CardType.RemoveRange(getCardType);
            await _dbContext.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Deleted the Card Type";

            return response;
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
        private List<CardType> CreateCardTypes(List<Guid> CountryIds, Guid UserId)
        {
            List<CardType> cardTypes = new List<CardType>();

            foreach (var countryId in CountryIds)
            {

                var newCardEcodeType = new CardType
                {
                    CountryId = countryId,
                    CardCategory = CardCategory.E_CODE,
                    CreatedBy = UserId,
                    CardStatus = CardStatus.Pending
                };

                cardTypes.Add(newCardEcodeType);

                var newCardPhysicalType = new CardType
                {
                    CountryId = countryId,
                    CardCategory = CardCategory.PHYSICAL,
                    CreatedBy = UserId,
                    CardStatus = CardStatus.Pending
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

            if (CountryIds != null && CardTypeIds != null)
            {
                if (cardTypes.Count != CardTypeIds.Count())
                    return new BaseResponse<bool>
                    {
                        ResponseMessage = "Card Type doesn't exists for this Card",
                        Errors = new List<string> { "Card Type doesn't exists for this Card" }
                    };

            }


            if (CardTypeIds != null && CountryIds != null)
            {
                //Check Country in CardType
                var countryCardType = cardTypes.Where(x => CountryIds.Contains(x.CountryId)).ToList();
                if (countryCardType.Count != CountryIds.Count())
                    return new BaseResponse<bool>
                    {
                        ResponseMessage = "Country doesn't exists for this Card Type",
                        Errors = new List<string> { "Country doesn't exists for this Card Type" }
                    };
            }


            return new BaseResponse<bool> { };

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
        /// VALIDATE FILE
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private BaseResponse<bool> ValidateFile(IFormFile file)
        {
            var response = new BaseResponse<bool>();

            if (!(file is null))
            {
                if (file.Length > 1024 * 1024)
                {
                    response.ResponseMessage = "Logo file size must not exceed 1Mb";
                    response.Errors.Add("Logo file size must not exceed 1Mb");
                    response.Status = RequestExecution.Failed;
                    return response;
                }

                var error = ValidateFileTypeHelper.ValidateFile(new[] { "jpg", "png", "jpeg" }, file.FileName);

                if (!error)
                {
                    response.ResponseMessage = "Logo file type must be .jpg or .png or .jpeg";
                    response.Errors.Add("Logo file type must be .jpg or .png or .jpeg");
                    response.Status = RequestExecution.Failed;
                    return response;
                }

            }

            return response;
        }


        /// <summary>
        /// GENERATE DELETE UPLOADED PATH
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>BaseResponse&lt;bool&gt;.</returns>
        private string GenerateDeleteUploadedPath(string value)
        {
            //Delete Image From Cloudinary
            var splittedLogoUrl = value.Split("/");

            //get the cloudinary PublicId
            var LogoPublicId = splittedLogoUrl[8];
            var splittedLogoPublicId = LogoPublicId.Split(".");

            //Get the Full Asset Path
            var fullPath = $"Optima/{splittedLogoPublicId[0]}";

            return fullPath;
        }
    }
}
