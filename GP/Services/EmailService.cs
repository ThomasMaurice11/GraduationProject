using System.Net.Mail;
using System.Net;

namespace GP.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public async Task SendEmailAsync(string email, string subject, string message)
        //{
        //    var emailSettings = _configuration.GetSection("EmailSettings");
        //    var mailMessage = new MailMessage
        //    {
        //        From = new MailAddress(emailSettings["Sender"], emailSettings["SenderName"]),
        //        Subject = subject,
        //        Body = message,
        //        IsBodyHtml = true
        //    };
        //    mailMessage.To.Add(email);

        //    using var smtpClient = new SmtpClient(emailSettings["MailServer"], int.Parse(emailSettings["MailPort"]))
        //    {
        //        Credentials = new NetworkCredential(emailSettings["Sender"], emailSettings["Password"]),
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        UseDefaultCredentials = false
        //    };

        //    await smtpClient.SendMailAsync(mailMessage);
        //}

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["Sender"], emailSettings["SenderName"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            using var smtpClient = new SmtpClient(emailSettings["MailServer"], int.Parse(emailSettings["MailPort"]))
            {
                Credentials = new NetworkCredential(emailSettings["Sender"], emailSettings["Password"]),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 20000 // 20 seconds timeout
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
