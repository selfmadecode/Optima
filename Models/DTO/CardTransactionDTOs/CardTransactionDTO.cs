using Optima.Models.DTO.CardSaleDTO;
using Optima.Models.DTO.UserDTOs;
using Optima.Models.Entities;
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
        public string TransactionStatus { get; set; }        
        public DateTime CreatedOn { get; set; }
        public UserDTO UserDTO { get; set; }
        public List<CardSoldDTO> CardSoldDTOs { get; set; }
        public List<CardTransactionImagesDTO> CardTransactionImagesDTOs { get; set; }



        public static implicit operator CardTransactionDTO(CardTransaction model)
        {
            return model is null ? null
               : new CardTransactionDTO
               {
                   Id = model.Id,
                   TransactionRefId = model.TransactionRef,
                   TotalExpectedAmount = model.TotalExpectedAmount,
                   AmountPaid = model.AmountPaid,
                   TransactionStatus = model.TransactionStatus.GetDescription(),
                   UserDTO = model.ApplicationUser,
                   CardSoldDTOs = model.CardSold.Select(x => (CardSoldDTO)x).ToList(),
                   CardTransactionImagesDTOs = model.TransactionUploadededFiles.Select(x => (CardTransactionImagesDTO)x).ToList(),
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
