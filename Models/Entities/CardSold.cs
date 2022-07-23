﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class CardSold : BaseEntity
    {
        public Guid DenominationId { get; set; }
        public Denomination Denomination { get; set; }

        public Decimal Rate { get; set; }

        public string Code { get; set; }

        public CardTransaction CardTransaction { get; set; }
        public Guid? CardTransactionId { get; set; }

        //public List<CardCodes> CardCodes { get; set; } = new List<CardCodes>();
    }
}
