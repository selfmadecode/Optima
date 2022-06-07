﻿using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.Entities
{
    public class Country : BaseEntity
    {
        public string Name { get; set; }
        public string LogoUrl { get; set; }

    }
}
