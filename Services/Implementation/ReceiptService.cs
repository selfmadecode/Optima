using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.DTO.ReceiptDTOs;
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
    public class ReceiptService : IReceiptService
    {
        private readonly ApplicationDbContext _context;
        public ReceiptService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// CREATE RECEIPT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateReceipt(CreateReceiptDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            var checkReceipt = await _context.Receipts.FirstOrDefaultAsync(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

            if (!(checkReceipt is null))
            {
                response.Data = false;
                response.ResponseMessage = "Receipt Type already Exists";
                response.Errors = new List<string> { "Receipt Type already Exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            var newReceipt = new Receipt
            {
                Name = model.Name,
                CreatedBy = UserId
            };

            await _context.AddAsync(newReceipt);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Receipt Created Successfully";
            return response;
            
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
                response.Data = false;
                response.ResponseMessage = "Receipt Type doesn't Exists";
                response.Errors = new List<string> { "Receipt Type doesn't Exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            var _ = await _context.CardTypeDenomination.AnyAsync(x => x.ReceiptId == id);

            if (_)
            {
                response.Data = false;
                response.ResponseMessage = "You cannot delete the receipt";
                response.Errors = new List<string> { "You cannot delete the receipt" };
                response.Status = RequestExecution.Failed;
                return response;
            }

            _context.Receipts.Remove(checkReceipt);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Receipt Type deleted Successfully";
            return response;
        }

        /// <summary>
        /// GET ALL RECEIPT
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;ReceiptDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<ReceiptDTO>>> GetAllReceipt()
        {
            var receipts = await _context.Receipts.OrderBy(x => x.Name).ToListAsync();

            var receiptsDTO = receipts.Select(x => (ReceiptDTO)x).ToList();

            return new BaseResponse<List<ReceiptDTO>> { Data = receiptsDTO, TotalCount = receiptsDTO.Count, ResponseMessage = $"Found {receiptsDTO.Count} Receipt Type(s)." };
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
                response.Data = false;
                response.ResponseMessage = "Receipt Type doesn't Exists";
                response.Errors = new List<string> { "Receipt Type doesn't Exists" };
                response.Status = RequestExecution.Failed;
                return response;
            }
            if (model.Name.Replace(" ", "").ToLower() != checkReceipt.Name.Replace(" ", "").ToLower())
            {
                var checkExistingReceipts = await _context.Receipts.AnyAsync(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

                if (checkExistingReceipts)
                {
                    response.Data = false;
                    response.ResponseMessage = "Receipt Type already Exists.";
                    response.Errors.Add("Receipt Type already Exists.");
                    response.Status = RequestExecution.Failed;
                    return response;
                }
            }


            checkReceipt.Name = string.IsNullOrWhiteSpace(model.Name) ? checkReceipt.Name : model.Name;

            _context.Receipts.Update(checkReceipt);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Receipt Type Updated Successfully";
            return response;

        }
    }
}
