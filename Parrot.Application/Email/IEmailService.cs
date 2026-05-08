namespace Parrot.Application.Email;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
