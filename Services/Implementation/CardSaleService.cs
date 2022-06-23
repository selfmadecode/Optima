using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.CardTransactionDTOs;
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
        private readonly ILog _logger;
        private readonly ICloudinaryServices _cloudinaryServices;


        public CardSaleService(ApplicationDbContext context, ICloudinaryServices cloudinaryServices)
        {
            _context = context;
            _cloudinaryServices = cloudinaryServices;
            _logger = LogManager.GetLogger(typeof(CardSaleService));
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
                var checkCardTpeDenominations = await FindCardTypeDenominations(model.CardTypeDTOs.Select(x => x.CardTypeDenominationId).ToList());

                if (checkCardTpeDenominations.Count != model.CardTypeDTOs.Select(x => x.CardTypeDenominationId).Count())
                {
                    Errors.Add(ResponseMessage.CardTypeDenominationNotFound);
                    return new BaseResponse<bool>(ResponseMessage.CardTypeDenominationNotFound, Errors);
                }

                var cardTransactionImages = await UploadTransactionImages(model, UserId);
                filesToDelete = cardTransactionImages.Select(x => x.LogoUrl).ToList();
                var (cardTransaction, cardSold) = await CreateCardForSales(model, UserId); 

                cardTransaction.CardSold.AddRange(cardSold);
                cardTransaction.TransactionUploadededFiles.AddRange(cardTransactionImages);
             
                await _context.CardTransactions.AddAsync(cardTransaction);

                _logger.Info("About to Save Card Transaction, Card Sold and Card Transaction Uploaded Files... at ExecutionPoint:CreateCardSales");
                await _context.SaveChangesAsync();
                _logger.Info("Successfully Saved Card Transaction, Card Sold and Card Transaction Uploaded Files... at ExecutionPoint:CreateCardSales");

                return new BaseResponse<bool>(true, ResponseMessage.CardSaleCreation);
            }
            catch (Exception ex)
            {
                foreach (var filePath in filesToDelete)
                {
                    var fullPath = GenerateDeleteUploadedPath(filePath);
                    await _cloudinaryServices.DeleteImage(fullPath);
                }

                _logger.Error(ex.Message, ex);

                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage999, Errors);
            }
           
        }

        /// <summary>
        /// CREATES CARD FOR SALE
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="UserId">The UserId</param>
        /// <returns>
        /// System.ValueTuple&lt;(CardTransaction, List&lt;CardSold)&gt;.
        /// </returns>
        private async Task<(CardTransaction, List<CardSold>)> CreateCardForSales(SellCardDTO model, Guid UserId)
        {
            decimal amount = 0;

            var cardTypeDenominations = await _context.CardTypeDenomination
                .Where(x => model.CardTypeDTOs.Select(x => x.CardTypeDenominationId).Contains(x.Id))
                .ToListAsync();

            foreach (var sellCardDto in model.CardTypeDTOs)
            {
                var aCardTypeDenomination = cardTypeDenominations.FirstOrDefault(x => x.Id == sellCardDto.CardTypeDenominationId);
                amount += CalculateAmount(aCardTypeDenomination, sellCardDto.CardCodes);
            }        

            var cardTransaction = new CardTransaction
            {
                TransactionStatus = TransactionStatus.Pending,
                TotalExpectedAmount = amount,
                ApplicationUserId = UserId,
                CreatedBy = UserId,
            };

            var cardSold = new List<CardSold>();
            var cardCodes = new List<CardCodes>();

            foreach (var sellCardDto in model.CardTypeDTOs)
            {

                var newCardSold = new CardSold
                {
                    CardTypeDenominationId = sellCardDto.CardTypeDenominationId,
                    Amount = CalculateAmount(cardTypeDenominations.FirstOrDefault(x => x.Id == sellCardDto.CardTypeDenominationId), sellCardDto.CardCodes),
                    CreatedBy = UserId,
                };

                foreach (var cardCode in sellCardDto.CardCodes)
                {

                    newCardSold.CardCodes.Add(new CardCodes
                    {
                        CardSoldId = newCardSold.Id,
                        CardCode = cardCode,
                        CreatedBy = UserId,
                    });
                }

                cardSold.Add(newCardSold);
            }

            return (cardTransaction, cardSold);
        }

        /// <summary>
        /// UPLOAD TRANSACTION IMAGES
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;List&lt;TransactionUploadFiles&gt;&gt;.</returns>
        private async Task<List<TransactionUploadFiles>> UploadTransactionImages(SellCardDTO model, Guid UserId)
        {

            var transactionUploadedFiles = new List<TransactionUploadFiles>();

            _logger.Info($"Uploading {model.CardTypeDTOs.SelectMany(x => x.CardImages).Count()} to Cloudinary... at ExecutionPoint:UploadTransactionImages");
            foreach (var file in model.CardTypeDTOs.SelectMany(x => x.CardImages).Select(cardImage => cardImage))
            {
                var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(file);
                transactionUploadedFiles.Add(new TransactionUploadFiles
                {
                    LogoUrl = uploadedFile,
                    CreatedBy = UserId,
                });

            }

            _logger.Info($"Successfully Uploaded {model.CardTypeDTOs.SelectMany(x => x.CardImages).Count()} to Cloudinary... at ExecutionPoint:UploadTransactionImages");
            return transactionUploadedFiles;
        }

        /// <summary>
        /// GET ALL CARD SALES
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;GetAllCardSales&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardTransactionDTO>>> GetAllCardSales(BaseSearchViewModel model)
        {
            var query = _context.CardTransactions
                .Include(x => x.CardSold).ThenInclude(x => x.CardCodes)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .Include(x => x.TransactionUploadededFiles)
                .Include(x => x.ApplicationUser)
                .Include(x => x.ActionBy)
                .AsNoTracking()
                .AsQueryable();

            var cardTransactions = await EntityFilter(model, query).OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);
            var cardTransactionsDto = cardTransactions.Select(x => (CardTransactionDTO)x).ToList();

            var data = new PagedList<CardTransactionDTO>(cardTransactionsDto, model.PageIndex, model.PageSize, cardTransactions.TotalItemCount);
            return new BaseResponse<PagedList<CardTransactionDTO>> 
            { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {data.Count} CARDTRANSACTION(s)." };
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
                .Include(x => x.CardSold).ThenInclude(x => x.CardCodes)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
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
            var cardTransactionsDto = cardTransactions.Select(x => (CardTransactionDTO)x).ToList();

            var data = new PagedList<CardTransactionDTO>(cardTransactionsDto, model.PageIndex, model.PageSize, cardTransactions.TotalItemCount);
            return new BaseResponse<PagedList<CardTransactionDTO>> 
            { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"FOUND {data.Count} CARD TRANSACTION(s)." };
        }

        /// <summary>
        /// UPDATE CARD FOR SALES
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateCardSales(UpdateSellCardDTO model, Guid UserId)
        {

            var cardTransaction = await FindCardTransaction(model.TransactionId);
                
            if (cardTransaction is null)
            {
                Errors.Add(ResponseMessage.CardTransactionNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTransactionNotFound, Errors);
            }

            //check card sold
            var cardSold = cardTransaction.CardSold.Where(x => model.UpdateCardSoldDTOs.Select(x => x.CardSoldId).Contains(x.Id)).ToList();

            if (cardSold.Count != model.UpdateCardSoldDTOs.Select(x => x.CardSoldId).Count())
            {
                Errors.Add(ResponseMessage.CardSoldNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardSoldNotFound, Errors);
            }

            //Check Card Codes
            var cardCodes = cardTransaction.CardSold.SelectMany(x => x.CardCodes)
                .Where(x => model.UpdateCardSoldDTOs.SelectMany(x => x.UpdateCardCodeDTOs.Select(x => x.CardCodeId)).Contains(x.Id)).ToList();

            if (cardCodes.Count != model.UpdateCardSoldDTOs.SelectMany(x => x.UpdateCardCodeDTOs.Select(x => x.CardCodeId)).Count())
            {
                Errors.Add(ResponseMessage.CardCodesNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardCodesNotFound, Errors);
            }

            var cardCodeIds = model.UpdateCardSoldDTOs.SelectMany(x => x.UpdateCardCodeDTOs).Select(x => x.CardCodeId);
            var cardSoldCodes = _context.CardCodes.Where(x => cardCodeIds.Contains(x.Id)).ToList();

            await CardSaleUpdate(model, cardCodes, UserId);

            return new BaseResponse<bool>(true, ResponseMessage.CardCodeUpdate);
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
            foreach (var UpdateCardSoldDTO in model.UpdateCardSoldDTOs)
            {
                foreach (var cardCodeDto in UpdateCardSoldDTO.UpdateCardCodeDTOs)
                {
                    var aCardCodeSold = cardCodes.FirstOrDefault(x => x.Id == cardCodeDto.CardCodeId);

                    aCardCodeSold.CardCode = string.IsNullOrWhiteSpace(cardCodeDto.CardCode) ? aCardCodeSold.CardCode : cardCodeDto.CardCode;
                    aCardCodeSold.ModifiedBy = UserId;
                    aCardCodeSold.ModifiedOn = DateTime.UtcNow;

                    _context.CardCodes.Update(aCardCodeSold);

                }
            }

            _logger.Info("About to Update Card Transaction;CardCodes.. at ExecutionPoint:CardSaleUpdate");
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Updated Card Transaction;CardCodes.. at ExecutionPoint:CardSaleUpdate");
        }

        /// <summary>
        /// UPDATE CARD TRANSACTION STATUS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateCardTransactionStatus(UpdateCardTransactionStatusDTO model, Guid UserId)
        {

            var cardTransaction = await FindCardTransaction(model.TransactionId);

            if (cardTransaction is null)
            {
                Errors.Add(ResponseMessage.CardTransactionNotFound);
                return new BaseResponse<bool>(ResponseMessage.CardTransactionNotFound, Errors);
            }

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
                        break;
                    }
                  
                case TransactionStatus.PartialApproval:
                    {
                        var cardTransactonUpdate = UpdateUserTransaction(model, cardTransaction, UserId);
                        _context.CardTransactions.Update(cardTransactonUpdate);
                        var creditDebit = await CreateCreditDebit(model, cardTransaction, UserId);
                        await _context.CreditDebit.AddAsync(creditDebit);
                        break;
                    }
                  
                case TransactionStatus.Approved:
                    {
                        var cardTransactonUpdate = UpdateUserTransaction(model, cardTransaction, UserId);
                        _context.CardTransactions.Update(cardTransactonUpdate);
                        var creditDebit = await CreateCreditDebit(model, cardTransaction, UserId);
                        await _context.CreditDebit.AddAsync(creditDebit);
                        break;
                    }
                default:
                    break;
            }


            _logger.Info("About to Update Card Transaction,Debit Credit,UserWallet.. at ExecutionPoint:UpdateCardTransactionStatus");
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Updated Card Transaction,Debit Credit,UserWallet.. at ExecutionPoint:UpdateCardTransactionStatus");

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

                switch (model.Filter)
                {
                    case "TransactionStatus":
                        {
                            query = query.Where(x => x.TransactionStatus == model.Keyword.Parse<TransactionStatus>());
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
            .Include(x => x.CardSold).ThenInclude(x => x.CardCodes)
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
            //get the user wallet and credit his account
            var userWallet = await _context.WalletBalance.Where(x => x.UserId == cardTransaction.ApplicationUserId).FirstOrDefaultAsync();

            var creditDebit = new CreditDebit();

            if (!(userWallet is null))
            {
                //Update User Wallet Balance
                userWallet.Balance += model.Amount;
                userWallet.ModifiedOn = DateTime.UtcNow;
                userWallet.ModifiedBy = UserId;

                //Create a Credit for the User               
                creditDebit.Amount = model.Amount;
                creditDebit.TransactionStatus = model.TransactionStatus;
                creditDebit.TransactionType = TransactionType.Credit;
                creditDebit.WalletBalanceId = userWallet.Id;
                creditDebit.ActionedByUserId = UserId;

            }

            return creditDebit;

        }
        
    }
}
