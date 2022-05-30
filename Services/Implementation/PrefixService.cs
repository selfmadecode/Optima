
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.DTO.PrefixDTOs;
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
    public class PrefixService : IPrefixService
    {
        private readonly ApplicationDbContext _context;

        public PrefixService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// CREATE PREFIX
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreatePrefix(CreatePrefixDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            var checkPrefix = await _context.VisaPrefixes.FirstOrDefaultAsync(x => x.PrefixNumber.ToLower().Replace(" ", "") == model.PrefixNumber.ToLower().Replace(" ", ""));

            if (!(checkPrefix is null))
            {
                response.Data = false;
                response.ResponseMessage = "Visa Prefix already Exists";
                response.Errors = new List<string> { "Visa Prefix already Exists" };
                response.Status = RequestExecution.Error;
                return response;
            }

            var newReceipt = new Prefix
            {
                PrefixNumber = model.PrefixNumber,
                CreatedBy = UserId
            };

            await _context.VisaPrefixes.AddAsync(newReceipt);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Visa Prefix Created Successfully";
            return response;
        }

        /// <summary>
        /// DELETE PREFIX
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeletePrefix(Guid id)
        {
            var response = new BaseResponse<bool>();

            var checkPrefix = await _context.VisaPrefixes.FirstOrDefaultAsync(x => x.Id == id);

            if (checkPrefix is null)
            {
                response.Data = false;
                response.ResponseMessage = "Visa Prefix doesn't Exists";
                response.Errors = new List<string> { "Visa Prefix doesn't Exists" };
                response.Status = RequestExecution.Error;
                return response;
            }

            var _ = await _context.CardTypeDenomination.AnyAsync(x => x.PrefixId == id);

            if (_)
            {
                response.Data = false;
                response.ResponseMessage = "You cannot delete the Visa Prefix";
                response.Errors = new List<string> { "You cannot delete the Visa Prefix" };
                response.Status = RequestExecution.Error;
                return response;
            }

            _context.VisaPrefixes.Remove(checkPrefix);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Visa Prefix deleted Successfully";
            return response;
        }

        /// <summary>
        /// GET ALL PREFIX
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;PrefixDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<PrefixDTO>>> GetAllPrefix()
        {
            var visaPrefixs = await _context.VisaPrefixes.OrderBy(x => x.PrefixNumber).ToListAsync();

            var visaPrefixsDTO = visaPrefixs.Select(x => (PrefixDTO)x).ToList();

            return new BaseResponse<List<PrefixDTO>> { Data = visaPrefixsDTO, ResponseMessage = $"Found {visaPrefixsDTO.Count} Receipt Type(s)." };
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
                response.Data = false;
                response.ResponseMessage = "Visa Prefix doesn't Exists";
                response.Errors = new List<string> { "Visa Prefix doesn't Exists" };
                response.Status = RequestExecution.Error;
                return response;
            }
            else if (_context.VisaPrefixes.Any(x => x.PrefixNumber.ToLower().Replace(" ", "") == model.PrefixNumber.ToLower().Replace(" ", "")))
            {
                response.Data = false;
                response.ResponseMessage = "Visa Prefix already Exists";
                response.Errors = new List<string> { "Visa Prefix already Exists" };
                response.Status = RequestExecution.Error;
                return response;
            }

            checkPrefix.PrefixNumber = string.IsNullOrWhiteSpace(model.PrefixNumber) ? checkPrefix.PrefixNumber : model.PrefixNumber;

            _context.VisaPrefixes.Update(checkPrefix);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Receipt Type Updated Successfully";
            return response;
        }
    }
}
