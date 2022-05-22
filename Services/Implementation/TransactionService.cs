using log4net;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.TransactionDTO;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILog _logger;


        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
            _logger = LogManager.GetLogger(typeof(TransactionService));
        }
        public async Task<BaseResponse<BalanceInquiryDTO>> GetUserAccountBalance(Guid userId)
        {
            var result = new BaseResponse<BalanceInquiryDTO>();

            var account = _context.AccountBalance.Include(x => x.User).FirstOrDefault(x => x.UserId == userId);

            _logger.Info("Retrieving user account...- ExecutionPoint: GetUserAccountBalance");

            if (account == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage600;
                result.Errors.Add(ResponseMessage.ErrorMessage600);
                _logger.Info("....User account not found - Point: GetUserAccountBalance");
                return result;
            };

            var data = new BalanceInquiryDTO
            {
                Balance = account.Balance,
                FullName = account.User.FullName,
            };

            result.ResponseMessage = ResponseMessage.SuccessMessage000;
            result.Data = data;
            return result;
        }
    }
}
