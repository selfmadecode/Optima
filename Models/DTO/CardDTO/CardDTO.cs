using Optima.Models.DTO.CountryDTOs;
using Optima.Models.DTO.DenominationDTOs;
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

    public class MainCardDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CardStatus CardStatus { get; set; }
        public string Logo { get; set; }
        public BaseCardType BaseCardType { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<MainCardTypeDTO> MainCardTypeDTOs { get; set; }

        public static implicit operator MainCardDTO(Card model)
        {
            return model is null ? null
               : new MainCardDTO
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

    public class MainCardTypeDTO
    {
        public Guid CountryId { get; set; }
        public string CountryName { get; set; }
        public string Logo { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<MainCardTypes> CardTypesDTO { get; set; }

        public static implicit operator MainCardTypeDTO(List<CardType> model)
        {
            return model is null ? null
               : new MainCardTypeDTO
               {
                   CardTypesDTO = model.Select(x => (MainCardTypes)x).ToList()
               };
        }
    }

    public class MainCardTypes
    {
        public Guid Id { get; set; }
        public CardCategory CardType { get; set; }
        public CardStatus CardStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public PrefixCardTypeDenomination Prefix { get; set; }
        public ReceiptCardTypeDenomination Receipt { get; set; }
        public List<MainCardTypeDenominationDTO> CardTypeDenominationDTOs { get; set; }

        public static implicit operator MainCardTypes(CardType model)
        {
            return model is null ? null
               : new MainCardTypes
               {
                   Id = model.Id,
                   CardType = model.CardCategory,
                   CardStatus = model.CardStatus,
                   CreatedOn = model.CreatedOn,
                   CardTypeDenominationDTOs = model.CardTypeDenomination.Select(x => (MainCardTypeDenominationDTO)x).ToList(),
               };
        }
    }
    #region

    /* public class MobileCardTypes
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
 */
    #endregion

    public class PrefixCardTypeDenomination
    {
        public Guid PrefixId { get; set; }
        public string PrefixName { get; set; }
        public List<MainCardTypeDenominationDTO> MainCardTypeDenominationDTO { get; set; }
    }

    public class ReceiptCardTypeDenomination
    {
        public Guid ReceiptId { get; set; }
        public string ReceiptType { get; set; }
        public List<MainCardTypeDenominationDTO> MainCardTypeDenominationDTO { get; set; }

    }

    public class MainCardTypeDenominationDTO
    {
        public Guid Id { get; set; }
        public DenominationDTO DenominationDTO { get; set; }
        public decimal Rate { get; set; }
        public DateTime CreatedOn { get; set; }



        public static implicit operator MainCardTypeDenominationDTO(CardTypeDenomination model)
        {
            return model is null ? null
               : new MainCardTypeDenominationDTO
               {
                   Id = model.Id,
                   DenominationDTO = model.Denomination,
                   Rate = model.Rate,
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
