using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using Optima.Models.Config;
using Optima.Models.DTO;
using Optima.Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly SmtpConfigSettings _smtpConfigSettings;
        private readonly IWebHostEnvironment _env;

        public EmailService(IOptions<SmtpConfigSettings> smtpConfigSettings, IWebHostEnvironment env)
        {
            _smtpConfigSettings = smtpConfigSettings.Value;
            _env = env;
        }

        public async Task<bool> SendMail(List<string> recipient, string[] replacements, string subject, string emailTemplatePath)
         => await BuildMailBody(recipient, replacements, subject, emailTemplatePath);

        private async Task<bool> BuildMailBody(List<string> destination, string[] replacements, string subject, string templateUrl)
        {
            var emailTemplatePath = Path.Combine(_env.ContentRootPath, templateUrl);

            var msg = new MailRequest
            {
                Recipient = destination,
                Subject = subject,
                IsHtmlBody = true,
                Body = GenerateEmailHtmlBody(replacements, emailTemplatePath)
            };

            return await SendEmail(msg);
        }

        private string GenerateEmailHtmlBody(string[] replacements, string path)
        {

            if (path == null)
                throw new NullReferenceException($"Email Template not found for {path}");

            var builder = new BodyBuilder();

            //IF THERE ARE NO REPLACEMENT, DO NOT THROW EXCEPTION
            //if (replacements == null)
            //  throw new NullReferenceException($"Replacements needed");

            string messageBody;
            using (StreamReader SourceReader = File.OpenText(path))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();

                if (replacements.Any())
                {
                    messageBody = string.Format(builder.HtmlBody, replacements);
                }
                else
                {
                    messageBody = string.Format(builder.HtmlBody);
                }

            }

            return messageBody;
        }

        private async Task<bool> SendEmail(MailRequest mailRequest)
        {

            try
            {
                if (!string.IsNullOrEmpty(mailRequest.Body) && !string.IsNullOrEmpty(mailRequest.Subject) && mailRequest.Recipient.Count > 0)
                {
                    var mailMessage = new MailMessage();

                    for (int i = 0; i < mailRequest.Recipient.Count; i++)
                    {
                        mailMessage.To.Add(new MailAddress(mailRequest.Recipient[i]));
                    }

                    if (mailRequest?.CC != null && mailRequest.CC.Count > 0)
                    {
                        for (int i = 0; i < mailRequest.CC.Count; i++)
                        {
                            mailMessage.To.Add(new MailAddress(mailRequest.CC[i]));
                        }
                    }

                    if (mailRequest?.BCC != null && mailRequest.BCC.Count > 0)
                    {
                        for (int i = 0; i < mailRequest.BCC.Count; i++)
                        {
                            mailMessage.To.Add(new MailAddress(mailRequest.BCC[i]));
                        }
                    }

                    mailMessage.Subject = mailRequest.Subject;
                    mailMessage.Priority = MailPriority.High;
                    mailMessage.Body = mailRequest.Body;

                   
                    mailMessage.From = new MailAddress(_smtpConfigSettings.Mail, _smtpConfigSettings.DisplayName);
                    mailMessage.IsBodyHtml = true;

                    var smtpClient = new SmtpClient();

                    var credential = new NetworkCredential(_smtpConfigSettings.UserName, _smtpConfigSettings.Password);
                    smtpClient.Host = _smtpConfigSettings.Host;
                    smtpClient.Port = _smtpConfigSettings.Port;
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = credential;

                    //_logger.LogInformation("Sending email notification....");

                    await smtpClient.SendMailAsync(mailMessage);

                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.StackTrace, ex, "An error occured while sending message");
                return false;
            }

        }
    }
}
