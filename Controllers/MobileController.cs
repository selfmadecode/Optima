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
    public class MobileController : BaseController
    {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILog _logger;

        public MobileController(IPushNotificationService pushNotificationService)
        {
            _pushNotificationService = pushNotificationService;
            _logger = LogManager.GetLogger(typeof(MobileController));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<string>), 200)]
        public async Task<IActionResult> Create([FromBody] DeviceDTO model)
        {
            try
            {
                var result = await _pushNotificationService.RegisterForPush(model);

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<ResponseModel>), 200)]
        public async Task<IActionResult> TestPush(TestPushNotificationDTO model) 
        {
            try
            {
                var result = await _pushNotificationService.TestPushNotification(model);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
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
                var result = await _pushNotificationService.DeleteDeviceToken(UserId, deviceToken);

                if (result.Errors.Any())
                    return ReturnResponse(result);

                return ReturnResponse(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return HandleError(ex);
            }

        }

       
    }
}
