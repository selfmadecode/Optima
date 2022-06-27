using Optima.Models.DTO.NotificationDTO;
using Optima.Models.Enums;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface INotificationService
    {
        Task<BaseResponse<bool>> CreateNotificationForUser(CreateNotificationDTO model, Guid UserId);

        Task<BaseResponse<bool>> CreateNotificationForAdmin(CreateAdminNotificationDTO model);
        Task<BaseResponse<List<GetNotificationDTO>>> GetUserUnReadNotification(Guid userId);
        Task<BaseResponse<List<GetNotificationDTO>>> GetAdminNotification(Guid UserId);

        Task<BaseResponse<Guid>> ReadNotification(Guid NotificationId);
        Task<BaseResponse<int>> GetUserUnreadNotificationCount(Guid UserId);
        Task<BaseResponse<int>> GetAdminUnreadNotificationCount();
    }
}
