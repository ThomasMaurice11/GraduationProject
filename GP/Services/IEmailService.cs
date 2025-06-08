namespace GP.Services
{
    //public interface IEmailService
    //{
    //    Task SendEmailAsync(string email, string subject, string message);
    //}
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}


