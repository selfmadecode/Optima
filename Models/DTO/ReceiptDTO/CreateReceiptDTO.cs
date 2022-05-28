using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.ReceiptDTO
{
    public class CreateReceiptDTO
    {
        public string Name { get; set; }
        public List<Guid> CountryIds { get; set; }
        public List<Guid> DenominationIds { get; set; }

    }
}
