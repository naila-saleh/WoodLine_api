using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace BakerGroup.PL.Utilities;

/// <summary>
/// Mock email sender for development/testing.
/// Logs emails instead of actually sending them.
/// Replace with real provider (SendGrid, Mailgun, etc.) in production.
/// </summary>
public class MockEmailSender : IEmailSender
{
    private readonly ILogger<MockEmailSender> _logger;

    public MockEmailSender(ILogger<MockEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation($"[MOCK EMAIL] To: {email}");
        _logger.LogInformation($"[MOCK EMAIL] Subject: {subject}");
        _logger.LogInformation($"[MOCK EMAIL] Body: {htmlMessage}");
        
        // For development, you can write to a file or console
        Console.WriteLine($"\n{'='*80}");
        Console.WriteLine($"[MOCK EMAIL SENT]");
        Console.WriteLine($"To: {email}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"{'='*80}\n");

        return Task.CompletedTask;
    }
}

