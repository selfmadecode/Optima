using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CountryDTOs
{
    public class CreateCountryDTO
    {
        [Required]
        public string CountryName { get; set; }
        public IFormFile Logo { get; set; } 
    }

    public class ValidateCountryDTO
    {
        public List<Guid> CountryIds { get; set; }
    }
}
