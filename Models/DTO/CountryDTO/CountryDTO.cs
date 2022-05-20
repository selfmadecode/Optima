using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CountryDTO
{
    public class CountryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }



        public static implicit operator CountryDTO(Country model)
        {
            return model is null ? null
               : new CountryDTO
               {
                   Id = model.Id,
                   Name = model.Name,
                   
               };
        }
    }
}
