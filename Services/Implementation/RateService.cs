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
    public class RateService : IRateService
    {
        private readonly ApplicationDbContext _context;

        public RateService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// CREATE RATE
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateRate(CreateRateDTO model)
        {
            var checkRate = await _context.Rates.FirstOrDefaultAsync(x => x.Amount == model.Amount);

            if(!(checkRate is null))
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    ResponseMessage = $"Amount {model.Amount} already exists.",
                    Errors = new List<string> { $"Amount {model.Amount} already exists" },
                    Status = RequestExecution.Failed
                };
            }

            var newRate = new Rate
            {
                Amount = model.Amount
            };

            _context.Add(newRate);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Data = false,
                ResponseMessage = $"Successfully Created the rate",
                Status = RequestExecution.Successful
            };
        }

        /// <summary>
        /// DELETE RATE
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteRate(Guid id)
        {

            var checkRate = await _context.Rates.FirstOrDefaultAsync(x => x.Id == id);

            if (checkRate is null)
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    ResponseMessage = "Rate doesn't exists.",
                    Errors = new List<string> { "Rate doesn't exists." },
                    Status = RequestExecution.Failed
                };
            }

            _context.Rates.Remove(checkRate);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Data = false,
                ResponseMessage = "Successfully deleted the rate.",
                Status = RequestExecution.Successful
            };
        }

        /// <summary>
        /// GET ALL RATES NON-PAGINATED
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;RateDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<RateDTO>>> GetAllRates()
        {
            var rates = await _context.Rates.ToListAsync();

            var ratesDTO = rates.Select(x => (RateDTO)x).ToList();

            return new BaseResponse<List<RateDTO>>
            {
                Data = ratesDTO,
                Status = RequestExecution.Successful,
                ResponseMessage = $"Found {rates.Count} rates"
            };
        }

        /// <summary>
        /// GET RATE    
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;RateDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<RateDTO>> GetRate(Guid id)
        {
            var checkRate = await _context.Rates.FirstOrDefaultAsync(x => x.Id == id);

            if (checkRate is null)
            {
                return new BaseResponse<RateDTO>
                {
                    Data = null,
                    ResponseMessage = "Rate doesn't exists.",
                    Errors = new List<string> { "Rate doesn't exists." },
                    Status = RequestExecution.Failed
                };
            }

            RateDTO rateDTO = checkRate;

            return new BaseResponse<RateDTO> { Data = rateDTO, ResponseMessage = "Success", Status = RequestExecution.Successful };

        }

        /// <summary>
        /// UPDATE RATE    
        /// </summary>
        /// <param name="model">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateRate(UpdateRateDTO model)
        {
            var checkRate = await _context.Rates.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (checkRate is null)
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    ResponseMessage = "Rate doesn't exists.",
                    Errors = new List<string> { "Rate doesn't exists." },
                    Status = RequestExecution.Failed
                };
            }

            checkRate.Amount = model.Amount;

            _context.Update(checkRate);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Data = false,
                ResponseMessage = "Successfully updated the rate",
                Status = RequestExecution.Successful
            };
        }
    }
}
