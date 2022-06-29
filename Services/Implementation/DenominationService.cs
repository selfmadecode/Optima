using AzureRays.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
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
        /// CREATE RATE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateDenomination(CreateDenominationDTO model, Guid UserId)
        {
            var checkDenomination = await CheckDenomination(null, model.Amount);

            if (checkDenomination is true)
            {
                Errors.Add($"Amount {model.Amount} already exists");
                return new BaseResponse<bool>($"Amount {model.Amount} already exists.", Errors);
            }

            var newDenomination = new Denomination
            {
                Amount = model.Amount,
                CreatedBy = UserId,
            };

            _context.Add(newDenomination);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, "Successfully Created the Denomination");
        }

        /// <summary>
        /// DELETE RATE
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteDenomination(Guid id)
        {
            var checkDenomination = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == id);

            if (checkDenomination is null)
            {
                Errors.Add("Denomination doesn't exists.");
                return new BaseResponse<bool>("Denomination doesn't exists.", Errors);
            }

            var _ = await _context.CardTypeDenomination.AnyAsync(x => x.PrefixId == id);

            if (_)
            {
                Errors.Add("Denomination Cannot be deleted.");
                return new BaseResponse<bool>("Denomination Cannot be deleted.", Errors);
            }

            _context.Denominations.Remove(checkDenomination);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, "Successfully deleted the Denomination!");
        }

        /// <summary>
        /// GET ALL RATES NON-PAGINATED
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;RateDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<DenominationDTO>>> GetAllDenominations()
        {
            var denomination = await _context.Denominations.OrderBy(x => x.Amount).ToListAsync();

            var denominationDTO = denomination.Select(x => (DenominationDTO)x).ToList();

            return new BaseResponse<List<DenominationDTO>>
            {
                Data = denominationDTO,
                TotalCount = denominationDTO.Count,
                Status = RequestExecution.Successful,
                ResponseMessage = $"Found {denominationDTO.Count} Denomination(s)."
            };
        }

        /// <summary>
        /// GET RATE    
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;RateDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<DenominationDTO>> GetDenomination(Guid id)
        {
            var checkDenomination = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == id);

            if (checkDenomination is null)
            {
                return new BaseResponse<DenominationDTO>
                {
                    Data = null,
                    ResponseMessage = "Denomination doesn't exists.",
                    Errors = new List<string> { "Denomination doesn't exists." },
                    Status = RequestExecution.Failed
                };
            }

            DenominationDTO rateDTO = checkDenomination;

            return new BaseResponse<DenominationDTO> { Data = rateDTO, ResponseMessage = "Successfully Found the Denomination", Status = RequestExecution.Successful };
        }

        /// <summary>
        /// UPDATE RATE    
        /// </summary>
        /// <param name="model">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateDenomination(UpdateDenominationDTO model, Guid UserId)
        {
            var checkDenomination = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (checkDenomination is null)
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    ResponseMessage = "Denomination doesn't exists.",
                    Errors = new List<string> { "Denomination doesn't exists." },
                    Status = RequestExecution.Failed
                };
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

            return new BaseResponse<bool>
            {
                Data = true,
                ResponseMessage = "Successfully updated the Denomination",
                Status = RequestExecution.Successful
            };
        }

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
