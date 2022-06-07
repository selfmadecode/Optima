using Optima.Models.DTO.ReceiptDTOs;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface 
{
    public interface IReceiptService
    {
        Task<BaseResponse<bool>> CreateReceipt(CreateReceiptDTO model, Guid UserId);
        Task<BaseResponse<List<ReceiptDTO>>> GetAllReceipt();
        Task<BaseResponse<bool>> UpdateReceipt(UpdateReceiptDTO model);
        Task<BaseResponse<bool>> DeleteReceipt(Guid id);
    }
}
