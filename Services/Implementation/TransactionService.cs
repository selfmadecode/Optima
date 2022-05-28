using log4net;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.TransactionDTO;
using Optima.Models.Entities;
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

            var account = _context.WalletBalance.Include(x => x.User).FirstOrDefault(x => x.UserId == userId);

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

        public async Task<BaseResponse<bool>> Withdraw(WithdrawDTO model, Guid userId)
        {
            var result = new BaseResponse<bool>();

            // Validate that the user selected account exist
            var userBankAccountExist = await ValidateUserBankAccountExist(model, userId);

            if(!userBankAccountExist.Errors.Any())
            {
                foreach (var error in userBankAccountExist.Errors)
                {
                    result.Errors.Add(error);
                }

                return result;
            }

            // Validate that user has an account
            var userAccountBalanceExist = await ValidateUserAccountBalanceExist(userId);

            if (userAccountBalanceExist.Data == null)
            {
                foreach (var error in userAccountBalanceExist.Errors)
                {
                    result.Errors.Add(error);
                }

                return result;
            }

            var withdrawRequest = await Withdraw(userAccountBalanceExist.Data, model.Amount, userBankAccountExist.Data, userId);

            if (withdrawRequest.Errors.Any())
            {
                foreach (var error in withdrawRequest.Errors)
                {
                    result.Errors.Add(error);
                }

                return result;
            }


            result.ResponseMessage = ResponseMessage.DebitRequestSuccess;
            result.Data = true;
            return result;
        }

        private async Task<BaseResponse<Guid>> ValidateUserBankAccountExist(WithdrawDTO model, Guid userId)
        {
            var result = new BaseResponse<Guid>();

            var account = _context.BankAccounts.FirstOrDefault(x => x.Id == model.UserSelectedBankAccountId && x.UserId == userId);

            if (account == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage600;
                result.Errors.Add(ResponseMessage.ErrorMessage600);
                _logger.Info("....User account not found - Point: Withdraw");
                return result;
            };

            result.Data = account.Id;
            return result;
        }
        
        private async Task<BaseResponse<WalletBalance>> ValidateUserAccountBalanceExist(Guid userId)
        {
            var result = new BaseResponse<WalletBalance>();

            var userAccountBalance = _context.WalletBalance.FirstOrDefault(x => x.UserId == userId);

            if (userAccountBalance == null)
            {
                result.ResponseMessage = ResponseMessage.ErrorMessage601;
                result.Errors.Add(ResponseMessage.ErrorMessage601);
                result.Data = null;
                _logger.Info("....User account balance not found - Point: Withdraw");
                return result;
            };

            result.Data = userAccountBalance;
            return result;
        }

        private async Task<BaseResponse<bool>> Withdraw(WalletBalance model, decimal amount, Guid BankAccountId, Guid UserId)
        {
            var result = new BaseResponse<bool>();

            if (amount < 500)
            {
                result.Errors.Add("");
                result.Data = false;
                return result;
            }

            if (amount > model.Balance)
            {
                result.Errors.Add("");
                result.Data = false;
                return result;
            }

            var debit = new CreditDebit
            {
                Amount = amount,
                CreatedOn = DateTime.UtcNow,
                TransactionStatus = Models.Enums.TransactionStatus.Pending,
                TransactionType = Models.Enums.TransactionType.Debit,
                AccountBalanceId = model.Id,
                BankAccountId = BankAccountId,
                CreatedBy = UserId                
            };

            _context.CreditDebit.Add(debit);
            await _context.SaveChangesAsync();

            result.Data = true;
            return result;
        }

    }
}
