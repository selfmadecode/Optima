﻿using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using Optima.Models.Config;
using Optima.Models.DTO;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;
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
        private readonly EmailLinkDTO _emailLink;
        private readonly ILog _logger;
        private readonly IEncrypt _encrypt;

        public EmailService(IOptions<SmtpConfigSettings> smtpConfigSettings, IWebHostEnvironment env, IOptions<EmailLinkDTO> emailLink, IEncrypt encrypt)
        {
            _smtpConfigSettings = smtpConfigSettings.Value;
            _env = env;
            _emailLink = emailLink.Value;
            _logger = LogManager.GetLogger(typeof(EmailService));
            _encrypt = encrypt;

            
        }

        public async Task<BaseResponse<bool>> SendMail(List<string> recipient, string[] replacements, string subject, string emailTemplatePath)
         => await BuildMailBody(recipient, replacements, subject, emailTemplatePath);

        private async Task<BaseResponse<bool>> BuildMailBody(List<string> destination, string[] replacements, string subject, string templateUrl)
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
        public string GeneratePasswordResetLinkAsync(string token, string email)
        {
            string baseUri = _emailLink.ResetPasswordUrl;

            var hrefValue = $"{baseUri}/{token}/{email}";
            var link = $"<a href='{hrefValue}'> Click here to reset password</a>";
            return link;
        }
        public async Task<BaseResponse<bool>> SendPasswordResetEmail(string emailAddress, string subject, string passwordResetLink)
        {

            string[] replacements = { passwordResetLink };
            return await GenerateMail(emailAddress, replacements, subject, EmailTemplateUrl.PasswordResetTemplate);
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

        private async Task<BaseResponse<bool>> SendEmail(MailRequest mailRequest)
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

                    var password = _smtpConfigSettings.Password; 

                    mailMessage.Subject = mailRequest.Subject;
                    mailMessage.Priority = MailPriority.High;
                    mailMessage.Body = mailRequest.Body;

                   
                    mailMessage.From = new MailAddress(_smtpConfigSettings.Mail, _smtpConfigSettings.DisplayName);
                    mailMessage.IsBodyHtml = true;

                    var smtpClient = new SmtpClient();

                    var credential = new NetworkCredential(_smtpConfigSettings.UserName, password);
                    smtpClient.Host = _smtpConfigSettings.Host;
                    smtpClient.Port = _smtpConfigSettings.Port;
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = credential;

                    _logger.Info("Sending email notification....");

                    await smtpClient.SendMailAsync(mailMessage);

                    return new BaseResponse<bool>(true, "MESSAGE SENT SUCCESSFULLY!...");
                }

                return new BaseResponse<bool>(false, "Message not Sent!");
            }
            catch (Exception error)
            {
                try
                {
                    _logger.Error($"Error sending Email... {error.StackTrace}", error);

                    _logger.Error($"Retrying to send email...");

                    await Retry(_smtpConfigSettings.Password, _smtpConfigSettings.DisplayName, _smtpConfigSettings.Mail, mailRequest.Subject, mailRequest.Body, mailRequest.Body, mailRequest.Recipient);

                    return new BaseResponse<bool>(true, "MESSAGE SENT SUCCESSFULLY!...");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error sending Email... {ex.StackTrace}", ex);
                    return new BaseResponse<bool>(false, "Message not Sent...");
                }
            }

        }
        private async Task Retry(string apiKey, string displayName, string sender, string subject, string plainTextContent, string htmlContent, List<string> recipients)
        {
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(sender, displayName);
            var emailRecipients = new List<EmailAddress>();

            foreach (var recipient in recipients)
            {
                emailRecipients.Add(new EmailAddress
                {
                    Email = recipient
                });
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, emailRecipients, subject, plainTextContent, htmlContent);
            await client.SendEmailAsync(msg);            
        }
        public string GenerateEmailConfirmationLinkAsync(string token, string email)
        {
            string baseUri = _emailLink.BaseUrl;

            var hrefValue = $"{baseUri}/{token}/{email}";
            var link = $"<h5>Click <a href='{hrefValue}'> here</a> to verify your account</h5>";
            return link;
        }
        public async Task<BaseResponse<bool>> SendAccountVerificationEmail(string emailAddress, string firstName, string subject, string confirmationLink)
        {

            string[] replacements = { firstName, confirmationLink };
            return await GenerateMail(emailAddress, replacements, subject, EmailTemplateUrl.AccountVerificationTemplate);
        }
        public async Task<BaseResponse<bool>> SendAdminAccountVerificationEmail(string emailAddress, string firstName, string subject, string confirmationLink, string password)
        {

            string[] replacements = { firstName, confirmationLink, password };
            return await GenerateMail(emailAddress, replacements, subject, EmailTemplateUrl.AdminAccountVerificationTemplate);
        }
        private async Task<BaseResponse<bool>> GenerateMail(string emailAddress, string[] replacements, string subject, string templateUrl)
        {
            var emailTemplatePath = Path.Combine(_env.ContentRootPath, templateUrl);

            var htmlbody = GenerateEmailHtmlBody(replacements, emailTemplatePath);

            var mail = new MailRequest();
            mail.Recipient.Add(emailAddress);
            mail.Subject = subject;
            mail.IsHtmlBody = true;
            mail.Body = htmlbody;

            //return await SendEmailAsync(emailAddress, subject, htmlbody);
            return await SendEmail(mail);
        }

        public async Task<BaseResponse<bool>> SendAccountConfirmationEmail(string email, string name)
        {
            string[] replacements = { name };
            return await GenerateMail(email, replacements, "ACCOUNT CONFIRMATION", EmailTemplateUrl.AccountConfirmationTemplate);
        }

        public async Task SendAccountBlockedEmail(string email, string name)
        {
            string[] replacements = { name };
            await GenerateMail(email, replacements, "ACCOUNT BLOCKED", EmailTemplateUrl.AccountBlockedTemplate);
        }

        public async Task SendAccountUnBlockedEmail(string email, string name)
        {
            string[] replacements = { name };
            await GenerateMail(email, replacements, "ACCOUNT UNBLOCKED", EmailTemplateUrl.AccountUnBlockedTemplate);
        }
    }
}
