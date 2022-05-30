using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CountryDTOs
{
    public class CountryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }



        public static implicit operator CountryDTO(Country model)
        {
            return model is null ? null
               : new CountryDTO
               {
                   Id = model.Id,
                   Name = model.Name,  
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
