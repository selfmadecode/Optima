using Optima.Models.DTO.CountryDTOs;
using Optima.Models.Entities;
using Optima.Models.Enums;
using Optima.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class CardTypeDTO
    {
        public Guid Id { get; set; }
        public CardCategory CardType { get; set; }
        public string CardStatus { get; set; }
        public CountryDTO CountryDTO { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<CardTypeDenominationDTO> CardTypeDenominationDTOs { get; set; }


        public static implicit operator CardTypeDTO(CardType model)
        {
            return model is null ? null
               : new CardTypeDTO
               {
                   Id = model.Id,
                   CardType = model.CardCategory,
                   CardStatus = model.CardStatus.GetDescription(),
                   CountryDTO = model.Country,
                   CardTypeDenominationDTOs = model.CardTypeDenomination.Select(x => (CardTypeDenominationDTO)x).ToList(),
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
