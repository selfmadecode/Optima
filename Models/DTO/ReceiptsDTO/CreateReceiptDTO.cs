﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.ReceiptDTOs
{
    public class CreateReceiptDTO
    {
        [Required]
        public string Name { get; set; }

    }
}
