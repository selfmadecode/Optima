using Optima.Models.DTO.PrefixDTOs;
using Optima.Models.DTO.RateDTO;
using Optima.Models.DTO.ReceiptDTOs;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class CardTypeDenominationDTO
    {
        public Guid Id { get; set; }
        public DenominationDTO DenominationDTO { get; set; }
        public PrefixDTO PrefixDTO { get; set; }
        public ReceiptDTO ReceiptDTO { get; set; }
        public decimal Rate { get; set; }
        public DateTime CreatedOn { get; set; }



        public static implicit operator CardTypeDenominationDTO(CardTypeDenomination model)
        {
            return model is null ? null
               : new CardTypeDenominationDTO
               {
                   Id = model.Id,
                   DenominationDTO = model.Denomination,
                   PrefixDTO = model.Prefix,
                   ReceiptDTO = model.Receipt,
                   Rate = model.Rate,
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
