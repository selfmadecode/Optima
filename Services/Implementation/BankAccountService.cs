using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.DTO.BankAccountDTO;
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
        public async Task<BaseResponse<bool>> CreateBankAccount(List<CreateBankAccountDTO> model, Guid UserId)
        {
            var response = new BaseResponse<bool>();

            if (model.Count > 3)
            {
                response.Data = false;
                response.ResponseMessage = "You cannot have more than 3 bank account" ;
                response.Errors.Add($"You cannot have more than 3 bank account");
                response.Status = RequestExecution.Failed;
                return response;
            }

            var checkBankInfo = await _context.BankAccounts
                .Where(x => x.UserId == UserId && model.Select(x => x.AccountNumber.Replace(" ", "")).Contains(x.AccountNumber.Replace(" ", ""))).ToListAsync();

            if (checkBankInfo.Any())
            {
                response.Data = false;
                var message = checkBankInfo.Select(x => string.Join(",", $"{x.AccountNumber}, {x.BankName} already Exists")).ToList();
                response.ResponseMessage = string.Join(", ", message);
                message.ForEach(x => response.Errors.Add(x));
                response.Status = RequestExecution.Failed;
                return response;
            }

            var newBankAccount = model.Select(x => new BankAccount
            {
                AccountName = x.AccountName,
                BankName = x.BankName,
                AccountNumber = x.AccountNumber,
                UserId = UserId,
                CreatedBy = UserId,

            }).ToList();
           
            _context.BankAccounts.AddRange(newBankAccount);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.Status = RequestExecution.Successful;
            response.ResponseMessage = $"Successfully Created {model.Count} Bank Account(s).";
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
        /// GET ALL BANK ACCOUNT
        /// </summary>
        /// <param name="UserId">The UserId.</param>
        /// <returns>Task&lt;BaseResponse&lt;List&lt;BankAccountDTO&gt;&gt;&gt;.</returns>
        public async Task<BaseResponse<List<BankAccountDTO>>> GetAllBankAccount(Guid UserId)
        {
            var bankAccounts = await _context.BankAccounts.Where(x => x.UserId == UserId).ToListAsync();

            if (!bankAccounts.Any())
            {
                return new BaseResponse<List<BankAccountDTO>>
                {
                    Data = null,
                    Errors = new List<string> { "Found no Bank Account" },
                    Status = RequestExecution.Failed,
                    ResponseMessage = "Found no Bank Account"
                };
            }

            var bankAccountDTOs = bankAccounts.Select(x => (BankAccountDTO)x).ToList();

            return new BaseResponse<List<BankAccountDTO>>
            {
                Data = bankAccountDTOs,
                Status = RequestExecution.Successful,
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
            var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(x=> x.UserId == UserId && x.Id == id);

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
                Status = RequestExecution.Successful,
                ResponseMessage = "Bank account updated successfully"
            };

        }
    }
}
