using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CountryDTOs
{
    public class CreateCountryDTO
    {
        public string CountryName { get; set; }
    }

    public class ValidateCountryDTO
    {
        public List<Guid> CountryIds { get; set; }
    }
}
