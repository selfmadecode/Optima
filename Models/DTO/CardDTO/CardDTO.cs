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
    public class CardDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CardStatus CardStatus { get; set; }
        public string Logo { get; set; }
        public BaseCardType BaseCardType { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<CardTypeDTO> CardTypeDTOs { get; set; }


        public static implicit operator CardDTO(Card model)
        {
            return model is null ? null
               : new CardDTO
               {
                   Id = model.Id,
                   Name = model.Name,
                   CardStatus = model.CardStatus,
                   Logo = model.LogoUrl,
                   CardTypeDTOs = model.CardType.Select(x => (CardTypeDTO)x).ToList(),
                   CreatedOn = model.CreatedOn,
                   BaseCardType = model.BaseCardType
               };
        }
    }

    public class MobileCardDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CardStatus CardStatus { get; set; }
        public string Logo { get; set; }
        public BaseCardType BaseCardType { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<MobileCardTypeDTO> MobileCardTypeDTOs { get; set; }

        public static implicit operator MobileCardDTO(Card model)
        {
            return model is null ? null
               : new MobileCardDTO
               {
                   Id = model.Id,
                   Name = model.Name,
                   CardStatus = model.CardStatus,
                   Logo = model.LogoUrl,
                   BaseCardType = model.BaseCardType,
                   CreatedOn = model.CreatedOn,
               };
        }
    }

    public class MobileCardTypeDTO
    {
        public Guid CountryId { get; set; }
        public string CountryName { get; set; }
        public string Logo { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<MobileCardTypes> CardTypesDTO { get; set; }

        public static implicit operator MobileCardTypeDTO(List<CardType> model)
        {
            return model is null ? null
               : new MobileCardTypeDTO
               {
                   CardTypesDTO = model.Select(x => (MobileCardTypes)x).ToList()
               };
        }
    }

    public class MobileCardTypes
    {
        public Guid Id { get; set; }
        public string CardType { get; set; }
        public string CardStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<CardTypeDenominationDTO> CardTypeDenominationDTOs { get; set; }

        public static implicit operator MobileCardTypes(CardType model)
        {
            return model is null ? null
               : new MobileCardTypes
               {
                   Id = model.Id,
                   CardType = model.CardCategory.GetDescription(),
                   CardStatus = model.CardStatus.GetDescription(),
                   CreatedOn = model.CreatedOn,
                   CardTypeDenominationDTOs = model.CardTypeDenomination.Select(x => (CardTypeDenominationDTO)x).ToList(),
               };
        }
    }

}
