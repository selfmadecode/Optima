using Microsoft.EntityFrameworkCore;
using Optima.Context;
using Optima.Models.Constant;
using Optima.Models.DTO.CardTransactionDTOs;
using Optima.Models.DTO.DashboardDTOs;
using Optima.Models.DTO.UserDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Services.Interface;
using Optima.Utilities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// DASHBOARD
        /// </summary>
        /// <param name=""></param>
        /// <returns>Task&lt;BaseResponse&lt;DashboardDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<DashboardDTO>> Dashboard()
        {
            var walletBalance = GetAllWallet().Result.Select(x => x.Balance).Sum();
            var pendingTransactionCount = await GetAllCardTransaction().Where(x => x.TransactionStatus == TransactionStatus.Pending).CountAsync();
            var usersCount = await AllUsers().CountAsync();
            var usersDTO = await AllUsers().Where(x => x.UserType == UserTypes.ADMIN).Select(x => (UserDTO)x).Take(10).ToListAsync();
            var cardTransactionDTOs = await GetAllCardTransaction().Select(x => (CardTransactionDTO)x).Take(10).ToListAsync();

            var data = new DashboardDTO
            {
                Spendings = walletBalance,
                PendingTransaction = pendingTransactionCount,
                TotalUserCount = usersCount,
                AdminUserDTOs = usersDTO,
                CardTransactionDTOs = cardTransactionDTOs,
            };

            return new BaseResponse<DashboardDTO> { Data = data, ResponseMessage = ResponseMessage.SuccessMessage000 };                    
        }

        /// <summary>
        /// GET ALL WALLET BALANCE
        /// </summary>
        /// <param name=""></param>
        /// <returns>Task&lt;List&lt;WalletBalance&gt;&gt;.</returns>
        private async Task<List<WalletBalance>> GetAllWallet() =>
             await _context.WalletBalance.ToListAsync();

        /// <summary>
        /// GET ALL CARD TRANSACTION
        /// </summary>
        /// <param name=""></param>
        /// <returns>Task&lt;List&lt;CardTransaction&gt;&gt;.</returns>
        private IQueryable<CardTransaction> GetAllCardTransaction() =>
            _context.CardTransactions
            .OrderByDescending(x => x.CreatedOn)
            .AsQueryable();

        /// <summary>
        /// ALL USERS
        /// </summary>
        /// <param name=""></param>
        /// <returns>Task&lt;List&lt;CardTypeDenomination&gt;&gt;.</returns>
        private IQueryable<ApplicationUser> AllUsers() =>
             _context.Users.OrderByDescending(x => x.CreationTime).AsQueryable();


        /// <summary>
        /// DASHBOARD
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns>Task&lt;BaseResponse&lt;DashboardFilterDTO&gt;&gt;.</returns>
        public async Task<BaseResponse<DashboardFilterDTO>> Dashboard(DateRangeQueryType range)
        {
            var data = new BaseResponse<DashboardFilterDTO>();
            var dashboarFilterDTO = new DashboardFilterDTO();
            
            var cardTransaction = GetAllCardTransaction()
                .Where(x => x.TransactionStatus == TransactionStatus.Approved || x.TransactionStatus == TransactionStatus.PartialApproval)
                .AsQueryable();

            var dateRange = new TimeBoundSearchVm
            {
                TimeRange = range
            };

            var date = DateRangeExtensions.SetDateRange(dateRange);

            switch (range)
            {
                case DateRangeQueryType.UNKNOWN:
                    break;
                case DateRangeQueryType.Today:
                    {
                        var response = cardTransaction.Where(x => x.ActionedByDateTime.Value.Date == DateTime.UtcNow.Date).AsQueryable();
                        dashboarFilterDTO.Revenue = await response.SumAsync(x => x.AmountPaid);
                    }
                    break;
                case DateRangeQueryType.Week:
                    {
                        var response = cardTransaction.Where(x => x.ActionedByDateTime.Value.Date >= date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                            .AsQueryable();
                        dashboarFilterDTO.Revenue = await response.SumAsync(x => x.AmountPaid);
                    }
                    break;
                case DateRangeQueryType.Month:
                    {
                        var response = cardTransaction.Where(x => x.ActionedByDateTime.Value.Date >= date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                            .AsQueryable();
                        dashboarFilterDTO.Revenue = await response.SumAsync(x => x.AmountPaid);
                    }
                    break;
                case DateRangeQueryType.Quarter:
                    {
                        var response = cardTransaction.Where(x => x.ActionedByDateTime.Value.Date >= date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                            .AsQueryable();
                        dashboarFilterDTO.Revenue = await response.SumAsync(x => x.AmountPaid);
                    }
                    break;
                case DateRangeQueryType.BiAnnual:
                    {
                        var response = cardTransaction.Where(x => x.ActionedByDateTime.Value.Date >= date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                            .AsQueryable();
                        dashboarFilterDTO.Revenue = await response.SumAsync(x => x.AmountPaid);
                    }
                    break;
                case DateRangeQueryType.Annual:
                    {
                        var response = cardTransaction.Where(x => x.ActionedByDateTime.Value.Date >= date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                            .AsQueryable();
                        dashboarFilterDTO.Revenue = await response.SumAsync(x => x.AmountPaid);
                    }
                    break;
                default:
                    break;
            }

            return new BaseResponse<DashboardFilterDTO> { Data = dashboarFilterDTO, ResponseMessage = ResponseMessage.SuccessMessage000 };
        }
    }
}
