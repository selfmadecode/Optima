using CorePush.Google;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.NotificationDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static Optima.Models.DTO.NotificationDTO.GoogleNotification;

namespace Optima.Services.Implementation
{
    public class PushNotificationService : BaseService, IPushNotificationService
    {
        /// <summary>
        /// The FMC notif setting
        /// </summary>
        private readonly FcmNotification _fmcNotifSetting;
        /// <summary>
        /// The user manager
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;
        /// <summary>
        /// The device repo
        /// </summary>
        private readonly ApplicationDbContext _context;

        private ILog _logger;
        

        public PushNotificationService(ApplicationDbContext context, IOptions<FcmNotification> fmcNotifSetting, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _fmcNotifSetting = fmcNotifSetting.Value;
            _userManager = userManager;
            _logger = LogManager.GetLogger(typeof(PushNotificationService));
           
        }

        /// <summary>
        /// DELETES THE DEVICE TOKEN
        /// </summary>
        /// <param name="userId">The user identifier.</param
        /// <param name="deviceToken">The user identifier.</param>
        /// <returns>Task&lt;ResultModel&lt;System.Int32&gt;&gt;.</returns>
        public async Task<BaseResponse<string>> DeleteDeviceToken(Guid userId, string deviceToken)
        {
            var tokens = await _context.UserDevices.Where(x => x.UserId == userId && x.Token == deviceToken).ToListAsync();
            if (!tokens.Any())
            {
                Errors.Add(ResponseMessage.DeviceTokenNotFound);
                return new BaseResponse<string>(ResponseMessage.DeviceTokenNotFound, Errors);
            }
               
            _context.UserDevices.RemoveRange(tokens);
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Deleted a User Device Token");
            return new BaseResponse<string>(ResponseMessage.SuccessMessage000, ResponseMessage.DeviceTokenDeleted);
        }

        /// <summary>
        /// REGISTERS THE DEVICE TOKEN FOR PUSH NOTIFICATION
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;ResultModel&lt;System.String&gt;&gt;.</returns>
        public async Task<BaseResponse<string>> RegisterForPush(DeviceDTO model)
        {

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());

            if (user is null)
            {
                Errors.Add(ResponseMessage.ErrorMessage000);
                return new BaseResponse<string>(ResponseMessage.ErrorMessage000, Errors);
            };

           
            var deviceTokens = _context.UserDevices.Where(x => x.Token == model.Token).ToList();

            foreach (var deviceToken in deviceTokens)
            {
                if (deviceToken != null)
                {
                    deviceToken.UserId = user.Id;
                    _context.Update(deviceToken);
                    await _context.SaveChangesAsync();
                    _logger.Info("Updated a User device token... at Execution:RegisterForPush");

                }
            }

            if (!deviceTokens.Any())
            {
                var newDevice = new UserDevice
                {
                    Token = model.Token,
                    UserId = model.UserId,
                    CreatedOn = DateTime.UtcNow,
                };

                _context.UserDevices.Add(newDevice);
                await _context.SaveChangesAsync();
                _logger.Info("Added a User Device Token... at Execution:RegisterForPush");
            }

            return new BaseResponse<string>(ResponseMessage.SuccessMessage000, ResponseMessage.DeviceTokenCreated);
        }

        /// <summary>
        /// SENDS THE PUSH NOTIFICATION
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task.</returns>
        public async Task SendPushNotification(SendPushNotificationDTO model)
        {
            if (!model.UserIds.Any())
            {
                return;
            }
            var deviceTokens = _context.UserDevices.Where(x => model.UserIds.Contains(x.UserId)).Select(x => x.Token).Distinct().ToList();

            if (!deviceTokens.Any())
            {
                return;
            };
            await SendPushNotification(deviceTokens, new NotificationModel
            {
                Body = model.Message,
                IsMobile = true,
                Title = model.Title
            });
        }

        /// <summary>
        /// TESTS THE PUSH NOTIFICATION
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;ResponseModel&gt;.</returns>
        public async Task<ResponseModel> TestPushNotification(TestPushNotificationDTO model)
        {
            var deviceToken = new List<string> { model.Token };

            var response = await SendPushNotification(deviceToken, new NotificationModel
            {
                Body = model.Message,
                IsMobile = true,
                Title = model.Title
            });

            if (response.IsSuccess)
            {
                response.IsSuccess = true;
                response.Message = ResponseMessage.NotificationSent;
                return response;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = ResponseMessage.NotificationNotSent;
                return response;
            }
        }

        /// <summary>
        /// SENDS THE PUSH NOTIFICATION
        /// </summary>
        /// <param name="deviceTokens">The device tokens.</param>
        /// <param name="notificationModel">The notification model.</param>
        /// <returns>Task&lt;ResponseModel&gt;.</returns>
        private async Task<ResponseModel> SendPushNotification(List<string> deviceTokens, NotificationModel notificationModel)
        {
            FcmResponse fcmSendResponse = null;
            ResponseModel response = new ResponseModel();

            if (notificationModel.IsMobile)
            {
                FcmSettings settings = new FcmSettings()
                {
                    SenderId = _fmcNotifSetting.SenderId,
                    ServerKey = _fmcNotifSetting.ServerKey
                };

                HttpClient httpClient = new HttpClient();

                string authorizationKey = string.Format("keyy={0}", settings.ServerKey);
                //string deviceToken = notificationModel.DeviceId;

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
                httpClient.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                DataPayload dataPayload = new DataPayload();

                dataPayload.Title = notificationModel.Title;
                dataPayload.Body = notificationModel.Body;

                GoogleNotification notification = new GoogleNotification();
                notification.Data = dataPayload;
                notification.Notification = dataPayload;

                var fcm = new FcmSender(settings, httpClient);

                foreach (var deviceToken in deviceTokens)
                {
                    fcmSendResponse = await fcm.SendAsync(deviceToken, notification);
                }
            }

            if (fcmSendResponse.IsSuccess())
            {
                response.IsSuccess = true;
                response.Message = ResponseMessage.NotificationSent;
                _logger.Info("Sent Push Notification.. at Execution:SendPushNotification");
                return response;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = fcmSendResponse.Results[0].Error;
                _logger.Info("Failed to Send Push Notification.. at Execution:SendPushNotification");
                return response;
            }

        }
    }
}
