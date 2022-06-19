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
        public string EmailAddress { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string ProfilePicture { get; set; }
        public string UserType { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastLoginDate { get; set; }


        public static implicit operator UserDTO(ApplicationUser model)
        {
            return model is null ? null
               : new UserDTO
               {
                   Id = model.Id,
                   EmailAddress = model.Email,
                   FullName = model.FullName,
                   PhoneNumber = model.PhoneNumber,
                   IsActive = model.IsAccountLocked is false ? true : false,
                   ProfilePicture = model.ProfilePicture,
                   UserType = model.UserType.GetDescription(),
                   CreatedOn = model.CreationTime,
                   LastLoginDate = model.LastLoginDate
               };
        }
    }
}
