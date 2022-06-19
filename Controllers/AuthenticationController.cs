using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.AuthDTO;
using Optima.Services.Interface;
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
                return ReturnResponse(await _authService.Login(model, CurrentDateTime));            }
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

        [HttpPost("{email}")]
        public async Task<IActionResult> LogOut(string email)
        {
            try
            {
                return ReturnResponse(await _authService.UpdateUserLastLogin(email, CurrentDateTime));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
             
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
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
                
        [HttpPost("{userEmail}")]
        //[Authorize(Roles = AppRoles.AdminRole)]
        public async Task<IActionResult> LockoutUser(string userEmail)
        {
            try
            {
                return ReturnResponse(await _authService.LockoutUser(userEmail));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
                
        [HttpPost("{userEmail}")]
        //[Authorize(Roles = AppRoles.AdminRole)]
        public async Task<IActionResult> UnLockUser(string userEmail)
        {
            try
            {
                return ReturnResponse(await _authService.UnLockUser(userEmail));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
        
    }
}
