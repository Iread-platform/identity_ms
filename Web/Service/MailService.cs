using System;
using System.Configuration;
using System.Net.Mail;
using MimeKit;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Math.EC;

namespace iread_identity_ms.Web.Service
{
    public class MailService
    {
        private IConfiguration _configuration;
        public void SendEmail(string userEmail, string subject , string body)
        {
            string adminMail = _configuration.GetValue<string>("Mail:Address");
            string smtpClient =  _configuration.GetValue<string>("Mail:SmtpClient");
            int smtpPort =  _configuration.GetValue<int>("Mail:SmtpPort");
            
            MailAddress to = new MailAddress(userEmail);
            MailAddress from = new MailAddress(adminMail);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            SmtpClient client = new SmtpClient(smtpClient, smtpPort)
            {
                Credentials = new NetworkCredential(adminMail, "chscgeieyuqcizti"),
                EnableSsl = true
            };
            
            try
            { 
                client.Send(message);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}