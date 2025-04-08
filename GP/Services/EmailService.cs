//using System.Net.Mail;
//using System.Net;

//namespace GP.Services
//{
//    public class EmailService : IEmailService
//    {
//        private readonly IConfiguration _configuration;

//        public EmailService(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        //public async Task SendEmailAsync(string email, string subject, string message)
//        //{
//        //    var emailSettings = _configuration.GetSection("EmailSettings");
//        //    var mailMessage = new MailMessage
//        //    {
//        //        From = new MailAddress(emailSettings["Sender"], emailSettings["SenderName"]),
//        //        Subject = subject,
//        //        Body = message,
//        //        IsBodyHtml = true
//        //    };
//        //    mailMessage.To.Add(email);

//        //    using var smtpClient = new SmtpClient(emailSettings["MailServer"], int.Parse(emailSettings["MailPort"]))
//        //    {
//        //        Credentials = new NetworkCredential(emailSettings["Sender"], emailSettings["Password"]),
//        //        EnableSsl = true,
//        //        DeliveryMethod = SmtpDeliveryMethod.Network,
//        //        UseDefaultCredentials = false
//        //    };

//        //    await smtpClient.SendMailAsync(mailMessage);
//        //}

//        public async Task SendEmailAsync(string email, string subject, string message)
//        {
//            var emailSettings = _configuration.GetSection("EmailSettings");

//            var mailMessage = new MailMessage
//            {
//                From = new MailAddress(emailSettings["Sender"], emailSettings["SenderName"]),
//                Subject = subject,
//                Body = message,
//                IsBodyHtml = true
//            };
//            mailMessage.To.Add(email);

//            using var smtpClient = new SmtpClient(emailSettings["MailServer"], int.Parse(emailSettings["MailPort"]))
//            {
//                Credentials = new NetworkCredential(emailSettings["Sender"], emailSettings["Password"]),
//                EnableSsl = true,
//                DeliveryMethod = SmtpDeliveryMethod.Network,
//                UseDefaultCredentials = false,
//                Timeout = 20000 // 20 seconds timeout
//            };

//            await smtpClient.SendMailAsync(mailMessage);
//        }
//    }
//}


using GP.Models;
using GP.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    // Inject the configuration options from appsettings.json
    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
        {
            Port = _emailSettings.Port,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
