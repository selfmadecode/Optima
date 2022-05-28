using AzureRays.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.DTO.RateDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class DenominationService : IDenominationService
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
            var checkRate = await CheckDenomination(null, model.Amount);

            if (checkRate is true)
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    ResponseMessage = $"Amount {model.Amount} already exists.",
                    Errors = new List<string> { $"Amount {model.Amount} already exists" },
                    Status = RequestExecution.Failed
                };
            }

            var newRate = new Denomination
            {
                Amount = model.Amount,
                CreatedBy = UserId,
            };

            _context.Add(newRate);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Data = true,
                ResponseMessage = $"Successfully Created the Denomination",
                Status = RequestExecution.Successful
            };
        }

        /// <summary>
        /// DELETE RATE
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteDenomination(Guid id)
        {

            var checkRate = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == id);

            if (checkRate is null)
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    ResponseMessage = "Denomination doesn't exists.",
                    Errors = new List<string> { "Denomination doesn't exists." },
                    Status = RequestExecution.Failed
                };
            }

            _context.Denominations.Remove(checkRate);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Data = false,
                ResponseMessage = "Successfully deleted the Denomination.",
                Status = RequestExecution.Successful
            };
        }

        /// <summary>
        /// GET ALL RATES NON-PAGINATED
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;RateDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<DenominationDTO>>> GetAllDenominations()
        {
            var rates = await _context.Denominations.OrderBy(x => x.Amount).ToListAsync();

            var ratesDTO = rates.Select(x => (DenominationDTO)x).ToList();

            return new BaseResponse<List<DenominationDTO>>
            {
                Data = ratesDTO,
                Status = RequestExecution.Successful,
                ResponseMessage = $"Found {rates.Count} Denomination(s)."
            };
        }

        /// <summary>
        /// GET RATE    
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;RateDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<DenominationDTO>> GetDenomination(Guid id)
        {
            var checkRate = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == id);

            if (checkRate is null)
            {
                return new BaseResponse<DenominationDTO>
                {
                    Data = null,
                    ResponseMessage = "Denomination doesn't exists.",
                    Errors = new List<string> { "Denomination doesn't exists." },
                    Status = RequestExecution.Failed
                };
            }

            DenominationDTO rateDTO = checkRate;

            return new BaseResponse<DenominationDTO> { Data = rateDTO, ResponseMessage = "Success", Status = RequestExecution.Successful };

        }

        /// <summary>
        /// UPDATE RATE    
        /// </summary>
        /// <param name="model">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateDenomination(UpdateDenominationDTO model, Guid UserId)
        {
            var checkRate = await _context.Denominations.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (checkRate is null)
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    ResponseMessage = "Denomination doesn't exists.",
                    Errors = new List<string> { "Denomination doesn't exists." },
                    Status = RequestExecution.Failed
                };
            }

            var response = await CheckDenomination(checkRate.Id, model.Amount);
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
