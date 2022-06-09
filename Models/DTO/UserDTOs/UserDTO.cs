using Optima.Models.Entities;
using Optima.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.UserDTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }
        public DateTime CreatedOn { get; set; }


        public static implicit operator UserDTO(ApplicationUser model)
        {
            return model is null ? null
               : new UserDTO
               {
                   Id = model.Id,
                   FullName = model.FullName,
                   UserType = model.UserType.GetDescription(),
                   CreatedOn = model.CreationTime
               };
        }
    }
}
