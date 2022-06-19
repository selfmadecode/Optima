using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardCodeDTOs
{
    public class CardCodeDTO
    {
        public Guid Id { get; set; }
        public string CardCode { get; set; }
        public DateTime CreatedOn { get; set; }


        public static implicit operator CardCodeDTO(CardCodes model)
        {
            return model is null ? null
               : new CardCodeDTO
               {
                   Id = model.Id,
                   CardCode = model.CardCode,
                   CreatedOn = model.CreatedOn,
               };
        }
    }
}
