using Optima.Models.DTO.AuthDTO;
using Optima.Models.DTO.UserDTOs;
using Optima.Models.Entities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IAuthenticationService
    {
        Task<BaseResponse<JwtResponseDTO>> Login(LoginDTO model, DateTime CurrentDateTime);
        Task<BaseResponse<JwtResponseDTO>> RefreshToken(string AccessToken, string RefreshToken);
        (string, DateTime) CreateJwtTokenAsync(ApplicationUser user, IList<string> userRoles);
        Task<BaseResponse<bool>> UpdateUserLastLogin(string emailAddress, DateTime CurrentDate);
        Task<BaseResponse<RegisterDTO>> Register(RegisterDTO model);
        Task<BaseResponse<string>> ConfirmEmail(string token, string email);
        Task<BaseResponse<ForgotPasswordDTO>> ForgotPassword(ForgotPasswordDTO model);
        Task<BaseResponse<ChangePasswordDTO>> ChangePassword(ChangePasswordDTO model);
        Task<BaseResponse<ResetPasswordDTO>> ResetPassword(ResetPasswordDTO model);
        Task<BaseResponse<string>> LockoutUser(string emailAddress);
        Task<BaseResponse<string>> UnLockUser(string emailAddress);
        Task<BaseResponse<AdminDTO>> CreateAdmin(CreateAdminAccountDTO model);
        Task<BaseResponse<UpdateClaimDTO>> UpdateClaimsAsync(UpdateClaimDTO model);
        Task<BaseResponse<AdminDetailsDTO>> GetAdminDetailsAndPermmissionsAsync(string email);
        Task<BaseResponse<AdminDetailsDTO>> GetAdminDetailsAndPermmissionsAsync(Guid UserId);

        Task<BaseResponse<List<AdminDetailsDTO>>> GetAllAdmins();
    }
}
