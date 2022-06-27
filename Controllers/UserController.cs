using AzureRays.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.UserDTOs;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedList<UserDTO>>), 200)]
        public async Task<IActionResult> Active([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _userService.ActiveUsers(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedList<UserDTO>>), 200)]
        public async Task<IActionResult> InActive([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _userService.InActiveUsers(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedList<UserDTO>>), 200)]
        public async Task<IActionResult> Disabled([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _userService.DisabledUsers(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Profile([FromForm] UpdateUserDTO model)
        {
            try
            {
                return ReturnResponse(await _userService.UpdateProfile(model, UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }


        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedList<UserDTO>>), 200)] 
        public async Task<IActionResult> Users([FromQuery] BaseSearchViewModel model)
        {
            try
            {
                return ReturnResponse(await _userService.AllUsers(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex); ;
            }
        }


        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(BaseResponse<UserDTO>), 200)]
        public async Task<IActionResult> Get(Guid userId)   
        {
            try
            {
                return ReturnResponse(await _userService.UserDetails(userId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
        
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(BaseResponse<UserDetailDTO>), 200)] 
        public async Task<IActionResult> BankAndTransactionDetails(Guid userId)   
        {
            try
            {
                return ReturnResponse(await _userService.GetUserBankAndTransactionDetails(userId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
