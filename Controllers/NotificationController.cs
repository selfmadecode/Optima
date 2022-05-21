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
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<GetNotificationDTO>>), 200)]
        public async Task<IActionResult> UnRead()
        {
            try
            {
                return ReturnResponse(await _notificationService.GetUserUnReadNotification(UserId));
            }
            catch (Exception ex)
            {

                return HandleError(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<int>), 200)]
        public async Task<IActionResult> UserCount()
        {
            try
            {
                return ReturnResponse(await _notificationService.GetUserUnreadNotificationCount(UserId));
            }
            catch (Exception ex)
            {

                return HandleError(ex);
            }
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        [ProducesResponseType(typeof(BaseResponse<int>), 200)]
        public async Task<IActionResult> AdminCount()
        {
            try
            {
                return ReturnResponse(await _notificationService.GetAdminUnreadNotificationCount());
            }
            catch (Exception ex)
            {

                return HandleError(ex);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<Guid>), 200)]
        public async Task<IActionResult> Read(Guid id)
        {
            try
            {
                return ReturnResponse(await _notificationService.ReadNotification(id));
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<List<GetNotificationDTO>>), 200)]
        public async Task<IActionResult> Admin()
        {
            try
            {
                return ReturnResponse(await _notificationService.GetAdminNotification());
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
