using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.RateDTO
{
    public class RateDTO
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }


        public static implicit operator RateDTO(Rate model)
        {
            return model is null ? null
               : new RateDTO
               {
                   Id = model.Id,
                   Amount = model.Amount,

               };
        }
    }
}
