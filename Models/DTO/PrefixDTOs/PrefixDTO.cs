using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.PrefixDTOs
{
    public class PrefixDTO
    {
        public Guid Id { get; set; }
        public string PrefixNumber { get; set; }
        public DateTime CreatedOn { get; set; }


        public static implicit operator PrefixDTO(Prefix model)
        {
            return model is null ? null
              : new PrefixDTO
              {
                  Id = model.Id,
                  PrefixNumber = model.PrefixNumber,
                  CreatedOn = model.CreatedOn
              };
        }
    }
}
