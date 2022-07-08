using AzureRays.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.DenominationDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class DenominationService : BaseService, IDenominationService
    {
        private readonly ApplicationDbContext _context;

        public DenominationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// CREATE DENOMINATION
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateDenomination(CreateDenominationDTO model, Guid UserId)
        {
            var checkDenomination = await CheckDenomination(null, model.Amount);

            if (checkDenomination is true)
            {
                Errors.Add($"AMOUNT {model.Amount} ALREADY EXISTS.");
                return new BaseResponse<bool>($"AMOUNT {model.Amount} ALREADY EXISTS.", Errors);
            }

            var newDenomination = new Denomination
            {
                Amount = model.Amount,
                CreatedBy = UserId,
            };

            _context.Add(newDenomination);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, ResponseMessage.CreateDenomination);
        }

        /// <summary>
        /// DELETE DENOMINATION
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteDenomination(Guid id)
        {
            var checkDenomination = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == id);

            if (checkDenomination is null)
            {
                Errors.Add(ResponseMessage.DenominationNotExists);
                return new BaseResponse<bool>(ResponseMessage.DenominationNotExists, Errors);
            }

            var _ = await _context.CardTypeDenomination.AnyAsync(x => x.PrefixId == id);

            if (_)
            {
                Errors.Add(ResponseMessage.DenominationCannotBeDeleted);
                return new BaseResponse<bool>(ResponseMessage.DenominationCannotBeDeleted, Errors);
            }

            _context.Denominations.Remove(checkDenomination);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, ResponseMessage.DeleteDenomination);
        }

        /// <summary>
        /// GET ALL DENOMINATION NON-PAGINATED
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;RateDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<DenominationDTO>>> GetAllDenominations()
        {
            var denomination = await _context.Denominations.OrderBy(x => x.Amount).ToListAsync();

            var denominationDTO = denomination.Select(x => (DenominationDTO)x).ToList();

            return new BaseResponse<List<DenominationDTO>>(denominationDTO, denomination.Count, $"FOUND {denomination.Count} DENOMINATION(S).");
        }

        /// <summary>
        /// GET DENOMINATION    
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;RateDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<DenominationDTO>> GetDenomination(Guid id)
        {
            var checkDenomination = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == id);

            if (checkDenomination is null)
            {
                Errors.Add(ResponseMessage.DenominationNotExists);
                return new BaseResponse<DenominationDTO>(ResponseMessage.DenominationNotExists, Errors);
            }

            DenominationDTO rateDTO = checkDenomination;

            return new BaseResponse<DenominationDTO>(rateDTO, ResponseMessage.CreateDenomination);
        }

        /// <summary>
        /// UPDATE DENOMINATION    
        /// </summary>
        /// <param name="model">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateDenomination(UpdateDenominationDTO model, Guid UserId)
        {
            var checkDenomination = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (checkDenomination is null)
            {
                Errors.Add(ResponseMessage.DenominationNotExists);
                return new BaseResponse<bool>(ResponseMessage.DenominationNotExists, Errors);
            }

            var response = await CheckDenomination(checkDenomination.Id, model.Amount);
            if (response is false)
            {
                var denomination = new Denomination
                {
                    Amount = model.Amount,
                    CreatedBy = UserId
                };

                _context.Add(denomination);
            }
           
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, ResponseMessage.UpdateDenomination);
        }

        /// <summary>
        /// VALIDATES DENOMINATION
        /// </summary>
        /// <param name="id">The Id</param>
        /// <param name="amount">The Amount</param>
        /// <returns></returns>
        private async Task<bool> CheckDenomination(Guid? id, decimal? amount)
        {         
            if (amount.HasValue && id is null)
            {
                var response = await _context.Denominations.FirstOrDefaultAsync(x => x.Amount == amount);
                if (!(response is null)) return true; else return false;
            }
            else if (id.HasValue && amount.HasValue)
            {
                var response = await _context.Denominations.AnyAsync(x => x.Amount == amount);
                if (response) return true; else return false;            
            }

            return false;
        }
    }
}
