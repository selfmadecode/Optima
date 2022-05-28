using AzureRays.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.DTO.CountryDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class CountryService : ICountryService
    {
        private readonly ApplicationDbContext _context;
       

        public CountryService(ApplicationDbContext context)
        { 
            _context = context;
           
        }

        /// <summary>
        /// CREATE COUNTRY
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateCountry(CreateCountryDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            var checkCountry = await _context.Countries
                .FirstOrDefaultAsync(x => x.Name.Replace(" ", "").ToLower() == model.CountryName.Replace(" ", "").ToLower());       

            if (!(checkCountry is null))
            {
                response.Data = false;
                response.ResponseMessage = "Country already Exists.";
                response.Errors.Add("Country already Exists.");
                response.Status = RequestExecution.Failed;
                return response;
            }

            var newCountry = new Country
            {
                Name = model.CountryName,
                CreatedBy = UserId
            };

             _context.Countries.Add(newCountry);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.Status = RequestExecution.Successful;
            response.ResponseMessage = $"Successfully Created the Country.";
            return response;

        }

        /// <summary>
        /// DELETE COUNTRY
        /// </summary>
        /// <param name="id">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteCountry(Guid id)
        {
            var response = new BaseResponse<bool>();

            var country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == id);

            if (country is null)
            {
                response.Data = false;
                response.ResponseMessage = "Country doesn't Exists.";
                response.Errors.Add("Country doesn't Exists.");
                response.Status = RequestExecution.Failed;
                return response;
            }

            _context.Remove(country);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Success deleted the Country";
            response.Status = RequestExecution.Successful;
            return response;
        }

        /// <summary>
        /// GET ALL COUNTRY PAGINATED
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CountryDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<CountryDTO>>> GetAllCountry(BaseSearchViewModel model)
        {
            var query = _context.Countries.AsQueryable();

            var countries = await query.OrderBy(x => x.Name).ToPagedListAsync(model.PageIndex, model.PageSize);

            var countriesDTO = countries.Select(X => (CountryDTO)X).ToList();

            var data = new PagedList<CountryDTO>(countriesDTO, model.PageIndex, model.PageSize, countries.TotalItemCount);

            return new BaseResponse<PagedList<CountryDTO>> { Data = data, ResponseMessage = $"Found {countriesDTO.Count()} Countries", Status = RequestExecution.Successful };
        }

        /// <summary>
        /// GET ALL COUNTRY NON-PAGINATED
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;CountryDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<CountryDTO>>> GetAllCountry()
        {
            var countries = await _context.Countries.OrderBy(x => x.Name).ToListAsync();

            var countriesDTO = countries.Select(X => (CountryDTO)X).ToList();

            return new BaseResponse<List<CountryDTO>> { Data = countriesDTO, ResponseMessage = $"Found {countriesDTO.Count()} Countries", Status = RequestExecution.Successful };
        }

        /// <summary>
        /// GET A COUNTRY
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<CountryDTO>> GetCountry(Guid id) 
        {
            var response = new BaseResponse<CountryDTO>();

            var country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == id);

            if (country is null)
            {
                response.Data = null;
                response.ResponseMessage = "Country doesn't Exists.";
                response.Errors.Add("Country doesn't Exists.");
                response.Status = RequestExecution.Failed;
                return response;
            }

            CountryDTO countryDTO = country;


            response.Data = countryDTO;
            response.ResponseMessage = "Success.";
            response.Status = RequestExecution.Successful;
            return response;

        }

        /// <summary>
        /// UPDATE A COUNTRY
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateCountry(UpdateCountryDTO model, Guid UserId) 
        {
            var response = new BaseResponse<bool>();

            var country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (country is null)
            {
                response.Data = false;
                response.ResponseMessage = "Country doesn't Exists.";
                response.Errors.Add("Country doesn't Exists.");
                response.Status = RequestExecution.Failed;
                return response;
            }

            var checkExistingCountries = _context.Countries.Any(x => x.Name == model.Name);

            if (!checkExistingCountries)
            {
                var newCountry = new Country
                {
                    Name = model.Name,
                    CreatedBy = UserId,
                };

                _context.Add(newCountry);
            }
            else
            {
                country.Name = model.Name;
                country.ModifiedBy = UserId;
                country.ModifiedOn = DateTime.UtcNow;
                _context.Countries.Update(country);
            }
           
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Success";
            response.Status = RequestExecution.Successful;
            return response;
        }
    }
}
