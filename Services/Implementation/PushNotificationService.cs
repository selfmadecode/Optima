using CorePush.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Optima.Context;
using Optima.Models.DTO.NotificationDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
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
    public class PushNotificationService : IPushNotificationService
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
        

        public PushNotificationService(ApplicationDbContext context, IOptions<FcmNotification> fmcNotifSetting, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _fmcNotifSetting = fmcNotifSetting.Value;
            _userManager = userManager;
           
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
                return new BaseResponse<string>
                {
                    ResponseMessage = "The Token doesn't exists for this User",
                    Errors = new List<string> { "The Token doesn't exists for this user"},
                    Status = RequestExecution.Failed,
                };

            _context.UserDevices.RemoveRange(tokens);
            await _context.SaveChangesAsync();

            /* foreach (var token in tokens)
             {
                 _context.UserDevices.Remove(token);
                 await _unitOfWork.SaveChangesAsync();
             }*/

            return new BaseResponse<string> { Data = "Success", ResponseMessage = $"Successfully Deleted the User device token", Status = RequestExecution.Successful };
        }

        /// <summary>
        /// REGISTERS THE DEVICE TOKEN FOR PUSH NOTIFICATION
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;ResultModel&lt;System.String&gt;&gt;.</returns>
        public async Task<BaseResponse<string>> RegisterForPush(DeviceDTO model)
        {
            var result = new BaseResponse<string>();

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());

            if (user == null)
            {
                result.ResponseMessage = "User doesn't exists";
                result.Errors = new List<string> { "User doesn't exists" };
                result.Status = RequestExecution.Failed;
                return result;
            };

           
            var deviceExist = _context.UserDevices.FirstOrDefault(x => x.Token == model.Token);

            if (deviceExist != null)
            {
                deviceExist.UserId = user.Id;
                _context.Update(deviceExist);
                await _context.SaveChangesAsync();

                result.Data = "Sucesss";
                result.ResponseMessage = "Success";
                result.Status = RequestExecution.Successful;
                return result;

            }

            var newDevice = new UserDevice
            {
                Token = model.Token,
                UserId = model.UserId,
                CreatedOn = DateTime.UtcNow,
            };

            _context.UserDevices.Add(newDevice);
            await _context.SaveChangesAsync();

            result.Data = "ADDED SUCCESSFULLY!";
            return result;
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
                response.Message = "Notification sent successfully";
                return response;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Failed to send Notification";
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
                response.Message = "Notification sent successfully";
                return response;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = fcmSendResponse.Results[0].Error;
                return response;
            }

        }
    }
}
