using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
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
    public class CardSaleService : ICardSaleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CardSaleService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        /// <summary>
        /// CREATE CARD FOR SALES
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateCardSales(SellCardDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();


            foreach (var file in model.CardTypeDTOs.SelectMany(x => x.CardImages).Select(cardImage => cardImage))
            {
                var result = ValidateFile(file); 

                if (result.Errors.Any())
                {
                    result.ResponseMessage = response.ResponseMessage;
                    result.Errors = response.Errors;
                    result.Status = RequestExecution.Failed;
                    return result;
                }
            }
           
            var checkCardTpeDenominations = await FindCardTypeDenominations(model.CardTypeDTOs.Select(x => x.CardTypeDenominationId).ToList());

            if (checkCardTpeDenominations.Count != model.CardTypeDTOs.Select(x => x.CardTypeDenominationId).Count())
            {
                response.Data = false;
                response.ResponseMessage = "CardType Denomination doesn't exists";
                response.Errors.Add("CardType Denomination doesn't exists");
                response.Status = RequestExecution.Failed;
                return response;
            }

            decimal amount = 0;

            var cardTypeDenominations = await _context.CardTypeDenomination
                .Where(x => model.CardTypeDTOs.Select(x => x.CardTypeDenominationId).Contains(x.Id))
                .ToListAsync();

            foreach (var sellCardDto in model.CardTypeDTOs)
            {
                var aCardTypeDenomination = cardTypeDenominations.FirstOrDefault(x => x.Id == sellCardDto.CardTypeDenominationId);
                amount += CalculateAmount(aCardTypeDenomination, sellCardDto.CardCodes);
            }

            //Upload to Cloudinary
            var transactionUploadedFiles = new List<TransactionUploadFiles>();

            foreach (var file in model.CardTypeDTOs.SelectMany(x => x.CardImages).Select(cardImage => cardImage))
            {
                var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(file, _configuration);
                transactionUploadedFiles.Add(new TransactionUploadFiles
                {
                    LogoUrl = uploadedFile,
                    CreatedBy = UserId,
                });
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

            cardTransaction.CardSold.AddRange(cardSold);
            cardTransaction.TransactionUploadededFiles.AddRange(transactionUploadedFiles);

            await _context.CardTransactions.AddAsync(cardTransaction);

            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Successfully Created your Gift Card For Sale";
            return response;
        }


        /// <summary>
        /// GET ALL CARD SALES
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;GetAllCardSales&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CardTransactionDTO>>> GetAllCardSales(BaseSearchViewModel model)
        {
            var query = _context.CardTransactions.AsNoTracking()
                .Include(x => x.CardSold).ThenInclude(x => x.CardCodes)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Denomination)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Prefix)
                .Include(x => x.CardSold).ThenInclude(x => x.CardTypeDenomination).ThenInclude(x => x.Receipt)
                .Include(x => x.TransactionUploadededFiles)
                .AsQueryable();

            var cardTransactions = await EntityFilter(model, query).OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);
            var cardTransactionsDto = cardTransactions.Select(x => (CardTransactionDTO)x).ToList();

            var data = new PagedList<CardTransactionDTO>(cardTransactionsDto, model.PageIndex, model.PageSize, cardTransactions.TotalItemCount);
            return new BaseResponse<PagedList<CardTransactionDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"Found {data.Count} Card Transaction(s)." };
        }


        /// <summary>
        /// FIND CARD TYPE DENOMINATIONS
        /// </summary>
        /// <param name="ids">The Ids.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private async Task<List<CardTypeDenomination>> FindCardTypeDenominations(List<Guid> ids) =>
            await _context.CardTypeDenomination.Where(x => ids.Contains(x.Id)).ToListAsync();


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
    }
}
