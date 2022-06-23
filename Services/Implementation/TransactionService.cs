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
    public class TransactionService : BaseService, ITransactionService
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
           // var result = new BaseResponse<BalanceInquiryDTO>();

            var account = await _context.WalletBalance.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);

            _logger.Info("Retrieving user account...- ExecutionPoint: GetUserAccountBalance");

            if (account == null)
            {
                _logger.Info("....User account not found - Point: GetUserAccountBalance");
                return new BaseResponse<BalanceInquiryDTO>(ResponseMessage.ErrorMessage600, Errors);
            };

            var data = new BalanceInquiryDTO
            {
                Balance = account.Balance,
                FullName = account.User.FullName,
            };

            return new BaseResponse<BalanceInquiryDTO>(data, ResponseMessage.SuccessMessage000);
        }


        /// <summary>
        /// USER WITHDRAW
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="userId">The userId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> Withdraw(WithdrawDTO model, Guid userId)
        {
            // Validate that the user selected account exist
            var userBankAccountExist = await ValidateUserBankAccountExist(model, userId);

            if(userBankAccountExist.Errors.Any())
            {
                //foreach (var error in userBankAccountExist.Errors)
                //{
                //    Errors.Add(error);
                //}
                return new BaseResponse<bool>(userBankAccountExist.ResponseMessage, userBankAccountExist.Errors);
            }

            // Validate that user has an account
            var userAccountBalanceExist = await ValidateAndReturnUserWallet(userId);

            if (userAccountBalanceExist.Data == null)
            {
                //foreach (var error in userAccountBalanceExist.Errors)
                //{
                //    result.Errors.Add(error);
                //}

                return new BaseResponse<bool>(userAccountBalanceExist.ResponseMessage, userAccountBalanceExist.Errors);
            }

            var withdrawRequest = await Withdraw(userAccountBalanceExist.Data, model.Amount, userBankAccountExist.Data, userId);

            if (withdrawRequest.Errors.Any())
            {
                return new BaseResponse<bool>(withdrawRequest.ResponseMessage, withdrawRequest.Errors);
            }

            return new BaseResponse<bool>(true, ResponseMessage.DebitRequestSuccess);
        }


        /// <summary>
        /// GET USER CREDIT-DEBIT TRANSACTIONS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;PagedList&lt;CreditDebitDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<PagedList<TransactionDTO>>> GetUserCreditDebit(BaseSearchViewModel model, Guid UserId)
        {
            var query = _context.WalletBalance
                .Where(x => x.UserId == UserId)
                .Include(x => x.User)
                .Include(x => x.CreditDebit).ThenInclude(x => x.BankAccount)
                .AsNoTracking()
                .AsQueryable();


            if (query.Any() is false)
            {
                Errors.Add(ResponseMessage.ErrorMessage600);
                return new BaseResponse<PagedList<TransactionDTO>>(ResponseMessage.ErrorMessage600, Errors);
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
        /// UPDATTE DEBIT TRANSACTION STATUS
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="UserId">The UserId</param>
        /// <returns>Task&lt;BaseResponse&lt;UpdateCreditDebitStatus&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateDebitStatus(UpdateDebitStatus model, Guid UserId)
        {
            var creditDebit = await _context.CreditDebit.Where(x => x.Id == model.CreditDebitId).Include(x => x.ActionedByUser).FirstOrDefaultAsync();

            if (creditDebit is null)
            {
                Errors.Add(ResponseMessage.CreditDebitNotFound);
                return new BaseResponse<bool>(ResponseMessage.CreditDebitNotFound, Errors);
            }

            if (creditDebit.TransactionStatus != TransactionStatus.Pending)
            {
                var message = $"Debit Transaction has already been actioned by Optima Admin with Name:" +
                    $" {creditDebit.ActionedByUser.FullName} on {creditDebit.ModifiedOn.Value:ddd, dd MMMM yyyy, hh:mm tt}";
                Errors.Add(message);
                return new BaseResponse<bool>(message, Errors);
            }

            switch (model.CreditDebitStatus)
            {
                case CreditDebitStatus.Approved:
                    {
                        creditDebit.TransactionStatus = TransactionStatus.Approved;
                        creditDebit.ActionedByUserId = UserId;
                        creditDebit.ModifiedOn = DateTime.UtcNow;
                        break;
                    } 
                    
                case CreditDebitStatus.Declined:
                    {
                        var findWallet = await _context.WalletBalance.FirstOrDefaultAsync(x => x.Id == creditDebit.WalletBalanceId);
                        creditDebit.TransactionStatus = TransactionStatus.Declined;
                        findWallet.Balance += creditDebit.Amount;
                        creditDebit.ActionedByUserId = UserId;
                        creditDebit.ModifiedOn = DateTime.UtcNow;
                        break;
                    }
                   
                default:
                    break;
            }

            await _context.SaveChangesAsync();
            return new BaseResponse<bool>(true, ResponseMessage.DebitUpdated);
        }

        /// <summary>
        /// VALIDATE USER BANK ACCOUNT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="userId">The userId.</param>
        /// <returns>Task&lt;BaseResponse&lt;Guid&gt;&gt;.</returns>
        private async Task<BaseResponse<Guid>> ValidateUserBankAccountExist(WithdrawDTO model, Guid userId)
        {
            var account = await _context.BankAccounts.FirstOrDefaultAsync(x => x.Id == model.UserSelectedBankAccountId && x.UserId == userId);

            if (account == null)
            {
                _logger.Info("....User account not found - Point: Withdraw");
                return new BaseResponse<Guid>(ResponseMessage.ErrorMessage600, Errors);
            };
            return new BaseResponse<Guid>(account.Id);
        }

        /// <summary>
        /// GET USER CREDIT-DEBIT TRANSACTIONS
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;WalletBalance&gt;&gt;.</returns>
        private async Task<BaseResponse<WalletBalance>> ValidateAndReturnUserWallet(Guid userId)
        {
            var userAccountBalance = await _context.WalletBalance.FirstOrDefaultAsync(x => x.UserId == userId);

            if (userAccountBalance == null)
            {
                _logger.Info("....User account balance not found - Point: Withdraw");
                return new BaseResponse<WalletBalance>(ResponseMessage.ErrorMessage601, Errors); ;
            };

            return new BaseResponse<WalletBalance>(userAccountBalance);
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
            if (amount < 500)
            {
                Errors.Add(ResponseMessage.MinAmountError);
                return new BaseResponse<bool>(ResponseMessage.MinAmountError, Errors);
            }

            if (amount > model.Balance)
            {
                Errors.Add(ResponseMessage.InsufficientError);
                return new BaseResponse<bool>(ResponseMessage.InsufficientError, Errors);
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

            // GET ACCOUNT TO DEBIT AND DEBIT
            var userAccountBalance = _context.WalletBalance.FirstOrDefault(x => x.Id == model.Id);
            userAccountBalance.Balance -= amount;

            _context.CreditDebit.Add(debit);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true);
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
                switch (model.Filter)
                {
                    case "TransactionType":
                        {
                            return query.SelectMany(x => x.CreditDebit).Where(x => x.TransactionType == model.Keyword.Parse<TransactionType>());   
                        }

                    case "TransactionStatus":
                        {
                            return query.SelectMany(x => x.CreditDebit).Where(x => x.TransactionStatus == model.Keyword.Parse<TransactionStatus>());
                        }

                    case "Date":
                        {
                            return BuildDateQueryFilter(query.SelectMany(x => x.CreditDebit), model.DateFilter);
                        }

                    default:
                        {
                            return query.SelectMany(x => x.CreditDebit).Where(x => x.TransactionType == model.Keyword.Parse<TransactionType>());
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
