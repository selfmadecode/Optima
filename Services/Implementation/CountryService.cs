﻿using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
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
    public class CountryService : ICountryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CountryService(ApplicationDbContext context, IConfiguration configuration)
        { 
            _context = context;
            _configuration = configuration;           
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

            var result = ValidateFile(model.Logo);

            if (result.Errors.Any())
            {
                response.ResponseMessage = result.ResponseMessage;
                response.Errors = result.Errors;
                response.Status = RequestExecution.Failed;
                return response;
            }


            //Upload to Cloudinary
            var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(model.Logo, _configuration);

            /*if (hasUploadError)
            {
                resultModel.Message = $"{responseMessage}";
                resultModel.AddError("Failed to Upload the file");
                return resultModel;
            }*/

            var newCountry = new Country
            {
                Name = model.CountryName,
                LogoUrl = uploadedFile,
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

            var _ = await _context.CardTypes.AnyAsync(x => x.CountryId == id);
            if (_)
            {
                response.Data = false;
                response.ResponseMessage = "You cannot delete this country.";
                response.Errors = new List<string> { "You cannot delete this country." };
                response.Status = RequestExecution.Failed;
                return response;
            }

            _context.Remove(country);
            await _context.SaveChangesAsync();

            //Delete Image From Cloudinary
            var splittedLogoUrl = country.LogoUrl.Split("/");

            //get the cloudinary PublicId
            var LogoPublicId = splittedLogoUrl[8];
            var splittedLogoPublicId = LogoPublicId.Split(".");

            //Get the Full Asset Path
            var fullPath = $"Optima/{splittedLogoPublicId[0]}";
            CloudinaryUploadHelper.DeleteImage(_configuration, fullPath);

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

            return new BaseResponse<List<CountryDTO>> { Data = countriesDTO, ResponseMessage = $"Found {countriesDTO.Count()} Countries" };
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

            var result = ValidateFile(model.Logo);

            if (result.Errors.Any())
            {
                response.ResponseMessage = result.ResponseMessage;
                response.Errors = result.Errors;
                response.Status = RequestExecution.Failed;
                return response;
            }

            var country = await _context.Countries.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (country is null)
            {
                response.Data = false;
                response.ResponseMessage = "Country doesn't Exists.";
                response.Errors.Add("Country doesn't Exists.");
                response.Status = RequestExecution.Failed;
                return response;
            }

            if (model.Name.Replace(" ", "").ToLower() != country.Name.Replace(" ", "").ToLower())
            {
                var checkExistingCountries = await _context.Countries.AnyAsync(x => x.Name.ToLower().Replace(" ", "") == model.Name.ToLower().Replace(" ", ""));

                if (checkExistingCountries)
                {
                    response.Data = false;
                    response.ResponseMessage = "Country already Exists.";
                    response.Errors.Add("Country already Exists.");
                    response.Status = RequestExecution.Failed;
                    return response;
                }
            }
          
            
            if (!(model.Logo is null) && !(country.LogoUrl is null))
            {

                var splittedLogoUrl = country.LogoUrl.Split("/");

                //get the cloudinary PublicId
                var LogoPublicId = splittedLogoUrl[8];
                var splittedLogoPublicId = LogoPublicId.Split(".");

                //Get the Full Asset Path
                var fullPath = $"Optima/{splittedLogoPublicId[0]}";
                CloudinaryUploadHelper.DeleteImage(_configuration, fullPath);


                var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(model.Logo, _configuration);

                /*if (hasUploadError)
                {
                    resultModel.Message = $"{responseMessage}";
                    resultModel.AddError("Failed to Upload the file");
                    return resultModel;
                }*/

                country.LogoUrl = uploadedFile;
            }

            if (!(model.Logo is null) && (country.LogoUrl is null))
            {
                var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(model.Logo, _configuration);

                /*if (hasUploadError)
                {
                    resultModel.Message = $"{responseMessage}";
                    resultModel.AddError("Failed to Upload the file");
                    return resultModel;
                }*/

                country.LogoUrl = uploadedFile;
            }


            country.Name = string.IsNullOrWhiteSpace(model.Name) ? country.Name : model.Name;
            country.ModifiedBy = UserId;
            country.ModifiedOn = DateTime.UtcNow;
            _context.Countries.Update(country);
            
           
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = "Country Updated Successfully";
            response.Status = RequestExecution.Successful;
            return response;
        }


        /// <summary>
        /// VALIDATE FILE
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>BaseResponse&lt;bool&gt;.</returns>
        private BaseResponse<bool> ValidateFile(IFormFile file)
        {
            var response = new BaseResponse<bool>();

            if (!(file is null))
            {
                if (file.Length > 1024 * 1024)
                {
                    response.ResponseMessage = "Logo file size must not exceed 1Mb";
                    response.Errors.Add("Logo file size must not exceed 1Mb");
                    response.Status = RequestExecution.Failed;
                    return response;
                }

                var error = ValidateFileTypeHelper.ValidateFile(new[] { "jpg", "png", "jpeg" }, file.FileName);

                if (!error)
                {
                    response.ResponseMessage = "Logo file type must be .jpg or .png or .jpeg";
                    response.Errors.Add("Logo file type must be .jpg or .png or .jpeg");
                    response.Status = RequestExecution.Failed;
                    return response;
                }

            }

            return response;
        }
    }
}
