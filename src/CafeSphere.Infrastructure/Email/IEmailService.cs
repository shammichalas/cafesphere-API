namespace CafeSphere.Infrastructure.Email;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default);
}
