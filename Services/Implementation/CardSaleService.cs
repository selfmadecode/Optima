using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.CardTransactionDTOs;
using Optima.Models.DTO.NotificationDTO;
using Optima.Models.DTO.SignalRNotificationDTO;
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
    public class CardSaleService : BaseService, ICardSaleService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILog _logger;
        private readonly ICloudinaryServices _cloudinaryServices;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<SignalRService, ISignalRService> _signalRNotificationService;
        private readonly IPushNotificationService _pushNotificationService;
        private static Random random = new Random();


        public CardSaleService(ApplicationDbContext context, ICloudinaryServices cloudinaryServices, 
            INotificationService notificationService, IHubContext<SignalRService, ISignalRService> signalRNotificationService,
             UserManager<ApplicationUser> userManager, IPushNotificationService pushNotificationService)
        {
            _context = context;
            _cloudinaryServices = cloudinaryServices;
            _logger = LogManager.GetLogger(typeof(CardSaleService));
            _notificationService = notificationService;
            _pushNotificationService = pushNotificationService;
            _signalRNotificationService = signalRNotificationService;
            _userManager = userManager;
        }


        /// <summary>
        /// CREATE CARD FOR SALES
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateCardSales(SellCardDTO model, Guid UserId)
        {
            var filesToDelete = new List<string>();

            try
            {
                // GET THE CARD BY ID AND THE USER SELECTED CARDTYPEID AND COUNTRY
                var card = _context.Cards
                    .Include(x => x.CardType).ThenInclude(x => x.CardTypeDenomination)
                    .FirstOrDefault(x => x.Id == model.CardId
                                && x.CardStatus == CardStatus.Approved);
                               

                if(card == null)
                {
                    Errors.Add(ResponseMessage.CardNotFoundOrApproved);
                    return new BaseResponse<bool>(ResponseMessage.CardNotFoundOrApproved, Errors);
                }

                var cardType = _context.CardType
                    .Include(x => x.CardTypeDenomination)
                    .Include(c => c.Card)
                    .FirstOrDefault(x => x.CountryId == model.CountryId && x.Id == model.CardTypeId && x.CardId == card.Id);

                if (cardType == null)
                {
                    Errors.Add(ResponseMessage.CardNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardNotFound, Errors);
                }

                var userSelectedDenominationIds = model.SelectedCardDenominationDTO.Select(x => x.DenominationId);

                IEnumerable<CardTypeDenomination> existingCardDenomination = null;

                switch (card.BaseCardType)
                {
                    case BaseCardType.REGULAR:
                        existingCardDenomination = cardType.CardTypeDenomination
                    .Where(x => userSelectedDenominationIds.Contains(x.DenominationId));
                        break;

                    case BaseCardType.AMAZON:
                        existingCardDenomination = cardType.CardTypeDenomination
                    .Where(x => userSelectedDenominationIds.Contains(x.DenominationId) && x.ReceiptId == model.SubTypeId);
                        break;

                    case BaseCardType.SPECIAL:
                        existingCardDenomination = cardType.CardTypeDenomination
                    .Where(x => userSelectedDenominationIds.Contains(x.DenominationId) && x.PrefixId == model.SubTypeId);
                        break;
                }

                if (existingCardDenomination.Count() != userSelectedDenominationIds.Distinct().Count())
                {
                    Errors.Add(ResponseMessage.CardTypeDenominationForCardNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationForCardNotFound, Errors);
                }

                decimal totalExpectedAmount = 0;

                var cardSold = new List<CardSold>();

                foreach (var item in model.SelectedCardDenominationDTO)
                {
                    var existingRate = existingCardDenomination.Where(x => x.DenominationId == item.DenominationId).FirstOrDefault();

                    totalExpectedAmount += existingRate.Rate;

                    cardSold.Add(new CardSold
                    {
                        Code = item.GiftCardCode,
                        CreatedOn = DateTime.Now,
                        Rate = existingRate.Rate,
                        DenominationId = item.DenominationId,
                        CreatedBy = UserId
                    });
                }

                var transactionImages = await UploadTransactionImages(model.CardImages, UserId);
                filesToDelete = transactionImages.Select(x => x.LogoUrl).ToList();

                var transaction = new CardTransaction
                {
                    TotalExpectedAmount = totalExpectedAmount,
                    ApplicationUserId = UserId,
                    CardTypeDenominationId = existingCardDenomination.FirstOrDefault().Id,
                    CreatedOn = DateTime.Now,
                    TransactionStatus = TransactionStatus.Pending,
                    TransactionRef ="", // Generate custom transactionRef
                    CardSold = cardSold,
                    TransactionUploadededFiles = transactionImages
                };

                _context.CardTransactions.Add(transaction);

                _logger.Info("About to Save Card Transaction, Card Sold and Card Transaction Uploaded Files... at ExecutionPoint:CreateCardSales");
                await _context.SaveChangesAsync();
                _logger.Info("Successfully Saved Card Transaction, Card Sold and Card Transaction Uploaded Files... at ExecutionPoint:CreateCardSales");

                //SIGNALR NOTIFICATION TO ADMIN
                var user = await GetUserById(UserId);             
                var data = CreateCardSaleNotificationDTO(user.FullName, "Card Sale");
                await _signalRNotificationService.Clients.Users(AdminUsers().Select(x => x.ToString()).ToList()).SendCardSaleNotification(data);
                
                //SAVE NOTIFICATION TO DB
                await SaveNotificationForAdmin(user.FullName);

                return new BaseResponse<bool>(true, ResponseMessage.CardSaleCreation);
            }
            catch (Exception ex)
            {
                //DELETES THE FILES ALREADY UPLOADED TO CLOUDINARY.
                foreach (var filePath in filesToDelete)
                {
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(filePath));
                        _logger.Error(ex.Message, ex);
                    }
                }

                _logger.Error(ex.Message, ex);

                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage999, Errors);
            }           
        }

        #region OLD CREATE CARD SALE
        /// <summary>
        /// CREATES CARD FOR SALE
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="UserId">The UserId</param>
        /// <returns>
        /// System.Value.Tuple&lt;(CardTransaction, List&lt;CardSold)&gt;.
        /// </returns>
        /// 

        /*
        private async Task<(List<CardTransaction>, List<string>)> CreateCardForSales(SellCardDTO model, Guid UserId)
        {
            decimal amount = 0;

            // GET THE CARD TYPE DENOMINATIONS
            var cardTypeDenominations = await _context.CardTypeDenomination
                .Where(x => model.CardTypeDTOs.Select(x => x.CardTypeDenominationId).Contains(x.Id))
                .ToListAsync();

            var cardTransaction = new List<CardTransaction>();
            var cardSold = new List<CardSold>();
            var cardCodes = new List<CardCodes>();

            //SET THE FILES TO DELETE INCASE AN EXCEPTION OCCUR WHILE CREATING THE CARD SALE.
            var filesToDelete = new List<string>();

            //LOOP THROUGH EACH OF THE INCOMING CARD SALE MODEL
            foreach (var sellCardDto in model.CardTypeDTOs)
            {
                //CALCULATES THE TOTAL EXPECTED AMOUNT THE ADMIN WOULD PAY FOR THIS CARD SALE TRANSACTION
                var aCardTypeDenomination = cardTypeDenominations.FirstOrDefault(x => x.Id == sellCardDto.CardTypeDenominationId);
                //CALCULATE THE EXPECTED AMOUNT FOR THAT PARTICULAR CARD SALE --> NOTE: SAME WITH CARD SOLD--> AMOUNT PROPERTY.
                amount = CalculateAmount(aCardTypeDenomination, sellCardDto.CardCodes);

                //SET THE TRANSACTION STATUS TO PENDING I.E. AWAITING APPROVAL FROM THE ADMIN
                var newcardTransaction = new CardTransaction
                {
                    TransactionStatus = TransactionStatus.Pending,
                    TotalExpectedAmount = amount,
                    ApplicationUserId = UserId,
                    CreatedBy = UserId,
                };

                //UPLOAD FILES TO CLOUDINARY FOR A PARTICULAR CARD SALE
                filesToDelete.AddRange(transactionImages.Select(x => x.LogoUrl));

                //CREATES THE CARD SOLD 
                var newCardSold = new CardSold
                {
                    CardTypeDenominationId = sellCardDto.CardTypeDenominationId,
                    //CALCULATE THE EXPECTED AMOUNT FOR THAT PARTICULAR CARD SALE
                    Amount = CalculateAmount(cardTypeDenominations.FirstOrDefault(x => x.Id == sellCardDto.CardTypeDenominationId), sellCardDto.CardCodes),
                    CreatedBy = UserId,
                };

                //LOOP THROUGH THAT PARTICULAR CARD CODES OF THE CARD SALE MODEL
                foreach (var cardCode in sellCardDto.CardCodes)
                {
                    //CREATES THE CARD CODES FOR THE CARD SALE
                    newCardSold.CardCodes.Add(new CardCodes
                    {
                        CardSoldId = newCardSold.Id,
                        CardCode = cardCode,
                        CreatedBy = UserId,
                    });
                }

                newcardTransaction.CardSold.Add(newCardSold);
                newcardTransaction.TransactionUploadededFiles.AddRange(transactionImages);
                cardTransaction.Add(newcardTransaction);
            }

            return (cardTransaction, filesToDelete);
        }

        */
        #endregion

        /// <summary>
        /// UPLOAD TRANSACTION IMAGES
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;List&lt;TransactionUploadFiles&gt;&gt;.</returns>
        private async Task<List<TransactionUploadFiles>> UploadTransactionImages(List<IFormFile> Files, Guid UserId)
        {

            var transactionUploadedFiles = new List<TransactionUploadFiles>();

            _logger.Info($"Uploading {Files.Count} Files to Cloudinary... at ExecutionPoint:UploadTransactionImages");
            foreach (var file in Files)
            {
                var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(file);
                transactionUploadedFiles.Add(new TransactionUploadFiles
                {
                    LogoUrl = uploadedFile,
                    CreatedBy = UserId,
                });
            }

            _logger.Info($"Successfully Uploaded {Files.Count} Images to Cloudinary... at ExecutionPoint:UploadTransactionImages");
            return transactionUploadedFiles;
        }

        /// <summary>
        /// GET ALL CARD SALES
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;GetAllCardSales&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<CardTransactionDTO>> GetCardSale(GetTransactionByIdDTO model)
        {
            var query = await _context.CardTransactions
                .Include(x => x.CardTypeDenomination).ThenInclude(x => x.CardType.Card)
                .Include(x => x.CardTypeDenomination.CardType.Country)// TO GET THE NAME
                //.Include(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                //.Include(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .Include(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination) // ??
                .Include(x => x.CardSold).ThenInclude(x => x.Denomination) //??
                .Include(x => x.TransactionUploadededFiles)
                .Include(x => x.ApplicationUser)
                .Include(x => x.ActionBy)
                .FirstOrDefaultAsync(x => x.Id == model.Id);


            if (query is null)
            {
                Errors.Add(ResponseMessage.CardTransactionNotFound);
                return new BaseResponse<CardTransactionDTO>(ResponseMessage.CardTransactionNotFound, Errors);
            }

            var data = new CardTransactionDTO
            {
                Id = query.Id,
                AmountPaid = query.AmountPaid,
                TotalExpectedAmount = query.TotalExpectedAmount,
                Status = query.TransactionStatus,             
                TransactionRefId = query.TransactionRef,
                CreatedOn = query.CreatedOn,
                CardDetails = new CardDetailsDTO
                {
                    CardName = query.CardTypeDenomination?.CardType?.Card?.Name,
                    Country = query.CardTypeDenomination?.CardType?.Country?.Name, // CHECK FOR NULL
                    Type = query.CardTypeDenomination?.CardType?.CardCategory.GetDescription()
                },
                ActionByUser = new ActionedByDTO
                {
                    FullName = query.ActionBy?.FullName,
                    UserId = query.ActionBy?.Id
                },
                CardTransactionImages = query.TransactionUploadededFiles.Select(x => new CardTransactionImagesDTO
                {
                    Id = x.Id,
                    CreatedOn = x.CreatedOn,
                    LogoUrl = x.LogoUrl
                }).ToList(),
                CustomerDetails = new CustomerDetailsDTO
                {
                    FullName = query.ApplicationUser.FullName,
                    PhoneNumber = query.ApplicationUser.PhoneNumber,
                    UserId = query.ApplicationUser.Id
                },
                CardSold  = query.CardSold.Select(x => new CardSoldDTO
                {
                    Amount = x.Rate,
                    Code = x.Code,
                    Denomination = x.Denomination.Amount
                }).ToList()
                
                
            };

            return new BaseResponse<CardTransactionDTO>(data, ResponseMessage.SuccessMessage000);
        }

        /// <summary>
        /// GET USER CARD TRANSACTIONS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardTransactionDTO>>> GetUserCardTransactions(BaseSearchViewModel model, Guid UserId)
        {

            var query = _context.CardTransactions
                .Where(x => x.ApplicationUserId == UserId)
                .Include(x => x.CardTypeDenomination.CardType.Card)
                .Include(x => x.CardTypeDenomination.CardType.Country)
                .Include(x => x.CardSold).ThenInclude(x => x.Denomination)
                //.Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                //.Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                //.Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .Include(x => x.TransactionUploadededFiles)
                .Include(x => x.ApplicationUser)
                .AsNoTracking()
                .AsQueryable();

            if (query.Any() is false)
            {
                Errors.Add(ResponseMessage.UserCardTransactionNotFound);
                return new BaseResponse<PagedList<CardTransactionDTO>>(ResponseMessage.UserCardTransactionNotFound, Errors);
            }
           
            var cardTransactions = await EntityFilter(model, query).OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);

            var cardTransactionsDto = cardTransactions.Select(x => new CardTransactionDTO
            {
                AmountPaid = x.AmountPaid,
                TotalExpectedAmount = x.TotalExpectedAmount,
                Status = x.TransactionStatus,
                Id = x.Id,
                TransactionRefId = x.TransactionRef,
                CreatedOn = x.CreatedOn,
                CardDetails = new CardDetailsDTO
                {
                    CardName = x.CardTypeDenomination?.CardType?.Card?.Name,
                    Country = x.CardTypeDenomination?.CardType?.Country?.Name, // CHECK FOR NULL
                    Type = x.CardTypeDenomination?.CardType?.CardCategory.GetDescription(),
                },
                ActionByUser = new ActionedByDTO
                {
                    FullName = x.ActionBy?.FullName,
                    UserId = x.ActionBy?.Id
                },
                CardTransactionImages = x.TransactionUploadededFiles.Select(x => new CardTransactionImagesDTO
                {
                    Id = x.Id,
                    CreatedOn = x.CreatedOn,
                    LogoUrl = x.LogoUrl
                }).ToList(),
                CustomerDetails = new CustomerDetailsDTO
                {
                    FullName = x.ApplicationUser.FullName,
                    PhoneNumber = x.ApplicationUser.PhoneNumber,
                    UserId = x.ApplicationUser.Id
                },
                CardSold = x.CardSold.Select(c => new CardSoldDTO
                {
                    Amount = c.Rate,
                    Code = c.Code,
                    Denomination = c.Denomination.Amount
                }).ToList()
            });

            var data = new PagedList<CardTransactionDTO>(cardTransactionsDto, model.PageIndex, model.PageSize, cardTransactions.TotalItemCount);
            return new BaseResponse<PagedList<CardTransactionDTO>> 
            { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {data.Count} CARD TRANSACTION(s)." };
        }

        #region OLD UPDATE CARD FOR SALE
        /// <summary>
        /// UPDATE CARD CODES FOR A CARD SALE TRANSACTION
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>

        public async Task<BaseResponse<bool>> UpdateCardSales(Guid transactionId, UpdateSellCardDTO model, Guid UserId)
        {
            /*
            //VALIDATES THE CARD TRANSACTION ID TO BE UPDATED
            var cardTransaction = await FindCardTransaction(transactionId);
                
            if (cardTransaction is null)
            {
                Errors.Add(ResponseMessage.CardTransactionNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTransactionNotFound, Errors);
            }

            //VALIDATES THE CARD SOLD IDs FOR THE CARD SALE TRANSACTION
            var cardSold = cardTransaction.CardSold.Where(x => model.UpdateCardSoldDTOs.Select(x => x.CardSoldId).Contains(x.Id)).ToList();

            if (cardSold.Count != model.UpdateCardSoldDTOs.Select(x => x.CardSoldId).Count())
            {
                Errors.Add(ResponseMessage.CardSoldNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardSoldNotFound, Errors);
            }

            //VALIDATES THE CARD CODES IDs FOR THE CARD TRANSACTION
            var cardCodes = cardTransaction.CardSold.Select(x => x.Code)
                .Where(x => model.UpdateCardSoldDTOs.SelectMany(x => x.UpdateCardCodeDTOs.Select(x => x.CardCodeId)).Contains(x.Id)).ToList();

            if (cardCodes.Count != model.UpdateCardSoldDTOs.SelectMany(x => x.UpdateCardCodeDTOs.Select(x => x.CardCodeId)).Count())
            {
                Errors.Add(ResponseMessage.CardCodesNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardCodesNotFound, Errors);
            }

            //UPDATES THE CARD SALES. THIS UPDATES THE CARD CODES.
            await CardSaleUpdate(model, cardCodes, UserId);

            
            return new BaseResponse<bool>(true, ResponseMessage.CardCodeUpdate);
            */
            return new BaseResponse<bool>();
        }
       

        /// <summary>
        /// CARD SALE UPDATE
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="cardCodes">The Card Codes</param>
        /// <param name="UserId">The UserId</param>
        /// <returns>System.Threading.Tasks.Task</returns>
        private async Task CardSaleUpdate(UpdateSellCardDTO model, List<CardCodes> cardCodes, Guid UserId)
        {
            //LOOP THROUGH THE INCOMING UPDATE SELL CARD MODEL 
            foreach (var UpdateCardSoldDTO in model.UpdateCardSoldDTOs)
            {
                //LOOP THROUGH THE INCOMING CARD CODES MODEL
                foreach (var cardCodeDto in UpdateCardSoldDTO.UpdateCardCodeDTOs)
                {
                    //TARGETS THE CARD CODE TO BE UPDATED
                    var aCardCodeSold = cardCodes.FirstOrDefault(x => x.Id == cardCodeDto.CardCodeId);

                    aCardCodeSold.CardCode = string.IsNullOrWhiteSpace(cardCodeDto.CardCode) ? aCardCodeSold.CardCode : cardCodeDto.CardCode;
                    aCardCodeSold.ModifiedBy = UserId;
                    aCardCodeSold.ModifiedOn = DateTime.UtcNow;

                    //_context.CardCodes.Update(aCardCodeSold);

                }
            }

            _logger.Info("About to Update Card Transaction;CardCodes.. at ExecutionPoint:CardSaleUpdate");
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Updated Card Transaction;CardCodes.. at ExecutionPoint:CardSaleUpdate");
        }
        #endregion

        /// <summary>
        /// UPDATE CARD TRANSACTION STATUS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateCardTransactionStatus(Guid transactionId, UpdateCardTransactionStatusDTO model, Guid UserId)
        {
            //VALIDATES THE CARD TRANSACTION ID
            var cardTransaction = await FindCardTransaction(transactionId);

            if (cardTransaction is null)
            {
                Errors.Add(ResponseMessage.CardTransactionNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTransactionNotFound, Errors);
            }

            //CHECK IF THE TRANSACTION HAS NOT BEEN ACTED ON BY THE ADMIN
            if (cardTransaction.TransactionStatus != TransactionStatus.Pending)
            {
                var message = $"Card Transaction has already been actioned by Optima Admin with Name:" +
                    $" {cardTransaction.ActionBy.FullName} on {cardTransaction.ActionedByDateTime.Value:ddd, dd MMMM yyyy, hh:mm tt}";
                Errors.Add(message);
                return new BaseResponse<bool>(message, Errors);
            }

            switch (model.TransactionStatus)
            {
                   
                case TransactionStatus.Declined:
                    {
                        cardTransaction.TransactionStatus = TransactionStatus.Declined;
                        _context.CardTransactions.Update(cardTransaction);
                        //SEND PUSH NOTIFICATION
                        var data = SendPushNotification(new List<Guid> { cardTransaction.ApplicationUserId }, TransactionStatus.Declined.GetDescription());
                        await _pushNotificationService.SendPushNotification(data);
                        //SAVE NOTIFICATION TO DB
                        await SaveNotificationForUser(new List<Guid> { cardTransaction.ApplicationUserId }, TransactionStatus.Declined.GetDescription());
                        break;
                    }
                  
                case TransactionStatus.PartialApproval:
                    {
                        var cardTransactonUpdate = UpdateUserTransaction(model, cardTransaction, UserId);
                        _context.CardTransactions.Update(cardTransactonUpdate);
                        var creditDebit = await CreateCreditDebit(model, cardTransaction, UserId);
                        await _context.CreditDebit.AddAsync(creditDebit);
                        //SEND PUSH NOTIFICATION
                        var data = SendPushNotification(new List<Guid> { cardTransaction.ApplicationUserId }, TransactionStatus.PartialApproval.GetDescription());
                        await _pushNotificationService.SendPushNotification(data);
                        //SAVE NOTIFICATION TO DB
                        await SaveNotificationForUser(new List<Guid> { cardTransaction.ApplicationUserId }, TransactionStatus.PartialApproval.GetDescription());
                        break;
                    }
                  
                case TransactionStatus.Approved:
                    {
                        var cardTransactonUpdate = UpdateUserTransaction(model, cardTransaction, UserId);
                        _context.CardTransactions.Update(cardTransactonUpdate);
                        var creditDebit = await CreateCreditDebit(model, cardTransaction, UserId);
                        await _context.CreditDebit.AddAsync(creditDebit);
                        //SEND PUSH NOTIFICATION
                        var data = SendPushNotification(new List<Guid> { cardTransaction.ApplicationUserId }, TransactionStatus.Approved.GetDescription());
                        await _pushNotificationService.SendPushNotification(data);
                        //SAVE NOTIFICATION TO DB
                        await SaveNotificationForUser(new List<Guid> { cardTransaction.ApplicationUserId }, TransactionStatus.Approved.GetDescription());
                        break;
                    }
                default:
                    break;
            }


            _logger.Info("About to Update Card Transaction, Debit Credit, UserWallet.. at ExecutionPoint:UpdateCardTransactionStatus");
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Updated Card Transaction, Debit Credit, UserWallet.. at ExecutionPoint:UpdateCardTransactionStatus");

            return new BaseResponse<bool>(true, ResponseMessage.CardTransactionUpdate);
        }

        /// <summary>
        /// FIND CARD TYPE DENOMINATIONS
        /// </summary>
        /// <param name="ids">The Ids.</param>
        /// <returns>Task&lt;List&lt;CardTypeDenomination&gt;&gt;.</returns>
        private async Task<List<CardTypeDenomination>> FindCardTypeDenominations(List<Guid> ids) =>
            await _context.CardTypeDenomination.Where(x => ids.Contains(x.Id)).ToListAsync();

        /// <summary>
        /// ENTITY FILTER
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="query">The query.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private IQueryable<CardTransaction> EntityFilter(BaseSearchViewModel model, IQueryable<CardTransaction> query)
        {
            if (!string.IsNullOrEmpty(model.Keyword) && !string.IsNullOrEmpty(model.Filter))
            {
                var key = model.Filter.ToLower();
                switch (key)
                {
                    case "transactionstatus":
                        {
                            query = query.Where(x => x.TransactionStatus == model.Keyword.Parse<TransactionStatus>());
                            break;
                        }
                    case "transactionid":
                        {
                            query = query.Where(x => x.TransactionRef.Contains(model.Keyword));
                            break;
                        }
                    case "customername":
                        {
                            query = query.Where(x => x.ApplicationUser.FullName.Contains(model.Keyword));
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return query;
        }

        /// <summary>
        /// CALCULATE AMOUNT
        /// </summary>
        /// <param name="cardTypeDenomination">The cardTypeDenomination.</param>
        /// <param name="cardCodes">The cardCodes.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private decimal CalculateAmount(CardTypeDenomination cardTypeDenomination, List<string> cardCodes)
        {
            var amount = cardTypeDenomination.Rate * cardCodes.Where(x => x != null).Count();
            return amount;
        }

        /// <summary>
        /// FIND CARD TRANSACTION
        /// </summary>
        /// <param name="Id">The Id.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private async Task<CardTransaction> FindCardTransaction(Guid Id) =>
            await _context.CardTransactions
            .Where(x => x.Id == Id)
            .Include(x => x.CardSold)
            .Include(x => x.ActionBy)
            .FirstOrDefaultAsync();

        /// <summary>
        /// FIND CARD TRANSACTION
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="cardTransaction">the cardTransaction</param>
        /// <param name="UserId">the UserId</param>
        /// <returns>CardTransaction</returns>
        private CardTransaction UpdateUserTransaction(UpdateCardTransactionStatusDTO model, CardTransaction cardTransaction, Guid UserId)
        {
            //UPDATES THE USER CARD TRANSACTION
            cardTransaction.TransactionStatus = model.TransactionStatus;
            cardTransaction.ActionById = UserId;
            cardTransaction.ActionedByDateTime = DateTime.UtcNow;
            cardTransaction.AmountPaid = model.Amount;

            return cardTransaction;
        }

        /// <summary>
        /// GENERATE DELETE UPLOADED PATH
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>BaseResponse&lt;bool&gt;.</returns>
        private string GenerateDeleteUploadedPath(string value)
        {
            //Split the Image Url From Cloudinary
            var splittedLogoUrl = value.Split("/");

            //get the cloudinary PublicId
            var LogoPublicId = splittedLogoUrl[8];
            var splittedLogoPublicId = LogoPublicId.Split(".");

            //Get the Full Asset Path
            var fullPath = $"Optima/{splittedLogoPublicId[0]}";

            return fullPath;
        }

        /// <summary>
        /// CREATE CREDIT-DEBIT TRANSACTION
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="cardTransaction">the cardTransaction</param>
        /// <param name="UserId">the UserId</param>
        /// <returns>Task&lt;CardTransaction&gt;</returns>
        private async Task<CreditDebit> CreateCreditDebit(UpdateCardTransactionStatusDTO model, CardTransaction cardTransaction, Guid UserId)
        {
            //GET THE USER WALLET
            var userWallet = await _context.WalletBalance.Where(x => x.UserId == cardTransaction.ApplicationUserId).FirstOrDefaultAsync();

            var creditDebit = new CreditDebit();

            if (!(userWallet is null))
            {
                //UPDATE THE USER WALLET BALANCE
                userWallet.Balance += model.Amount;
                userWallet.ModifiedOn = DateTime.UtcNow;
                userWallet.ModifiedBy = UserId;

                //CREATE A CREDIT DEBIT FOR THE USER              
                creditDebit.Amount = model.Amount;
                creditDebit.TransactionStatus = model.TransactionStatus;
                creditDebit.TransactionType = TransactionType.Credit;
                creditDebit.WalletBalanceId = userWallet.Id;
                creditDebit.ActionedByUserId = UserId;
                creditDebit.TransactionReference = GenerateCreditDebitTransactionRef(TransactionType.Credit).Result;

            }

            return creditDebit;

        }

        /// <summary>
        /// SAVE USER NOTIFICATION
        /// </summary>
        /// <param name="userIds">the userIds</param
        /// <param name="name">the name</param>
        /// <returns></returns>
        private async Task SaveNotificationForAdmin(string name)
        {        
            var data = new CreateAdminNotificationDTO
            {
                Message = $"{name} has made a Card Sale",
                Type = NotificationType.Card_Sale,  
            };
            await _notificationService.CreateNotificationForAdmin(data);            
        }

        /// <summary>
        /// GET ADMIN USERS
        /// </summary>
        /// <returns>List&lt;Guid&gt;</returns>
        private List<Guid> AdminUsers() =>
            _userManager.Users.Where(x => x.UserType != UserTypes.USER).ToListAsync().Result.Select(x => x.Id).ToList();

        /// <summary>
        /// CREATE CARD SALE NOTIFICATION
        /// </summary>
        /// <param name="name">the name</param>
        /// <param name="message">the message</param>
        /// <returns>CardSaleNotificationDTO</returns>
        private CardSaleNotificationDTO CreateCardSaleNotificationDTO(string name, string message)
        {
            return new CardSaleNotificationDTO
            {
                Name = $"{name} has made a Card Sale",
                Message = message
            };
        }

        /// <summary>
        /// GET USER BY ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Task&lt;ApplicationUser&gt;</returns>
        private async Task<ApplicationUser> GetUserById(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Sends the push notification.
        /// </summary>
        /// <param name="userIds">The user ids.</param>
        /// <param name="transactionStatus">Name of the company.</param>
        /// <returns>SendPushNotificationDTO.</returns>
        private SendPushNotificationDTO SendPushNotification(List<Guid> userIds, string transactionStatus)
        {
            return new SendPushNotificationDTO
            {
                Title = "Card Transaction",
                Message = $"Optima Admin has {transactionStatus} your Card Sale Transaction",
                UserIds = userIds
            };
        }

        /// <summary>
        /// SAVE USER NOTIFICATION
        /// </summary>
        /// <param name="userIds">the userIds</param
        /// <param name="transactionStatus">the transactionStatus</param>
        /// <returns></returns>
        private async Task SaveNotificationForUser(List<Guid> userIds, string transactionStatus)
        {
            foreach (var userId in userIds)
            {
                var data = new CreateNotificationDTO
                {
                    Type = GetNotificationStatus(transactionStatus),
                    Message = $"Optima Admin has {transactionStatus} your Card Sale Transaction",
                };

                await _notificationService.CreateNotificationForUser(data, userId);
            };
        }

        /// <summary>
        /// GET NOTIFICATION STATUS
        /// </summary>
        /// <param name="notificationType">the notificationType</param>
        /// <returns></returns>
        private NotificationType GetNotificationStatus(string notificationType)
        {
            switch (notificationType)
            {
                case "Approved":
                    return NotificationType.Approved_Transaction;
                
                case "Partial Approval":
                    return NotificationType.Partial_Approved_Transaction; 

                case "Declined":
                    return NotificationType.Declined_Transaction;
                  
                default:
                    return NotificationType.Nil;
            }

        }

        /// <summary>
        /// GENERATES THE CREDIT DEBIT TRANSACTION REF ID
        /// </summary>
        /// <param name="transactionType">the transaction type</param>
        /// <returns></returns>
        private async Task<string> GenerateCreditDebitTransactionRef(TransactionType transactionType)
        {
            var lastCreditDebit = await _context.CreditDebit.LastOrDefaultAsync(x => x.TransactionType == transactionType);

            return GenerateCreditDebitTransactionRef(lastCreditDebit, transactionType);
        }

        public async Task<BaseResponse<PagedList<AllTransactionDTO>>> GetPendingTransaction(BaseSearchViewModel model)
        {
            var status = new List<TransactionStatus>();
            status.Add(TransactionStatus.Pending);

            return await GetTransactionByStatus(model, status);
        }
        public async Task<BaseResponse<PagedList<AllTransactionDTO>>> GetDeclinedTransaction(BaseSearchViewModel model)
        {
            var status = new List<TransactionStatus>();
            status.Add(TransactionStatus.Declined);

            return await GetTransactionByStatus(model, status);
        }

        public async Task<BaseResponse<PagedList<AllTransactionDTO>>> GetApproved_PartialApproved_Transaction(BaseSearchViewModel model)
        {
            var status = new List<TransactionStatus>();
            status.Add(TransactionStatus.Approved);
            status.Add(TransactionStatus.PartialApproval);

            return await GetTransactionByStatus(model, status);
        }

        public async Task<BaseResponse<PagedList<AllTransactionDTO>>> GetTransactionByStatus(BaseSearchViewModel model, List<TransactionStatus> status)
        {
            var query = _context.CardTransactions
                .Include(x => x.ApplicationUser)
                .Include(x => x.CardTypeDenomination.CardType.Card)
                .Include(x => x.CardSold)
                    .ThenInclude(x => x.Denomination)
                        //.ThenInclude(x => x.CardType)
                            //.ThenInclude(x => x.Card)
                            .Where(x => status.Contains(x.TransactionStatus))
                            .AsQueryable();

            var cardTransactions = await EntityFilter(model, query).OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);

            var filteredData = BuildTransactionModel(cardTransactions);

            var data = new PagedList<AllTransactionDTO>(filteredData, model.PageIndex, model.PageSize, cardTransactions.TotalItemCount);
            return new BaseResponse<PagedList<AllTransactionDTO>>
            { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {data.Count} CARD TRANSACTION(s)." };
        }

        private IQueryable<AllTransactionDTO> BuildTransactionModel(IPagedList<CardTransaction> model)
        {
            return model.Select(x => new AllTransactionDTO
            {
                //CardName = x.CardSold.Select(x => x.CardTypeDenomination.CardType.Card.Name).FirstOrDefault(),
                CardName = x.CardTypeDenomination.CardType.Card.Name,
                CreatedOn = x.CreatedOn,
                Id = x.Id,
                Status = x.TransactionStatus,
                TotalAmount = x.TotalExpectedAmount,
                TransactionRefId = x.TransactionRef,
                UserName = x.ApplicationUser.FullName
            }).AsQueryable();
        }

        public async Task<BaseResponse<List<AllTransactionDTO>>> GetUserRecentTransactions(Guid UserId)
        {
            var query = SeeAllTransactions(UserId).Result.Data.Take(20).ToList();

            return new BaseResponse<List<AllTransactionDTO>>(query, ResponseMessage.SuccessMessage000);
        }

        public async Task<BaseResponse<List<AllTransactionDTO>>> SeeAllTransactions(Guid UserId)
        {
            var query = await _context.CardTransactions
                .Include(x => x.ApplicationUser)
                .Include(x => x.CardTypeDenomination.CardType.Card)
                .Include(x => x.CardSold)
                    .ThenInclude(x => x.Denomination)
                        //.ThenInclude(x => x.CardType)
                            //.ThenInclude(x => x.Card)
                            .Where(x => x.ApplicationUserId == UserId)
                            .OrderBy(x => x.CreatedOn).AsNoTracking()
                            .Select(x => new AllTransactionDTO
                            {
                                //CardName = x.CardSold.Select(x => x.CardTypeDenomination.CardType.Card.Name).FirstOrDefault(),
                                Id = x.Id,
                                CardName = x.CardTypeDenomination.CardType.Card.Name,
                                CreatedOn = x.CreatedOn,                               
                                Status = x.TransactionStatus,
                                TotalAmount = x.TotalExpectedAmount,
                                TransactionRefId = x.TransactionRef,
                                UserName = x.ApplicationUser.FullName
                            }).ToListAsync();


            return new BaseResponse<List<AllTransactionDTO>>(query, ResponseMessage.SuccessMessage000);
        }
    }
}
