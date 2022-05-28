using Optima.Models.DTO.ReceiptDTO;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface 
{
    public interface IReceiptService
    {
        Task<BaseResponse<bool>> CreateReceipt(CreateReceiptDTO model);
    }
}
