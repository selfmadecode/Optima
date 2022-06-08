using Optima.Models.DTO.CardCodeDTOs;
using Optima.Models.DTO.CardDTO;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardSaleDTO
{
    public class CardSalesDTO
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public CardTypeDenominationDTO CardTypeDenominationDTO { get; set; }
        public List<CardCodeDTO> CardCodeDTOs { get; set; }
        public DateTime CreatedOn { get; set; }
       


        public static implicit operator CardSalesDTO(CardSold model)
        {
            return model is null ? null
               : new CardSalesDTO
               {
                   Id = model.Id,
                   Amount = model.Amount,
                   CardTypeDenominationDTO = model.CardTypeDenomination,
                   CardCodeDTOs = model.CardCodes.Select(x => (CardCodeDTO)x).ToList(),
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
