using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO.AuthDTO
{
    public class RegisterDTO
    {
        [Required]
        public string FullName { get; set; }
                
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool HasAcceptedTerms { get; set; }
    }
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }

    public class ResetPasswordDTO
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }
        public string Token { get; set; }
    }

    public class ChangePasswordDTO
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }

    public class ResetDefaultPasswordDTO
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }

        public bool IsFounder { get; set; }
        public bool IsInvestor { get; set; }
        public bool IsFundManager { get; set; }
    }

    public class CreateAdminAccountDTO
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public IList<string> Permissions { get; set; }
    }
}
