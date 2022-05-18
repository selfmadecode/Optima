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
                response.ResponseMessage = $"You have already created {checkBankInfo.Select(x => string.Join(",", x.AccountNumber, x.BankName))} bank account(s).";
                response.Errors.Add($"You have already created {checkBankInfo.Select(x => string.Join(",", x.AccountNumber, x.BankName))} bank account(s).");
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
            response.ResponseMessage = $"Successfully Created {checkBankInfo.Count} Bank Account(s).";
            return response;

        }

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
