using Optima.Context;
using Optima.Models.DTO.ReceiptDTO;
using Optima.Models.Entities;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class ReceiptService : IReceiptService
    {
        private readonly ApplicationDbContext _context;
        public ReceiptService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<BaseResponse<bool>> CreateReceipt(CreateReceiptDTO model)
        {
            throw new NotImplementedException();
        }
    }
}
