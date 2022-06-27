using Optima.Models.DTO.SignalRNotificationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface ISignalRService  
    {
        Task SendCardSaleNotification(CardSaleNotificationDTO model);
    }
}
