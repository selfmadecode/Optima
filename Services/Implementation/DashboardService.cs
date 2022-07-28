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
            // SUM ALL USERS WALLET
            
            var walletBalance = await GetAllWallet();

            // GET ALL PENDING CARD SALE
            List<TransactionStatus> status = new List<TransactionStatus>
            {
                TransactionStatus.Pending
            };
            var pendingTransactionCount = await GetAllCardTransaction(status).CountAsync();
                        
            // RETURN USERS COUNT
            var usersCount = await AllUsers().Where(x => x.UserType == UserTypes.USER)
                .CountAsync();

            // RETURN 8 ADMINS
            var usersDTO = await AllUsers().Where(x => x.UserType == UserTypes.ADMIN).Select(x => (UserDTO)x).Take(8).ToListAsync();

            List<RecentActivitiesDTO> recentActivities = new List<RecentActivitiesDTO>();

            var recentCardTransactions = GetRecentCardSale();
            var withdrawals = GetRecentWithdrawals();

            recentActivities.AddRange(withdrawals);
            recentActivities.AddRange(recentCardTransactions);

            var data = new DashboardDTO
            {
                AvailableToPayOut = walletBalance,
                PendingTransaction = pendingTransactionCount,
                TotalUserCount = usersCount,
                AdminUserDTOs = usersDTO,
                RecentActivities = recentActivities.OrderBy(x => x.CreatedOn).Take(6).ToList(),
            };

            return new BaseResponse<DashboardDTO> { Data = data, ResponseMessage = ResponseMessage.SuccessMessage000 };                    
        }

        private IQueryable<RecentActivitiesDTO> GetRecentCardSale()
        {
            return _context.CardTransactions
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new RecentActivitiesDTO
                {
                    Id = x.Id,
                    Amount = x.TotalExpectedAmount,
                    TransactionType = "Card Sale",
                    CreatedOn = x.CreatedOn,
                    Status = x.TransactionStatus
                }).Take(6);
        }

        private IQueryable<RecentActivitiesDTO> GetRecentWithdrawals()
        {
            return _context.CreditDebit.Where(x => x.TransactionType == TransactionType.Debit)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new RecentActivitiesDTO
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    TransactionType = "Withdrawal",
                    CreatedOn = x.CreatedOn,
                    Status = x.TransactionStatus
                }).Take(6);
        }

        /// <summary>
        /// GET ALL WALLET BALANCE
        /// </summary>
        /// <param name=""></param>
        /// <returns>Task&lt;List&lt;WalletBalance&gt;&gt;.</returns>
        private async Task<decimal> GetAllWallet() =>
             await _context.WalletBalance.Select(x => x.Balance).SumAsync();

        /// <summary>
        /// GET ALL CARD TRANSACTION
        /// </summary>
        /// <param name=""></param>
        /// <returns>Task&lt;List&lt;CardTransaction&gt;&gt;.</returns>
        private IQueryable<CardTransaction> GetAllCardTransaction(List<TransactionStatus> status) =>
            _context.CardTransactions.Where(x => status.Contains(x.TransactionStatus))
            .OrderByDescending(x => x.CreatedOn)
            .AsQueryable();

        private IQueryable<CardTransaction> GetAllCardTransaction() =>
            _context.CardTransactions
            .OrderByDescending(x => x.CreatedOn);
  

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

            List<TransactionStatus> status = new List<TransactionStatus>
            {
                TransactionStatus.Approved,
                TransactionStatus.PartialApproval
            };

            var cardTransaction = GetAllCardTransaction(status);

            var dateRange = new TimeBoundSearchVm
            {
                TimeRange = range
            };

            var date = DateRangeExtensions.SetDateRange(dateRange);

            var (revenue, percentage) = await CalculateRevenueAndPercentage(cardTransaction, range, dateRange);

            var dashboarFilterDTO = new DashboardFilterDTO
            {
                Revenue = revenue,
                Percentage = percentage,
            };

            return new BaseResponse<DashboardFilterDTO> { Data = dashboarFilterDTO, ResponseMessage = ResponseMessage.SuccessMessage000 };
        }

        public async Task<BaseResponse<DashboardGraphDTO>> Dashboard(int year)
        {
            var cardSales = await _context.CardTransactions.Where(x => x.CreatedOn.Year == year).ToListAsync();

            //First Implementation --> Returns a List of <List<DashboardGraphDTO>>
            /* var groupedCardSales = cardSales.GroupBy(x => x.CreatedOn.Month).OrderBy(x => x.Key);
             var data = groupedCardSales.Select(x => new DashboardGraphDTO
             {
                 Month = GetFullName(x.Key),
                 CardSalesCount = x.Count(),
             }).ToList();*/

            var data2 = new DashboardGraphDTO
            {
                January = cardSales.Where(x => x.CreatedOn.Month == 1).Count(),
                February = cardSales.Where(x => x.CreatedOn.Month == 2).Count(),
                March = cardSales.Where(x => x.CreatedOn.Month == 3).Count(),
                April = cardSales.Where(x => x.CreatedOn.Month == 4).Count(),
                May = cardSales.Where(x => x.CreatedOn.Month == 5).Count(),
                June = cardSales.Where(x => x.CreatedOn.Month == 6).Count(),
                July = cardSales.Where(x => x.CreatedOn.Month == 7).Count(),
                August = cardSales.Where(x => x.CreatedOn.Month == 8).Count(),
                September = cardSales.Where(x => x.CreatedOn.Month == 9).Count(),
                October = cardSales.Where(x => x.CreatedOn.Month == 10).Count(),
                November = cardSales.Where(x => x.CreatedOn.Month == 10).Count(),
                December = cardSales.Where(x => x.CreatedOn.Month == 12).Count(),
            };      
           
            return new BaseResponse<DashboardGraphDTO>(data2, ResponseMessage.SuccessMessage000);                     
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <returns>System.String.</returns>
        private string GetFullName(int month)
        {
            DateTime date = new DateTime(DateTime.Now.Year, month, 1);

            return date.ToString("MMMM");
        }

        /// <summary>
        /// CALCULATES THE PERCENTAGE
        /// </summary>
        /// <param name="currentAmout"></param>
        /// <param name="previousAmount"></param>
        /// <returns></returns>
        private decimal CalculatePercentage(decimal currentAmount, decimal previousAmount)
        {
            if (previousAmount != 0)
            {
                var percentage = (currentAmount / previousAmount) * 100;
                return Math.Round(percentage);
            }

            return 0;
           
        }

        /// <summary>
        /// CALCULATES THE REVENUE AND PERCENTAGE
        /// </summary>
        /// <param name="cardTransactions"></param>
        /// <param name="dateRangeQueryType"></param>
        /// <param name="date"></param>
        /// <returns>System.Tuple&lt;(decimal, decimal)&gt;</returns>
        private async Task<(decimal, decimal)> CalculateRevenueAndPercentage(IQueryable<CardTransaction> cardTransactions, DateRangeQueryType dateRangeQueryType, TimeBoundSearchVm date)
        {
            decimal currentRevenue = 0, previousRevenue = 0, percentage = 0;

            switch (dateRangeQueryType)
            {
                case DateRangeQueryType.UNKNOWN:
                    {
                       
                    }
                    break;
                case DateRangeQueryType.Today:
                    {
                        currentRevenue = await cardTransactions
                          .Where(x => x.ActionedByDateTime.Value.Date == DateTime.UtcNow.Date)
                          .SumAsync(x => x.AmountPaid);

                        previousRevenue = await cardTransactions
                          .Where(x => x.ActionedByDateTime.Value.Date == DateTime.UtcNow.Date.AddDays(-1))
                          .SumAsync(x => x.AmountPaid);

                        percentage = CalculatePercentage(currentRevenue, previousRevenue);
                    }
                    break;
                case DateRangeQueryType.Week:
                    {
                        currentRevenue = await cardTransactions
                            .Where(x => x.ActionedByDateTime.Value.Date > date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                            .SumAsync(x => x.AmountPaid);

                        previousRevenue = await cardTransactions
                          .Where(x => x.ActionedByDateTime.Value.Date <= date.FromDate.Value)
                          .SumAsync(x => x.AmountPaid);

                        percentage = CalculatePercentage(currentRevenue, previousRevenue);
                    }
                    break;
                case DateRangeQueryType.Month:
                    {
                        currentRevenue = await cardTransactions
                         .Where(x => x.ActionedByDateTime.Value.Date > date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                         .SumAsync(x => x.AmountPaid);

                        previousRevenue = await cardTransactions
                          .Where(x => x.ActionedByDateTime.Value.Date <= date.FromDate.Value)
                          .SumAsync(x => x.AmountPaid);

                        percentage = CalculatePercentage(currentRevenue, previousRevenue);
                    }
                    break;
                case DateRangeQueryType.Quarter:
                    {
                        currentRevenue = await cardTransactions
                        .Where(x => x.ActionedByDateTime.Value.Date > date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                         .SumAsync(x => x.AmountPaid);

                        previousRevenue = await cardTransactions
                          .Where(x => x.ActionedByDateTime.Value.Date <= date.FromDate.Value)
                          .SumAsync(x => x.AmountPaid);

                        percentage = CalculatePercentage(currentRevenue, previousRevenue);
                    }
                    break;
                case DateRangeQueryType.BiAnnual:
                    {
                        currentRevenue = await cardTransactions
                        .Where(x => x.ActionedByDateTime.Value.Date > date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                         .SumAsync(x => x.AmountPaid);

                        previousRevenue = await cardTransactions
                          .Where(x => x.ActionedByDateTime.Value.Date <= date.FromDate.Value)
                          .SumAsync(x => x.AmountPaid);

                        percentage = CalculatePercentage(currentRevenue, previousRevenue);
                    }
                    break;
                case DateRangeQueryType.Annual:
                    {
                        currentRevenue = await cardTransactions
                         .Where(x => x.ActionedByDateTime.Value.Date > date.FromDate.Value.Date && x.ActionedByDateTime.Value.Date <= DateTime.UtcNow.Date)
                         .SumAsync(x => x.AmountPaid);

                        previousRevenue = await cardTransactions
                          .Where(x => x.ActionedByDateTime.Value.Date <= date.FromDate.Value)
                          .SumAsync(x => x.AmountPaid);

                        percentage = CalculatePercentage(currentRevenue, previousRevenue);
                    }
                    break;
                default:
                    break;                 
            }

            return (currentRevenue, percentage);

        }
    }
}
