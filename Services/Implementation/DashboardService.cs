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
            
            var walletBalance = GetAllWallet().Result.Select(x => x.Balance).Sum();

            // GET ALL PENDING CARD SALE
            var pendingTransactionCount = await GetAllCardTransaction()
                .Where(x => x.TransactionStatus == TransactionStatus.Pending).CountAsync();

            // RETURN USERS COUNT
            var usersCount = await AllUsers().Where(x => x.UserType == UserTypes.USER)
                .CountAsync();

            // RETURN 10 ADMINS
            var usersDTO = await AllUsers().Where(x => x.UserType == UserTypes.ADMIN).Select(x => (UserDTO)x).Take(8).ToListAsync();

            // RETURNS LAST 15 PENDING TRANSACTION
            var cardTransactionDTOs = await GetAllCardTransaction().Take(15)
                .Select(x => new AllTransactionDTO { 
                    CardName = x.CardTypeDenomination.CardType.Card.Name,
                    CreatedOn = x.CreatedOn,
                    Id = x.Id,
                    Status = x.TransactionStatus,
                    TotalAmount = x.TotalExpectedAmount,
                    TransactionRefId = x.TransactionRef,
                    UserName = x.ApplicationUser.FullName
                }).ToListAsync();

            var data = new DashboardDTO
            {
                AvailableToPayOut = walletBalance,
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
            _context.CardTransactions.Where(x => x.TransactionStatus == TransactionStatus.Pending)
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
                    var unknownResponse = cardTransaction.Where(x => x.ActionedByDateTime.Value.Date == DateTime.UtcNow.Date).AsQueryable();
                    dashboarFilterDTO.Revenue = await unknownResponse.SumAsync(x => x.AmountPaid);
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
                    var defaultResponse = cardTransaction.Where(x => x.ActionedByDateTime.Value.Date == DateTime.UtcNow.Date).AsQueryable();
                    dashboarFilterDTO.Revenue = await defaultResponse.SumAsync(x => x.AmountPaid);
                    break;
            }

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
    }
}
