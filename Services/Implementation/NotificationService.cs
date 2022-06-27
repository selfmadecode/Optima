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
    public class NotificationService : BaseService, INotificationService
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
            try
            {
                var notification = new Notification
                {
                    Message = model.Message,
                    IsAdminNotification = true,
                    IsRead = false,
                    NotificationType = model.Type,
                    UserId = model.UserId                 
                };

                var status = _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return new BaseResponse<bool>(true, ResponseMessage.NotificationAdded);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.StackTrace, ex);
                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage999, Errors);
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
            try
            {
                var user = await _userManager.FindByIdAsync(UserId.ToString());

                if (user == null)
                {
                    Errors.Add(ResponseMessage.ErrorMessage000);
                    return new BaseResponse<bool>(ResponseMessage.ErrorMessage000, Errors);
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

                return new BaseResponse<bool>(true, ResponseMessage.NotificationAdded);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.StackTrace, ex);
                Errors.Add(ResponseMessage.ErrorMessage999);
                return new BaseResponse<bool>(ResponseMessage.ErrorMessage999, Errors);
            }
        }        

        /// <summary>
        /// RETURNS ALL UNREAD NOTIFICATION FOR AN ADMIN
        /// </summary>
        /// <returns></returns>
        public async Task<BaseResponse<List<GetNotificationDTO>>> GetAdminNotification(Guid UserId)
        {
            var notifications = _context.Notifications.Where(x => x.UserId == UserId && x.IsAdminNotification == true && x.IsRead == false)
                .OrderByDescending(x => x.CreatedOn);

            var data = await notifications.Select(x => new GetNotificationDTO
            {
                DateCreated = x.CreatedOn,
                IsRead = x.IsRead,
                Message = x.Message,
                NotificationId = x.Id,
                NotificationType = NotificationHelper.GetNotificationType(x.NotificationType)
            }).ToListAsync();

            return new BaseResponse<List<GetNotificationDTO>>(data);
        }

        /// <summary>
        /// RETURNS ALL UNREAD NOTIFICATION FOR A USER
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<BaseResponse<List<GetNotificationDTO>>> GetUserUnReadNotification(Guid userId)
        {
            var notifications = _context.Notifications.Where(x => x.UserId == userId && x.IsRead == false)
                .OrderByDescending(x => x.CreatedOn);

            var data = await notifications.Select(x => new GetNotificationDTO
            {
                DateCreated = x.CreatedOn,
                IsRead = x.IsRead,
                Message = x.Message,
                NotificationId = x.Id,
                NotificationType = NotificationHelper.GetNotificationType(x.NotificationType)
            }).ToListAsync();

            return new BaseResponse<List<GetNotificationDTO>>(data);
        }        


        /// <summary>
        /// MARKS A NOTIFICATION AS READ
        /// </summary>
        /// <param name="NotificationId"></param>
        /// <returns></returns>
        public async Task<BaseResponse<Guid>> ReadNotification(Guid NotificationId)
        {
            //var result = new BaseResponse<Guid>();

            var notification = _context.Notifications.FirstOrDefault(x => x.Id == NotificationId);

            if (notification == null)
            {
                Errors.Add(ResponseMessage.NotificationNotFound);
                return new BaseResponse<Guid>(ResponseMessage.NotificationNotFound, Errors);
            }

            notification.IsRead = true;
            notification.ModifiedOn = DateTime.UtcNow;

            _context.Update(notification);
            await _context.SaveChangesAsync();

            return new BaseResponse<Guid>(notification.Id, ResponseMessage.NotificationUpdate);
        }

        public async Task<BaseResponse<int>> GetAdminUnreadNotificationCount()
        {
            var count = await _context.Notifications.Where(x => x.IsAdminNotification && x.IsRead == false).CountAsync();

            return new BaseResponse<int>(count);
        }

        /// <summary>
        /// RETURNS UNREAD NOTIFICATION COUNT FOR A USER
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns>int</returns>
        public async Task<BaseResponse<int>> GetUserUnreadNotificationCount(Guid UserId)
        {
            var data = GetUnReadNotification(UserId).Result.Count();

            return new BaseResponse<int>(data);
        }
        private async Task<List<Notification>> GetUnReadNotification(Guid userId)
            => await _context.Notifications.Where(x => x.UserId == userId && x.IsRead == false).OrderBy(x => x.CreatedOn).ToListAsync();

    }
}
