using Optima.Models.DTO.NotificationDTO;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IPushNotificationService
    {
        Task SendPushNotification(SendPushNotificationDTO model);
        
        Task<BaseResponse<string>> RegisterForPush(DeviceDTO model);
     
        Task<ResponseModel> TestPushNotification(TestPushNotificationDTO model);
      
        Task<BaseResponse<string>> DeleteDeviceToken(Guid userId, string deviceToken);
    }
}
