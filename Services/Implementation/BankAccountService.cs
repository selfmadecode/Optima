using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.BankAccountDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class BankAccountService : IBankAccountService
    {
        private readonly ApplicationDbContext _context;
        public BankAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// CREATE BANK ACCOUNT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;bool&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> CreateBankAccount(CreateBankAccountDTO model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            var getAllBanks = await _context.BankAccounts.Where(x => x.UserId == UserId).ToListAsync();

            if (getAllBanks.Count == 2)
            {
                response.Data = false;
                response.Errors = new List<string> { "You can only have a minimun of 2 Bank Account(s)." };
                response.Status = RequestExecution.Failed;
                response.ResponseMessage = "You can only have a minimun of 2 Bank Account(s).";
                return response;
            }

            var checkBankInfo = await _context.BankAccounts
                .Where(x => x.UserId == UserId && x.AccountNumber.Replace(" ", "") == model.AccountNumber.Replace(" ", "")).FirstOrDefaultAsync();

            if (!(checkBankInfo is null))
            {
                response.Data = false;
                var message = $"AccountNumber: {checkBankInfo.AccountNumber} with AccountName: {checkBankInfo.BankName} already Exists";
                response.ResponseMessage = message;
                response.Errors = new List<string> { message };
                response.Status = RequestExecution.Failed;
                return response;
            }

            var newBankAccount = new BankAccount
            {
                AccountName = model.AccountName,
                BankName = model.BankName,
                AccountNumber = model.AccountNumber,
                UserId = UserId,
                CreatedBy = UserId,

            };
           
            _context.BankAccounts.Add(newBankAccount);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.ResponseMessage = $"Successfully Created your Bank Account(s).";
            return response;

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
                return new BaseResponse<bool>
                {
                    Data = false,
                    Errors = new List<string> { "Bank account doesn't exists" },
                    Status = RequestExecution.Failed,
                    ResponseMessage = "Bank account doesn't exists"
                };
            }

            _context.BankAccounts.Remove(bankAccount);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Data = true,
                Status = RequestExecution.Successful,
                ResponseMessage = "Successfully deleted your bank account"
            };

        }

        /// <summary>
        /// GET A USER BANK ACCOUNT
        /// </summary>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;BankAccountDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<BankAccountDTO>>> GetUserBankAccounts(Guid UserId)
        {
            var response = new BaseResponse<List<BankAccountDTO>>();

            var user = GetUserById(UserId);
            if (user is null)
            {
                response.Data = null;
                response.Errors.Add(ResponseMessage.ErrorMessage000);
                response.Status = RequestExecution.Failed;
                response.ResponseMessage = ResponseMessage.ErrorMessage000;
                return response;
            }

            var bankAccounts = await _context.BankAccounts.Where(x => x.UserId == UserId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync();

            var bankAccountDTOs = bankAccounts.Select(x => (BankAccountDTO)x).ToList();

            return new BaseResponse<List<BankAccountDTO>>
            {
                Data = bankAccountDTOs,
                TotalCount = bankAccountDTOs.Count,
                ResponseMessage = $"Found {bankAccounts.Count} Bank Account(s)."
            };

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
                return new BaseResponse<BankAccountDTO>
                {
                    Data = null,
                    Errors = new List<string> { "Bank account doesn't exists" },
                    Status = RequestExecution.Failed,
                    ResponseMessage = "Bank account doesn't exists"
                };
            }

            BankAccountDTO bankAccountDTO = bankAccount;

            return new BaseResponse<BankAccountDTO>
            {
                Data = bankAccountDTO,
                ResponseMessage = "Success"
            };
        }

        /// <summary>
        /// UPDATE BANK ACCOUNT
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;BankAccountDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<bool>> UpdateBankAccount(UpdateBankAccountDTO model, Guid UserId)
        {
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(x => x.UserId == UserId && x.Id == model.Id);

            if (bankAccount is null)
            {
                return new BaseResponse<bool>
                {
                    Data = false,
                    Errors = new List<string> { "Bank account doesn't exists" },
                    Status = RequestExecution.Failed,
                    ResponseMessage = "Bank account doesn't exists"
                };
            }

            bankAccount.AccountName = string.IsNullOrWhiteSpace(model.AccountName) ? bankAccount.AccountName : model.AccountName;
            bankAccount.AccountNumber = string.IsNullOrWhiteSpace(model.AccountName) ? bankAccount.AccountNumber : model.AccountNumber;
            bankAccount.BankName = string.IsNullOrWhiteSpace(model.AccountName) ? bankAccount.BankName : model.BankName;

            _context.BankAccounts.Update(bankAccount);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Data = true,
                ResponseMessage = "Bank account updated successfully"
            };

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
