using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.FaqsDTO
{
    public class FaqDTO
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedOn { get; set; }


        public static implicit operator FaqDTO(Faq model)
        {
            return model is null ? null
               : new FaqDTO
               {
                   Id = model.Id,
                   Question = model.Question,
                   Answer = model.Answer,
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
