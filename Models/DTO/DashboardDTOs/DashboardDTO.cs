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
}
