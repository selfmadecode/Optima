using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.UserDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardTransactionDTOs
{
    public class CardTransactionDTO
    {
        public Guid Id { get; set; }
        public string TransactionRefId { get; set; }
        public decimal TotalExpectedAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public TransactionStatus Status { get; set; }        
        public DateTime CreatedOn { get; set; }
        public CardDetailsDTO CardDetails { get; set; }
        public CustomerDetailsDTO CustomerDetails { get; set; }
        public ActionedByDTO ActionByUser { get; set; }
        public List<CardSoldDTO> CardSold { get; set; }
        public List<CardTransactionImagesDTO> CardTransactionImages { get; set; }        
    }

    public class CardDetailsDTO
    {
        public string Country { get; set; }
        public string Type { get; set; } // E-CODE
        public string CardName { get; set; }

    }

    public class CustomerDetailsDTO
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ActionedByDTO
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
    }

    public class CardSoldDTO
    {
        public decimal Denomination { get; set; }
        public string Code { get; set; }
        public decimal Amount { get; set; }
    }
}
