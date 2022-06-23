using log4net;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.ReceiptDTOs;
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
    public class ReceiptService : BaseService, IReceiptService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILog _logger;

        public ReceiptService(ApplicationDbContext context)
        {
            _context = context;
            _logger = LogManager.GetLogger(typeof(ReceiptService));
        }

        /// <summary>
        /// CREATE RECEIPT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateReceipt(CreateReceiptDTO model, Guid UserId)
        {

            var checkReceipt = await _context.Receipts.FirstOrDefaultAsync(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

            if (!(checkReceipt is null))
            {
                Errors.Add(ResponseMessage.ReceiptExist);
                return new BaseResponse<bool>(ResponseMessage.ReceiptExist, Errors);
            }

            var newReceipt = new Receipt
            {
                Name = model.Name,
                CreatedBy = UserId
            };

            await _context.AddAsync(newReceipt);
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Created a Receipt Type... at ExecutionPoint:CreateReceipt");

            return new BaseResponse<bool>(true, ResponseMessage.ReceiptCreated);
        }

        /// <summary>
        /// DELETE RECEIPT
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteReceipt(Guid id)
        {
            var response = new BaseResponse<bool>();

            var checkReceipt = await _context.Receipts.FirstOrDefaultAsync(x => x.Id == id);

            if (checkReceipt is null)
            {
                Errors.Add(ResponseMessage.ReceiptNotFound);
                return new BaseResponse<bool>(ResponseMessage.ReceiptNotFound, Errors);
            }

            var _ = await _context.CardTypeDenomination.AnyAsync(x => x.ReceiptId == id);

            if (_)
            {
                Errors.Add(ResponseMessage.CannotDeleteReceipt);
                return new BaseResponse<bool>(ResponseMessage.CannotDeleteReceipt, Errors);
            }

            _context.Receipts.Remove(checkReceipt);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, ResponseMessage.ReceiptDeleted);
        }

        /// <summary>
        /// GET ALL RECEIPT
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;ReceiptDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<ReceiptDTO>>> GetAllReceipt()
        {
            var receipts = await _context.Receipts.OrderBy(x => x.Name).ToListAsync();

            var receiptsDTO = receipts.Select(x => (ReceiptDTO)x).ToList();

            return new BaseResponse<List<ReceiptDTO>> 
            { Data = receiptsDTO, TotalCount = receiptsDTO.Count, ResponseMessage = $"FOUND {receiptsDTO.Count} RECEIPT TYPE(s)." };
        }


        /// <summary>
        /// UPDATE RECEIPT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateReceipt(UpdateReceiptDTO model)
        {
            var response = new BaseResponse<bool>();

            var checkReceipt = await _context.Receipts.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (checkReceipt is null)
            {
                Errors.Add(ResponseMessage.ReceiptNotFound);
                return new BaseResponse<bool>(ResponseMessage.ReceiptNotFound, Errors);
            }

            if (model.Name.Replace(" ", "").ToLower() != checkReceipt.Name.Replace(" ", "").ToLower())
            {
                var checkExistingReceipts = await _context.Receipts.AnyAsync(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

                if (checkExistingReceipts)
                {
                    Errors.Add(ResponseMessage.ReceiptExist);
                    return new BaseResponse<bool>(ResponseMessage.ReceiptExist, Errors);
                }
            }


            checkReceipt.Name = string.IsNullOrWhiteSpace(model.Name) ? checkReceipt.Name : model.Name;

            _context.Receipts.Update(checkReceipt);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, ResponseMessage.ReceiptUpdated);
        }
    }
}
