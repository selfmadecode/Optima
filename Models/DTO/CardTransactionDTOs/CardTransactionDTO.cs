using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardTransactionDTOs
{
    public class CardTransactionDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<CardTransactionImagesDTO> CardTransactionImagesDTOs { get; set; }


        public static implicit operator CardTransactionDTO(CardTransaction model)
        {
            return model is null ? null
               : new CardTransactionDTO
               {
                   Id = model.Id,
                   Name = model.Name,
                   Logo = model.LogoUrl,
                   CardTransactionImagesDTOs = model.TransactionUploadededFiles.Select(x => (CardTransactionImagesDTO)x).ToList(),
                   CreatedOn = model.CreatedOn
               };
        }
    }
}
