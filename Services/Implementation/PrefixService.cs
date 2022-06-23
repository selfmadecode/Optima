
using log4net;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.PrefixDTOs;
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
    public class PrefixService : BaseService, IPrefixService
    {
        /// <summary>
        ///  The Application DbContext
        /// </summary>
        private readonly ApplicationDbContext _context;
        private readonly ILog _logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixService" /> class.
        /// </summary>
        /// <param name="context">The Application DbContext</param>
        public PrefixService(ApplicationDbContext context)
        {
            _context = context;
            _logger = LogManager.GetLogger(typeof(PrefixService));
        }

        /// <summary>
        /// CREATE PREFIX
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreatePrefix(CreatePrefixDTO model, Guid UserId)
        {

            var checkPrefix = await _context.VisaPrefixes.
                FirstOrDefaultAsync(x => x.PrefixNumber.ToLower().Replace(" ", "") == model.PrefixNumber.ToLower().Replace(" ", ""));

            if (!(checkPrefix is null))
            {
                Errors.Add(ResponseMessage.VisaPrefixExist);
                return new BaseResponse<bool>(ResponseMessage.VisaPrefixExist, Errors);
            }

            var newReceipt = new Prefix
            {
                PrefixNumber = model.PrefixNumber,
                CreatedBy = UserId
            };

            await _context.VisaPrefixes.AddAsync(newReceipt);
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Created a Prefix... at ExecutionPoint:CreatePrefix");

            return new BaseResponse<bool>(true, ResponseMessage.VisaPrefixCreation);
        }

        /// <summary>
        /// DELETE PREFIX
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeletePrefix(Guid id)
        {

            var checkPrefix = await _context.VisaPrefixes.FirstOrDefaultAsync(x => x.Id == id);

            if (checkPrefix is null)
            {
                Errors.Add(ResponseMessage.VisaPrefixNotFound);
                return new BaseResponse<bool>(ResponseMessage.VisaPrefixNotFound, Errors);
            }

            var _ = await _context.CardTypeDenomination.AnyAsync(x => x.PrefixId == id);

            if (_)
            {
                Errors.Add(ResponseMessage.CannotDeleteVisaPrefix);
                return new BaseResponse<bool>(ResponseMessage.CannotDeleteVisaPrefix, Errors);
            }

            _context.VisaPrefixes.Remove(checkPrefix);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, ResponseMessage.DeleteVisaPrefix);
        }

        /// <summary>
        /// GET ALL PREFIX
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;PrefixDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<PrefixDTO>>> GetAllPrefix()
        {
            var visaPrefixs = await _context.VisaPrefixes.OrderBy(x => x.PrefixNumber).ToListAsync();

            var visaPrefixsDTO = visaPrefixs.Select(x => (PrefixDTO)x).ToList();

            return new BaseResponse<List<PrefixDTO>> 
            { Data = visaPrefixsDTO, TotalCount = visaPrefixsDTO.Count, ResponseMessage = $"FOUND {visaPrefixsDTO.Count} RECEIPT TYPE(s)." };
        }

        /// <summary>
        /// UPDATE PREFIX
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdatePrefix(UpdatePrefixDTO model)
        {
            var response = new BaseResponse<bool>();

            var checkPrefix = await _context.VisaPrefixes.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (checkPrefix is null)
            {
                Errors.Add(ResponseMessage.VisaPrefixNotFound);
                return new BaseResponse<bool>(ResponseMessage.VisaPrefixNotFound, Errors);
            }

            if (model.PrefixNumber.Replace(" ", "").ToLower() != checkPrefix.PrefixNumber.Replace(" ", "").ToLower())
            {
                var checkExistingReceipts = await _context.Receipts.AnyAsync(x => x.Name.ToLower().Replace(" ", "") == model.PrefixNumber.ToLower().Replace(" ", ""));

                if (checkExistingReceipts)
                {
                    Errors.Add(ResponseMessage.VisaPrefixExist);
                    return new BaseResponse<bool>(ResponseMessage.VisaPrefixExist, Errors);
                }
            }
            

            checkPrefix.PrefixNumber = string.IsNullOrWhiteSpace(model.PrefixNumber) ? checkPrefix.PrefixNumber : model.PrefixNumber;

            _context.VisaPrefixes.Update(checkPrefix);
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Updated Prefix");
            return new BaseResponse<bool>(true, ResponseMessage.DeleteVisaPrefix);
        }
    }
}
