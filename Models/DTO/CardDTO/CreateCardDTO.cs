using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class CreateCardDTO
    {
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public List<Guid> CountryIds { get; set; }
    }
}
