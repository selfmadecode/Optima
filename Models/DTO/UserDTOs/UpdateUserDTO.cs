using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.UserDTOs
{
    public class UpdateUserDTO
    {
        [Required]
        public string PhoneNumber { get; set; }
        public IFormFile ProfilePicture { get; set; }

    }
}
