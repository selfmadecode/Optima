using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CardTransaction : BaseEntity
    {
        public string TransactionRef { get; set; }

        public TransactionStatus TransactionStatus { get; set; }

        public decimal TotalExpectedAmount { get; set; }
        public decimal AmountPaid { get; set; }

        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public Guid? ActionById { get; set; } // admin that acted on the request
        public ApplicationUser ActionBy { get; set; }
        public DateTime? ActionedByDateTime { get; set; }

        public List<TransactionUploadFiles> TransactionUploadededFiles { get; set; } = new List<TransactionUploadFiles>();
        public List<CardSold> CardSold { get; set; } = new List<CardSold>();

    }    
}
