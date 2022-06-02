using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class UpdateCardDTO
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public IFormFile Logo { get; set; }
        [Required]
        public List<Guid> CountryIds { get; set; }
        //public List<Guid> CardTypeIds { get; set; }
    }
}
