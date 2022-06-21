using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.BankAccountDTOs;
using Optima.Models.DTO.CardTransactionDTOs;
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
        private readonly ICloudinaryServices _cloudinaryServices;


        public UserService(ApplicationDbContext context, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, ICloudinaryServices cloudinaryServices)
        {
            _context = context;
            _configuration = configuration;
            _logger = LogManager.GetLogger(typeof(CountryService));
            _userManager = userManager;
            _cloudinaryServices = cloudinaryServices;
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
            var response = new BaseResponse<bool>();

            try
            {
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
                    var fullPath = GenerateDeleteUploadedPath(user.ProfilePicture);
                    await _cloudinaryServices.DeleteImage(fullPath);

                    var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(model.ProfilePicture);

                    user.ProfilePicture = uploadedFile;

                    //Set this Property to delete uploaded cloudinary file if an exception occur
                    uploadedFileToDelete = uploadedFile;

                }

                if (!(model.ProfilePicture is null) && (user.ProfilePicture is null))
                {
                    var (uploadedFile, hasUploadError, responseMessage) = await _cloudinaryServices.UploadImage(model.ProfilePicture);

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
                await _cloudinaryServices.DeleteImage(GenerateDeleteUploadedPath(uploadedFileToDelete));
                _logger.Error(ex.Message, ex);

                return response;
            }

        }


        /// <summary>
        /// GET USER DETAILS
        /// </summary>
        /// <param name="UserId">the UserId</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<UserDetailDTO>> UserDetails(Guid UserId)
        {
            var response = new BaseResponse<UserDetailDTO>();
            var data = new UserDetailDTO();

            var user = await GetUserById(UserId);
            if (user is null)
            {
                response.Data = null;
                response.Errors.Add(ResponseMessage.ErrorMessage000);
                response.Status = RequestExecution.Failed;
                response.ResponseMessage = ResponseMessage.ErrorMessage000;
                return response;
            }

            var users = await _context.Users.Where(x => x.Id == UserId).FirstOrDefaultAsync();

            data.UserDTO = users;
            data.BankAccountDTOs = GetUserBankAccount(UserId).Result.Select(x => (BankAccountDTO)x).ToList();
            data.CardTransactionDTOs = GetUserCardTransaction(UserId).Result.Select(x => (CardTransactionDTO)x).ToList();

            response.Data = data;
            response.ResponseMessage = ResponseMessage.SuccessMessage000;
            return response;
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
                    case "EmailAddress":
                        {
                            query = query.Where(x => x.Email.ToLower() == model.Keyword.ToLower()); 
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

        /// <summary>
        ///  GET A USER CARD TRANSACTION
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>&lt;Task<ApplicationUser>&gt;</returns>
        private async Task<List<CardTransaction>> GetUserCardTransaction(Guid userId) =>
           await _context.CardTransactions
                .Where(x => x.ApplicationUserId == userId)
                .OrderByDescending(x => x.CreatedOn)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        ///  GET A USER BANK ACCOUNTS
        /// </summary>
        /// <param name="id"></param>
        /// <returns>&lt;Task<ApplicationUser>&gt;</returns>
        private async Task<List<BankAccount>> GetUserBankAccount(Guid userId) =>
           await _context.BankAccounts
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync();
        
        
    }
}
