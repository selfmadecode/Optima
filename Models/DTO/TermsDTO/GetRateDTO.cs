using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.TermsDTO
{
    public class GetRateDTO
    {
        public Guid Id { get; set; }
        public string TermsAndCondition { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
