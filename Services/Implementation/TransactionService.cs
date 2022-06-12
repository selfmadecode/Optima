using AzureRays.Shared.ViewModels;
using log4net;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.TransactionDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using Optima.Utilities.Pagination;
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

        /// <summary>
        /// GET USER ACCOUNT BALANCE
        /// </summary>
        /// <param name="userId">The userId.</param>
        /// <returns>Task&lt;BaseResponse&lt;BalanceInquiryDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<BalanceInquiryDTO>> GetUserAccountBalance(Guid userId)
        {
            var result = new BaseResponse<BalanceInquiryDTO>();

            var account = await _context.WalletBalance.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);

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


        /// <summary>
        /// USER WITHDRAW
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="userId">The userId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> Withdraw(WithdrawDTO model, Guid userId)
        {
            var result = new BaseResponse<bool>();

            // Validate that the user selected account exist
            var userBankAccountExist = await ValidateUserBankAccountExist(model, userId);

            if(userBankAccountExist.Errors.Any())
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


        /// <summary>
        /// GET USER CREDIT-DEBIT TRANSACTIONS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CreditDebitDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<TransactionDTO>>> GetUserCreditDebit(BaseSearchViewModel model, Guid UserId)
        {

            var response = new BaseResponse<PagedList<TransactionDTO>>();

            var query = _context.WalletBalance
                .Where(x => x.UserId == UserId)
                .Include(x => x.User)
                .Include(x => x.CreditDebit).ThenInclude(x => x.BankAccount)
                .AsNoTracking()
                .AsQueryable();


            if (query.Any() is false)
            {
                response.Data = null;
                response.Errors.Add(ResponseMessage.ErrorMessage600);
                response.Status = RequestExecution.Failed;
                response.ResponseMessage = ResponseMessage.ErrorMessage600;
                return response;
            }


            var userCreditDebit = await EntityFilter(query, model).OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);
            var creditDebitDTO = userCreditDebit.Select(x => (CreditDebitDTO)x).ToList();

            var userTransaction = await query.OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);
            var userTransactionDTO = userTransaction.Select(x => (TransactionDTO)x).ToList();


            userTransactionDTO.ForEach(x =>
            {
                x.CreditDebitDTOs = creditDebitDTO.Where(y => y.WalletBalanceId == x.Id).ToList();
            });

            var data = new PagedList<TransactionDTO>(userTransactionDTO, model.PageIndex, model.PageSize, userCreditDebit.TotalItemCount);
            return new BaseResponse<PagedList<TransactionDTO>>
            { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"Found {userTransactionDTO.Count} Transaction(s)" };

        }


        /// <summary>
        /// GET ALL USER CREDIT-DEBIT TRANSACTIONS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CreditDebitDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<TransactionDTO>>> GetAllUserCreditDebit(BaseSearchViewModel model)
        {

            var query = _context.WalletBalance
                .Include(x => x.User)
                .Include(x => x.CreditDebit).ThenInclude(x => x.BankAccount)
                .Include(x => x.CreditDebit).ThenInclude(x => x.ActionedByUser)
                .AsNoTracking()
                .AsQueryable();


            var userCreditDebit = await EntityFilter(query, model).OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);
            var creditDebitDTO = userCreditDebit.Select(x => (CreditDebitDTO)x).ToList();

            var userTransaction = await query.OrderByDescending(x => x.CreatedOn).ToPagedListAsync(model.PageIndex, model.PageSize);
            var userTransactionDTO = userTransaction.Select(x => (TransactionDTO)x).ToList();

        
            userTransactionDTO.ForEach(x =>
            {
                 x.CreditDebitDTOs = creditDebitDTO.Where(y => y.WalletBalanceId == x.Id).ToList();
            });

            var data = new PagedList<TransactionDTO>(userTransactionDTO, model.PageIndex, model.PageSize, userCreditDebit.TotalItemCount);
            return new BaseResponse<PagedList<TransactionDTO>>
            { Data = data, TotalCount = data.TotalItemCount, ResponseMessage = $"Found {userTransactionDTO.Count} Transaction(s)" };

        }


        /// <summary>
        /// VALIDATE USER BANK ACCOUNT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="userId">The userId.</param>
        /// <returns>Task&lt;BaseResponse&lt;Guid&gt;&gt;.</returns>
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

        /// <summary>
        /// GET USER CREDIT-DEBIT TRANSACTIONS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;WalletBalance&gt;&gt;.</returns>
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


        /// <summary>
        /// WITHDRAW
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="BankAccountId">the bankaccountId</param>
        /// <param name="UserId">the UserId</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        private async Task<BaseResponse<bool>> Withdraw(WalletBalance model, decimal amount, Guid bankAccountId, Guid UserId) 
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
                TransactionStatus = TransactionStatus.Pending,
                TransactionType = TransactionType.Debit,
                WalletBalanceId = model.Id,
                BankAccountId = bankAccountId,
                CreatedBy = UserId                
            };

            _context.CreditDebit.Add(debit);
            await _context.SaveChangesAsync();

            result.Data = true;
            return result;
        }


        /// <summary>
        /// ENTITY FILTER
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="query">The query.</param>
        /// <returns>Task&lt;Card&gt;.</returns>
        private IQueryable<CreditDebit> EntityFilter(IQueryable<WalletBalance> query, BaseSearchViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Keyword) && !string.IsNullOrEmpty(model.Filter))
            {
                IQueryable<CreditDebit> creditBalancequery;
                switch (model.Filter)
                {
                    case "TransactionType":
                        {                          
                            creditBalancequery = query.SelectMany(x => x.CreditDebit).Where(x => x.TransactionType == model.Keyword.Parse<TransactionType>());
                            return creditBalancequery;
                        }

                    case "TransactionStatus":
                        {
                            creditBalancequery = query.SelectMany(x => x.CreditDebit).Where(x => x.TransactionStatus == model.Keyword.Parse<TransactionStatus>());
                            return creditBalancequery;
                        }

                    case "Date":
                        {
                            creditBalancequery = BuildDateQueryFilter(query.SelectMany(x => x.CreditDebit), model.DateFilter);
                            return creditBalancequery;
                        }

                    default:
                        {
                            break;
                        }
                }
            }

            return query.SelectMany(x => x.CreditDebit);
        }


        /// <summary>
        /// BUILDS THE DATE QUERY FILTER
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="DateFilter">The date filter.</param>
        /// <returns>IQueryable&lt;TimeSheet&gt;.</returns>
        private static IQueryable<CreditDebit> BuildDateQueryFilter(IQueryable<CreditDebit> query, DateFilter DateFilter)
        {
            if (DateFilter is null || DateFilter.DateFrom is null || DateFilter.DateTo is null) return query;

            query = query.Where(x => x.CreatedOn.Date >= DateFilter.DateFrom.Value.Date && x.CreatedOn.Date <= DateFilter.DateTo.Value.Date);
            return query;

        }

    }
}
