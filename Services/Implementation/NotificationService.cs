using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.NotificationDTO;
using Optima.Models.Entities;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILog _logger;

        public NotificationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _logger = LogManager.GetLogger(typeof(NotificationService));

        }

        /// <summary>
        /// CREATE IN-APP NOTIFICATION FOR AN ADMIN
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<BaseResponse<bool>> CreateNotificationForAdmin(CreateAdminNotificationDTO model)
        {
            var result = new BaseResponse<bool>();

            try
            {
                var notification = new Notification
                {
                    Message = model.Message,
                    IsAdminNotification = true,
                    IsRead = false,
                    NotificationType = model.Type
                };

                var status = _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                result.Data = true;
                result.ResponseMessage = ResponseMessage.NotificationAdded;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.StackTrace, ex);
                result.Data = false;
                return result;
            }
        }

        /// <summary>
        /// CREATE IN-APP NOTIFICATION FOR A USER, VALIDATES IF THE USER EXIST
        /// </summary>
        /// <param name="model"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<BaseResponse<bool>> CreateNotificationForUser(CreateNotificationDTO model, Guid UserId)
        {
            var result = new BaseResponse<bool>();
            try
            {
                var user = await _userManager.FindByIdAsync(UserId.ToString());

                if (user == null)
                {
                    result.ResponseMessage = ResponseMessage.ErrorMessage000;
                    result.Errors.Add(ResponseMessage.ErrorMessage000);
                    return result;
                };

                var notification = new Notification
                {
                    Message = model.Message,
                    IsAdminNotification = false,
                    IsRead = false,
                    UserId = UserId,
                    NotificationType = model.Type
                };

                var status = _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                result.Data = true;
                result.ResponseMessage = ResponseMessage.NotificationAdded;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.StackTrace, ex);
                result.Data = false;
                return result;
            }
        }        

        /// <summary>
        /// RETURNS ALL UNREAD NOTIFICATION FOR AN ADMIN
        /// </summary>
        /// <returns></returns>
        public async Task<BaseResponse<List<GetNotificationDTO>>> GetAdminNotification()
        {
            var result = new BaseResponse<List<GetNotificationDTO>>();

            var data = _context.Notifications.Where(x => x.IsAdminNotification == true && x.IsRead == false)
                .OrderBy(x => x.CreatedOn);

            result.Data = await data.Select(x => new GetNotificationDTO
            {
                DateCreated = x.CreatedOn,
                IsRead = x.IsRead,
                Message = x.Message,
                NotificationId = x.Id,
                NotificationType = NotificationHelper.GetNotificationType(x.NotificationType)
            }).ToListAsync();

            return result;
        }

        public async Task<BaseResponse<int>> GetAdminUnreadNotificationCount()
        {
            var result = new BaseResponse<int>();
            var count = await _context.Notifications.Where(x => x.IsAdminNotification && x.IsRead == false).CountAsync();

            result.Data = count;
            return result;
        }

        /// <summary>
        /// RETURNS ALL UNREAD NOTIFICATION FOR A USER
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<BaseResponse<List<GetNotificationDTO>>> GetUserUnReadNotification(Guid userId)
        {
            var result = new BaseResponse<List<GetNotificationDTO>>();

            var data = _context.Notifications.Where(x => x.UserId == userId && x.IsRead == false)
                .OrderBy(x => x.CreatedOn);

            result.Data = await data.Select(x => new GetNotificationDTO
            {
                DateCreated = x.CreatedOn,
                IsRead = x.IsRead,
                Message = x.Message,
                NotificationId = x.Id,
                NotificationType = NotificationHelper.GetNotificationType(x.NotificationType)
            }).ToListAsync();

            return result;
        }
        /// <summary>
        /// RETURNS UNREAD NOTIFICATION COUNT FOR A USER
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns>int</returns>
        public async Task<BaseResponse<int>> GetUserUnreadNotificationCount(Guid UserId)
        {
            var result = new BaseResponse<int>();

            result.Data = GetUnReadNotification(UserId).Result.Count();
            return result;
        }

        private async Task<List<Notification>> GetUnReadNotification(Guid userId)
            => await _context.Notifications.Where(x => x.UserId == userId && x.IsRead == false).OrderBy(x => x.CreatedOn).ToListAsync();


        /// <summary>
        /// MARKS A NOTIFICATION AS READ
        /// </summary>
        /// <param name="NotificationId"></param>
        /// <returns></returns>
        public async Task<BaseResponse<Guid>> ReadNotification(Guid NotificationId)
        {
            var result = new BaseResponse<Guid>();

            var notification = _context.Notifications.FirstOrDefault(x => x.Id == NotificationId);

            if (notification == null)
            {
                result.ResponseMessage = ResponseMessage.NotificationNotFound;
                result.Errors.Add(ResponseMessage.NotificationNotFound);
                return result;
            }

            notification.IsRead = true;
            notification.ModifiedOn = DateTime.UtcNow;

            _context.Update(notification);
            await _context.SaveChangesAsync();

            result.Data = notification.Id;
            result.ResponseMessage = ResponseMessage.NotificationUpdate;
            return result;
        }
    }
}
