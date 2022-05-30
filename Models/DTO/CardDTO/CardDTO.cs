﻿using Optima.Models.DTO.CountryDTOs;
using Optima.Models.Entities;
using Optima.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.CardDTO
{
    public class CardDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CardStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<CardTypeDTO> CardTypeDTOs { get; set; }


        public static implicit operator CardDTO(Card model)
        {
            return model is null ? null
               : new CardDTO
               {
                   Id = model.Id,
                   Name = model.Name,
                   CardStatus = model.CardStatus.GetDescription(),
                   CardTypeDTOs = model.CardType.Select(x => (CardTypeDTO)x).ToList(),
                   CreatedOn = model.CreatedOn
               };
        }
    }
}