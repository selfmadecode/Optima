using Optima.Models.Entities;
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
        public string CardCategory { get; set; }
        public string CountryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<CardTypeDenominationDTO> CardTypeDenominationDTOs { get; set; }


        public static implicit operator CardTypeDTO(CardType model)
        {
            return model is null ? null
               : new CardTypeDTO
               {
                   Id = model.Id,
                   CardCategory = model.CardCategory.GetDescription(),
                   CountryName = model.Country.Name,
                   CardTypeDenominationDTOs = model.CardTypeDenomination.Select(x => (CardTypeDenominationDTO)x).ToList(),
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
