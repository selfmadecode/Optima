using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.AuthDTO;
using Optima.Models.DTO.UserDTOs;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationService _authService;

        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }
        
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.Login(model, CurrentDateTime));            
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO request)
        {
            try
            {
                return ReturnResponse(await _authService.RefreshToken(request.AccessToken, request.RefreshToken));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.Register(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                return ReturnResponse(await _authService.ConfirmEmail(token, email));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }
               
                
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.ForgotPassword(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
                
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.ResetPassword(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogOut([FromBody] UserEmailDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.UpdateUserLastLogin(model.UserName, CurrentDateTime));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
             
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordDTO model)
        {

            try
            {
                return ReturnResponse(await _authService.ChangePassword(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
                
        [HttpPost]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        public async Task<IActionResult> LockoutUser([FromBody]UserEmailDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.LockoutUser(model.UserName));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
                
        [HttpPost]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        public async Task<IActionResult> UnLockUser([FromBody]UserEmailDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.UnLockUser(model.UserName));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminAccountDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.CreateAdmin(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPut]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        public async Task<IActionResult> AdminPermmission([FromBody] UpdateClaimDTO model)
        {
            try
            {
                return ReturnResponse(await _authService.UpdateClaimsAsync(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [Authorize]
        //[Authorize(Roles = RoleHelper.SUPERADMIN)]
        public async Task<IActionResult> AdminDetails(string EmailAddress)
        {
            try
            {
                return ReturnResponse(await _authService.GetAdminDetailsAndPermmissionsAsync(EmailAddress));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CurrentAdminDetails()
        {
            try
            {
                return ReturnResponse(await _authService.GetAdminDetailsAndPermmissionsAsync(UserId));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [Authorize(Roles = RoleHelper.SUPERADMIN)]
        [ProducesResponseType(typeof(BaseResponse<List<AdminDetailsDTO>>), 200)]

        public async Task<IActionResult> Admins()
        {
            try
            {
                return ReturnResponse(await _authService.GetAllAdmins());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

    }
}
