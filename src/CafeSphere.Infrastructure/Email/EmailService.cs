using CafeSphere.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CafeSphere.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending Email to [{To}] Subject: [{Subject}]", to, subject);
        return Task.CompletedTask;
    }

    public Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        var body = $"<h1>Welcome to CafeSphere!</h1><p>Please verify your email using this token: <strong>{token}</strong></p>";
        return SendEmailAsync(to, "CafeSphere - Email Verification", body, true, cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        var body = $"<h1>Password Reset Request</h1><p>Reset your CafeSphere password using this token: <strong>{token}</strong></p>";
        return SendEmailAsync(to, "CafeSphere - Password Reset", body, true, cancellationToken);
    }
}
