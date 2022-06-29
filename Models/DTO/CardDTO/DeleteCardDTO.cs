using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class DeleteCardDTO
    {
    }

    public class DeleteCardTypeDTO
    {
        public List<Guid> CardTypeIds { get; set; } = new List<Guid>();
    }

}
