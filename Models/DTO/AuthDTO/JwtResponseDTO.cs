﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.AuthDTO
{
    public class JwtResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public IList<string> Roles { get; set; }
        public IList<string> Permissions { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
    }
}
