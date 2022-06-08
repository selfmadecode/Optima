using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
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

        public async Task<BaseResponse<bool>> CreateCardSales(SellCardDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();


            foreach (var file in model.CardImages)
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
           
            var checkCardTpeDenominations = await FindCardTypeDenominations(model.CardTypeDTO.Select(x => x.CardTypeDenominationId).ToList());

            if (checkCardTpeDenominations.Count != model.CardTypeDTO.Select(x => x.CardTypeDenominationId).Count())
            {
                response.Data = false;
                response.ResponseMessage = "CardType Denomination doesn't exists";
                response.Errors.Add("CardType Denomination doesn't exists");
                response.Status = RequestExecution.Failed;
                return response;
            }

            decimal amount = 0;
            foreach (var sellCardDto in model.CardTypeDTO)
            {
                var cardTypeDenominations = await _context.CardTypeDenomination.Where(x => model.CardTypeDTO.Select(x => x.CardTypeDenominationId).Contains(x.Id))
                    .Include(x => x.Denomination)
                    .ToListAsync();

                var aCardTypeDenomination = cardTypeDenominations.FirstOrDefault(x => x.Id == sellCardDto.CardTypeDenominationId);
                amount += aCardTypeDenomination.Denomination.Amount * sellCardDto.Quantity;
            }

            //Upload to Cloudinary
            var transactionUploadedFiles = new List<TransactionUploadFiles>();

            foreach (var file in model.CardImages)
            {
                var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(file, _configuration);
                transactionUploadedFiles.Add(new TransactionUploadFiles
                {
                    Url = responseMessage,
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

            var cardSell = new List<CardSold>();

            foreach (var sellCardDto in model.CardTypeDTO)
            {
                cardSell.Add(new CardSold
                {
                    CardTypeDenominationId = sellCardDto.CardTypeDenominationId,
                    Quantity = sellCardDto.Quantity,
                }) ;
            }

            cardTransaction.CardSold.AddRange(cardSell);
            cardTransaction.TransactionUploadededFiles.AddRange(transactionUploadedFiles);


            response.Data = true;
            response.ResponseMessage = "Successfully Created your Gift Card For Sale";
            return response;
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
    }
}
