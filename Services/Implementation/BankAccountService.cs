using log4net;
using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.BankAccountDTOs;
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
    public class BankAccountService : BaseService, IBankAccountService
    {
        private readonly ILog _logger;
        private readonly ApplicationDbContext _context;
        public BankAccountService(ApplicationDbContext context)
        {
            _context = context;
            _logger = LogManager.GetLogger(typeof(BankAccountService));
        }

        /// <summary>
        /// CREATE BANK ACCOUNT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateBankAccount(CreateBankAccountDTO model, Guid UserId)
        {
            var geBanksForUser = await _context.BankAccounts.Where(x => x.UserId == UserId).ToListAsync();

            if (geBanksForUser.Count == 2)
            {
                Errors.Add(ResponseMessage.MaxAccountError);
                return new BaseResponse<bool>(ResponseMessage.MaxAccountError, Errors);
            }

            var checkBankInfo = await _context.BankAccounts
                .Where(x => x.UserId == UserId 
                     && x.AccountNumber.Replace(" ", "") == model.AccountNumber.Replace(" ", "")
                     && x.BankName.ToLower().Replace(" ", "") == model.BankName.ToLower().Replace(" ", "")
                     && x.IsActive
                ).FirstOrDefaultAsync();

            if (!(checkBankInfo is null))
            {
                var message = $"ACCOUNT NUMBER: {checkBankInfo.AccountNumber} WITH ACCOUNT NAME: {checkBankInfo.BankName} ALREADY EXISTS";
                Errors.Add( message);
                return new BaseResponse<bool>(message, Errors);
            }

            _context.BankAccounts.Add(CreateAccount(UserId, model));
            await _context.SaveChangesAsync();

            _logger.Info("Created user Bank Account.....");

            return new BaseResponse<bool>(true, ResponseMessage.BankAccountCreated);
        }

        /// <summary>
        /// CREATES THE BANK ACCOUNT
        /// </summary>
        /// <param name="UserId">The UserId</param>
        /// <param name="model">The Model</param>
        /// <returns>BankAccount</returns>
        private BankAccount CreateAccount(Guid UserId, CreateBankAccountDTO model)
        {
            return new BankAccount
            {
                AccountName = model.AccountName,
                BankName = model.BankName,
                AccountNumber = model.AccountNumber,
                UserId = UserId,
                IsActive = true,
                CreatedBy = UserId
            };
        }

        /// <summary>
        /// DELETE BANK ACCOUNT
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> DeleteBankAccount(Guid id, Guid UserId)
        {
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(x => x.UserId == UserId && x.Id == id);

            if (bankAccount is null)
            {
                Errors.Add(ResponseMessage.BankAccountNotFound);
                return new BaseResponse<bool>(ResponseMessage.BankAccountNotFound, Errors);
            }

            bankAccount.IsActive = false;
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>(true, ResponseMessage.BankAccountDeleted);            
        }

        /// <summary>
        /// GET A USER BANK ACCOUNT
        /// </summary>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;BankAccountDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<BankAccountDTO>>> GetUserBankAccounts(Guid UserId)
        {
            var user = await GetUserById(UserId);
            if (user is null)
            {
                Errors.Add(ResponseMessage.ErrorMessage000);
                return new BaseResponse<List<BankAccountDTO>>(ResponseMessage.ErrorMessage000, Errors);
            }

            var bankAccounts = await _context.BankAccounts
                .Where(x => x.UserId == UserId && x.IsActive)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync();

            var bankAccountDTOs = bankAccounts.Select(x => (BankAccountDTO)x).ToList();

            var message = $"FOUND {bankAccounts.Count} BANK ACCOUNT(S).";
            return new BaseResponse<List<BankAccountDTO>>(bankAccountDTOs, message);            
        }

        /// <summary>
        /// GET BANK ACCOUNT
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;BankAccountDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<BankAccountDTO>> GetBankAccount(Guid id, Guid UserId)
        {
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(x => x.UserId == UserId && x.Id == id);

            if (bankAccount is null)
            {
                Errors.Add(ResponseMessage.BankAccountNotFound);
                return new BaseResponse<BankAccountDTO>(ResponseMessage.BankAccountNotFound, Errors);                
            }

            BankAccountDTO bankAccountDTO = bankAccount;

            return new BaseResponse<BankAccountDTO>(bankAccount, ResponseMessage.SuccessMessage000);            
        }

        /// <summary>
        /// UPDATE BANK ACCOUNT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;BankAccountDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateBankAccount(UpdateBankAccountDTO model, Guid UserId)
        {
            var bankAccount = await _context.BankAccounts
                .FirstOrDefaultAsync(x => x.UserId == UserId && x.Id == model.Id);

            if (bankAccount is null)
            {
                Errors.Add(ResponseMessage.BankAccountNotFound);
                return new BaseResponse<bool>(ResponseMessage.BankAccountNotFound, Errors);
            }

            bankAccount.AccountName = model.AccountName;
            bankAccount.AccountNumber = model.AccountNumber;
            bankAccount.BankName = model.BankName;

            _context.BankAccounts.Update(bankAccount);
            await _context.SaveChangesAsync();
            _logger.Info("Successfully Updated a User Bank Account");
            return new BaseResponse<bool>(true, ResponseMessage.BankAccountUpdated);
        }

        /// <summary>
        /// GET USER BY ID
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>IQueryable&lt;TimeSheet&gt;.</returns>
        private async Task<ApplicationUser> GetUserById(Guid id) =>
            await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
    }
}
