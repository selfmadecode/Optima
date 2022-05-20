using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IEmailService
    {
        Task<bool> SendAccountVerificationEmail(string emailAddress, string firstName, string subject, string confirmationLink);
        string GenerateEmailConfirmationLinkAsync(string token, string email);
        Task<bool> SendMail(List<string> destination, string[] replacements, string subject, string emailTemplatePath);
        string GeneratePasswordResetLinkAsync(string token, string email);
        Task<bool> SendPasswordResetEmail(string emailAddress, string subject, string passwordResetLink);
    }
}
