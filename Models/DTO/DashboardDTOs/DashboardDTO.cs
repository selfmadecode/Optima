using Optima.Models.DTO.CardTransactionDTOs;
using Optima.Models.DTO.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.DashboardDTOs
{
    public class DashboardDTO
    {
        public decimal Spendings { get; set; }
        public int PendingTransaction { get; set; }
        public int TotalUserCount { get; set; }
        public List<UserDTO> AdminUserDTOs { get; set; }
        public List<AllTransactionDTO> CardTransactionDTOs { get; set; } 
    }

    public class DashboardGraphDTO
    {

        //First Implementation
       /* public string Month { get; set; }
        public int CardSalesCount { get; set; }*/


        //Second Implementation.
        public int January { get; set; }
        public int February { get; set; }
        public int March { get; set; }
        public int April { get; set; }
        public int May { get; set; }
        public int June { get; set; }
        public int July { get; set; }
        public int August { get; set; }
        public int September { get; set; }
        public int October { get; set; }
        public int November { get; set; }
        public int December { get; set; }

    }
}
