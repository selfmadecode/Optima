using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardTransactionDTOs
{
    public class CardTransactionImagesDTO
    {
        public Guid Id { get; set; }
        public string LogoUrl { get; set; }
        public DateTime CreatedOn { get; set; }


        public static implicit operator CardTransactionImagesDTO(TransactionUploadFiles model)
        {
            return model is null ? null
               : new CardTransactionImagesDTO
               {
                   Id = model.Id,
                   LogoUrl = model.LogoUrl,
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
