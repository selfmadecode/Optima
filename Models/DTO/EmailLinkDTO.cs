using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO
{
    public class EmailLinkDTO
    {
        public string BaseUrl { get; set; }
        public string ChangeDefaultPasswordUrl { get; set; }
        public string ResetPasswordUrl { get; set; }
        public string AdminChangePasswordUrl { get; set; }
    }
}
