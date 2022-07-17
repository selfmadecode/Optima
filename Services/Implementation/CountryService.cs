using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.CountryDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class CountryService : BaseService, ICountryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILog _logger;
        private readonly ICloudinaryServices _cloudinaryServices;


        public CountryService(ApplicationDbContext context, ICloudinaryServices cloudinaryServices, IConfiguration configuration)
        { 
            _context = context;
            _configuration = configuration;
            _logger = LogManager.GetLogger(typeof(CountryService));
            _cloudinaryServices = cloudinaryServices;
        }

        /// <summary>
        /// CREATE COUNTRY
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateCountry(CreateCountryDTO model, Guid UserId)
        {
            var uploadedFileToDelete = string.Empty;
        
            try
            {
                var checkCountry = await _context.Countries
                    .FirstOrDefaultAsync(x => x.Name.Replace(" ", "").ToLower() == model.CountryName.Replace(" ", "").ToLower());
                
                if (!(checkCountry is null))
                {
                    Errors.Add(ResponseMessage.CountryAlreadyExist);
                    return new BaseResponse<bool>(ResponseMessage.CountryAlreadyExist, Errors);
                }                

                //Upload to Cloudinary
                var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(model.Logo);
                //Set this Property to delete uploaded cloudinary file if an exception occur
                uploadedFileToDelete = uploadedFile;

                var newCountry = new Country
                {
                    Name = model.CountryName,
                    LogoUrl = uploadedFile,
                    CreatedBy = UserId
                };

                _context.Countries.Add(newCountry);
                await _context.SaveChangesAsync();

                return new BaseResponse<bool>(true, ResponseMessage.CountryCreated);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(uploadedFileToDelete))
                {
                    await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(uploadedFileToDelete));
                    _logger.Error(ex.Message, ex);
                }

                return new BaseResponse<bool>();
            }           
        }

        /// <summary>
        /// DELETE COUNTRY
        /// </summary>
        /// <param name="id">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteCountry(Guid id)
        {
            var country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == id);

            if (country is null)
            {
                return new BaseResponse<bool>(ResponseMessage.CountryDoesNotExist, Errors);
            }

            var _ = await _context.CardTypes.AnyAsync(x => x.CountryId == id);

            if (_)                            
                return new BaseResponse<bool>(ResponseMessage.CountryCannotBeDeleted, Errors);
            

            _context.Remove(country);
            await _context.SaveChangesAsync();

            var fullPath = GenerateDeleteUploadedPath(country.LogoUrl);
            await _cloudinaryServices.DeleteImage(fullPath);

            return new BaseResponse<bool>(true, ResponseMessage.CountryDeleted);
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

            return new BaseResponse<PagedList<CountryDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"Found {countriesDTO.Count()} Countries", Status = RequestExecution.Successful };
        }

        /// <summary>
        /// GET ALL COUNTRY NON-PAGINATED
        /// </summary>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;CountryDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<CountryDTO>>> GetAllCountry()
        {
            var countries = await _context.Countries.OrderBy(x => x.Name).ToListAsync();

            var countriesDTO = countries.Select(X => (CountryDTO)X).ToList();

            return new BaseResponse<List<CountryDTO>> { Data = countriesDTO, TotalCount = countriesDTO.Count, ResponseMessage = $"Found {countriesDTO.Count()} Countries" };
        }

        /// <summary>
        /// GET A COUNTRY
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<CountryDTO>> GetCountry(Guid id) 
        {
            var country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == id);

            if (country is null)
            {
                return new BaseResponse<CountryDTO>(ResponseMessage.CountryNotFound, Errors);
            }

            CountryDTO countryDTO = country;

            return new BaseResponse<CountryDTO>(countryDTO);
        }

        /// <summary>
        /// UPDATE A COUNTRY
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateCountry(UpdateCountryDTO model, Guid UserId, Guid CountryId) 
        {
            var uploadedFileToDelete = string.Empty;

            try
            {            

                var country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == CountryId);

                if (country is null)
                {
                    return new BaseResponse<bool>(ResponseMessage.CountryNotFound, Errors);
                }

                if (!String.IsNullOrWhiteSpace(model.Name))
                {
                    if (model.Name.Replace(" ", "").ToLower() != country.Name.Replace(" ", "").ToLower())
                    {
                        var checkExistingCountries = await _context.Countries.AnyAsync(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

                        if (checkExistingCountries)
                        {
                            Errors.Add(ResponseMessage.CountryAlreadyExist);
                            return new BaseResponse<bool>(ResponseMessage.CountryAlreadyExist, Errors);
                        }

                        country.Name = model.Name;
                    }
                }



                if (!(model.Logo is null) && !(country.LogoUrl is null))
                {

                    //Get the Full Asset Path
                    var fullPath = GenerateDeleteUploadedPath(country.LogoUrl);
                    await _cloudinaryServices.DeleteImage(fullPath);

                    var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(model.Logo);

                    country.LogoUrl = uploadedFile;

                    //Set this Property to delete uploaded cloudinary file if an exception occur
                    uploadedFileToDelete = uploadedFile;

                }

                if (!(model.Logo is null) && (country.LogoUrl is null))
                {
                    var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(model.Logo);

                    country.LogoUrl = uploadedFile;

                    //Set this Property to delete uploaded cloudinary file if an exception occur
                    uploadedFileToDelete = uploadedFile;
                }
              

                country.Name = string.IsNullOrWhiteSpace(model.Name) ? country.Name : model.Name;
                country.ModifiedBy = UserId;
                country.ModifiedOn = DateTime.UtcNow;
                _context.Countries.Update(country);


                await _context.SaveChangesAsync();                             

                return new BaseResponse<bool>("Country Updated Successfully");
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(uploadedFileToDelete))
                {
                    await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(uploadedFileToDelete));
                    _logger.Error(ex.Message, ex);
                }
                return new BaseResponse<bool>();
            }
           
        }

 
        /// <summary>
        /// GENERATE DELETE UPLOADED PATH
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>BaseResponse&lt;bool&gt;.</returns>
        private string GenerateDeleteUploadedPath(string value)
        {
            //Delete Image From Cloudinary
            var splittedLogoUrl = value.Split("/");

            //get the cloudinary PublicId
            var LogoPublicId = splittedLogoUrl[8];
            var splittedLogoPublicId = LogoPublicId.Split(".");

            //Get the Full Asset Path
            var fullPath = $"Optima/{splittedLogoPublicId[0]}";

            return fullPath;
        }
    }
}
