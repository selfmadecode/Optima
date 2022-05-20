using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Models.DTO
{
    public class MailRequest
    {
        public List<string> Recipient { get; set; } = new List<string>();
        public List<string> BCC { get; set; } = new List<string>();
        public List<string> CC { get; set; } = new List<string>();
        public bool IsHtmlBody { get; set; } = true;
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
    }
}
