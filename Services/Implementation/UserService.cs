using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.UserDTOs;
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
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILog _logger;

        public UserService(ApplicationDbContext context, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _logger = LogManager.GetLogger(typeof(CountryService));
            _userManager = userManager;

        }


        /// <summary>
        /// GET A USER
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;CardDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<UserDTO>> AUser(Guid UserId)
        {
            var response = new BaseResponse<UserDTO>();

            var user = await GetUserById(UserId);

            if (user is null)
            {
                response.Data = null;
                response.ResponseMessage = ResponseMessage.ErrorMessage000;
                response.Errors.Add(ResponseMessage.ErrorMessage000);
                response.Status = RequestExecution.Failed;
                return response;
            }

            var userDTO = user;

            response.Data = userDTO;
            response.ResponseMessage = ResponseMessage.SuccessMessage000;
            return response;

        }


        /// <summary>
        /// GET ALL USERS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;CardDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<UserDTO>>> AllUsers(BaseSearchViewModel model)
        {
            var query = _userManager.Users.AsQueryable();

            var users = await EntityFilter(query, model).OrderByDescending(x => x.CreationTime).ToPagedListAsync(model.PageIndex, model.PageSize);
            var usersDTO = users.Select(x => (UserDTO)x).ToList();

            var data = new PagedList<UserDTO>(usersDTO, model.PageIndex, model.PageSize, users.TotalItemCount);
            return new BaseResponse<PagedList<UserDTO>> { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"Found {usersDTO.Count} Users" };
        }


        /// <summary>
        /// UPDATE USER PROFILE
        /// </summary>
        /// <param name="model">the model</param>
        /// <param name="UserId">the UserId</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateProfile(UpdateUserDTO model, Guid UserId)
        {
            var uploadedFileToDelete = string.Empty;

            try
            {
                var response = new BaseResponse<bool>();

                var user = await _userManager.FindByIdAsync(UserId.ToString());

                if (user is null)
                {
                    response.ResponseMessage = ResponseMessage.ErrorMessage000;
                    response.Errors.Add(ResponseMessage.ErrorMessage000);
                    response.Status = RequestExecution.Failed;
                    return response;
                }


                if (!(model.ProfilePicture is null) && !(user.ProfilePicture is null))
                {

                    //Get the Full Asset Path
                    var fullPath = GenerateDeleteUploadedPath(uploadedFileToDelete);
                    CloudinaryUploadHelper.DeleteImage(_configuration, fullPath);

                    var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(model.ProfilePicture, _configuration);

                    user.ProfilePicture = uploadedFile;

                    //Set this Property to delete uploaded cloudinary file if an exception occur
                    uploadedFileToDelete = uploadedFile;

                }

                if (!(model.ProfilePicture is null) && (user.ProfilePicture is null))
                {
                    var (uploadedFile, hasUploadError, responseMessage) = await CloudinaryUploadHelper.UploadImage(model.ProfilePicture, _configuration);

                   user.ProfilePicture = uploadedFile;

                    //Set this Property to delete uploaded cloudinary file if an exception occur
                    uploadedFileToDelete = uploadedFile;
                }

                user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? user.PhoneNumber : model.PhoneNumber;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();

                response.Data = true;
                response.ResponseMessage = "Successfully Updated your Information";
                return response;
            }
            catch (Exception ex)
            {
                CloudinaryUploadHelper.DeleteImage(_configuration, GenerateDeleteUploadedPath(uploadedFileToDelete));
                _logger.Error(ex.Message, ex);

                throw;
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


        /// <summary>
        /// ENTITY FILTER
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="query">The query.</param>
        /// <returns>Task&lt;ApplicationUser&gt;.</returns>
        private IQueryable<ApplicationUser> EntityFilter(IQueryable<ApplicationUser> query, BaseSearchViewModel model)
        {

            if (!string.IsNullOrEmpty(model.Keyword) && !string.IsNullOrEmpty(model.Filter))
            {

                switch (model.Filter)
                {
                    case "UserType":
                        {
                            query = query.Where(x => x.UserType == model.Keyword.Parse<UserTypes>());
                            break;
                        }

                    case "FullName":
                        {
                            query = query.Where(x => x.FullName.ToLower() == model.Keyword.ToLower());
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return query;
        }

        /// <summary>
        ///  GET A USER
        /// </summary>
        /// <param name="id"></param>
        /// <returns>&lt;Task<ApplicationUser>&gt;</returns>
        private async Task<ApplicationUser> GetUserById(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

    }
}
