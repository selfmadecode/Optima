using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.DTO.BankAccountDTO;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Ultilities.Helpers;
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

            var checkBankInfo = await _context.BankAccounts
                .Where(x => x.UserId == UserId && model.Select(x => x.BankName.Replace(" ", "").ToLower()).Contains(x.BankName.ToLower().Replace(" ", ""))).ToListAsync();

            if (checkBankInfo.Any())
            {
                response.Data = false;
                response.ResponseMessage = $"{checkBankInfo.Select(x => x.BankName)} already Exists";
                response.Errors.Add($"{checkBankInfo.Select(x => x.BankName)} already Exists");
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
            response.ResponseMessage = $"Successfully Created {checkBankInfo.Count} Bank Account";
            return response;

        }
    }
}
