using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.DTO.NotificationDTO;
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
    public class PushNotificationController : BaseController
    {
        private readonly IPushNotificationService _pushNotificationService;

        public PushNotificationController(IPushNotificationService pushNotificationService)
        {
            _pushNotificationService = pushNotificationService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<string>), 200)]
        public async Task<IActionResult> Create([FromBody] DeviceDTO model)
        {
            try
            {
                return ReturnResponse(await _pushNotificationService.RegisterForPush(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<ResponseModel>), 200)]
        public async Task<IActionResult> TestPush(TestPushNotificationDTO model) 
        {
            try
            {
                return ReturnResponse(await _pushNotificationService.TestPushNotification(model));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

        [Authorize]
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResponse<string>), 200)]
        public async Task<IActionResult> DeleteToken(string deviceToken) 
        {
            try
            {
                return ReturnResponse(await _pushNotificationService.DeleteDeviceToken(UserId, deviceToken));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }

        }

       
    }
}
