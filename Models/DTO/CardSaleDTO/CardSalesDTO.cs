using Optima.Models.DTO.RateDTO;
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
        public string Name { get; set; }
        public string Logo { get; set; }
        public DenominationDTO DenominationDTO { get; set; }
        public DateTime CreatedOn { get; set; }
       


        public static implicit operator CardSalesDTO(CardSold model)
        {
            return model is null ? null
               : new CardSalesDTO
               {
                   Id = model.Id,
                   Name = model.Name,
                   Logo = model.LogoUrl,
                   CardTypeDTOs = model.CardType.Select(x => (CardTypeDTO)x).ToList(),
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
