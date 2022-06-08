using Optima.Models.DTO.CardSaleDTO;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface ICardSaleService
    {
        Task<BaseResponse<bool>> CreateCardSales(SellCardDTO model, Guid UserId);
        Task<BaseResponse<bool>> CreateCardSales(SellCardDTO model, Guid UserId);
    }
}
