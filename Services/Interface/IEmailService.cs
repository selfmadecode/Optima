using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface IEmailService
    {
        Task<bool> SendMail(List<string> destination, string[] replacements, string subject, string emailTemplatePath);
    }
}
