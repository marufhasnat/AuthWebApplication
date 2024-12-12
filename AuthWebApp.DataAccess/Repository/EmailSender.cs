using AuthWebApp.DataAccess.Repository.IRepository;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AuthWebApp.Models.ViewModels.Email;

namespace AuthWebApp.DataAccess.Repository
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> EamilSendAsynce(string email, string subject, string message)
        {
            bool status = false;

            try
            {
                GetEmailSetting getEmailSetting = new GetEmailSetting()
                {
                    SecretKey = _configuration.GetValue<string>("AppSettings:SecretKey") ?? string.Empty,
                    From = _configuration.GetValue<string>("AppSettings:EmailSettings:From") ?? string.Empty,
                    SmtpServer = _configuration.GetValue<string>("AppSettings:EmailSettings:SmtpServer") ?? string.Empty,
                    Port = _configuration.GetValue<int>("AppSettings:EmailSettings:Port"),
                    EnableSSL = _configuration.GetValue<bool>("AppSettings:EmailSettings:EnablSSL"),
                };

                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(getEmailSetting.From),
                    Subject = subject,
                    Body = message,
                    BodyEncoding = System.Text.Encoding.ASCII,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                SmtpClient smtpClient = new SmtpClient(getEmailSetting.SmtpServer)
                {
                    Port = getEmailSetting.Port,
                    Credentials = new NetworkCredential(getEmailSetting.From, getEmailSetting.SecretKey),
                    EnableSsl = getEmailSetting.EnableSSL
                };

                await smtpClient.SendMailAsync(mailMessage);
                status = true;
            }
            catch (Exception)
            {

                status = false;
            }

            return status;

        }
    }
}
