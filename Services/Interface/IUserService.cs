using AzureRays.Shared.ViewModels;
using Optima.Models.DTO.UserDTOs;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IUserService
    {
        Task<BaseResponse<bool>> UpdateProfile(UpdateUserDTO model, Guid UserId);
        Task<BaseResponse<PagedList<UserDTO>>> ActiveUsers(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<UserDTO>>> InActiveUsers(BaseSearchViewModel model);
        Task<BaseResponse<PagedList<UserDTO>>> DisabledUsers(BaseSearchViewModel model); 
        Task<BaseResponse<PagedList<UserDTO>>> AllUsers(BaseSearchViewModel model);
        Task<BaseResponse<UserDTO>> AUser(Guid UserId);
        Task<BaseResponse<UserDetailDTO>> UserDetails(Guid UserId);
       

    }
}
