using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CountryDTOs
{
    public class UpdateCountryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IFormFile Logo { get; set; }
    }
}
