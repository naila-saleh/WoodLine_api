using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WoodLine.PL.Utilities;

public class EmailSetting : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailSetting> _logger;

    public EmailSetting(IOptions<EmailSettings> settings, ILogger<EmailSetting> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            _logger.LogInformation($"[SMTP] ========== EMAIL SEND STARTED ==========");
            _logger.LogInformation($"[SMTP] Recipient: {email}");
            _logger.LogInformation($"[SMTP] Subject: {subject}");
            
            var host = _settings.Host ?? throw new InvalidOperationException("EmailSettings:Host is missing.");
            var port = _settings.Port > 0 ? _settings.Port : throw new InvalidOperationException("EmailSettings:Port is missing or invalid.");
            var username = _settings.Username ?? throw new InvalidOperationException("EmailSettings:Username is missing.");
            var password = _settings.Password ?? throw new InvalidOperationException("EmailSettings:Password is missing.");
            var fromAddress = string.IsNullOrWhiteSpace(_settings.FromAddress) ? username : _settings.FromAddress;
            var fromName = string.IsNullOrWhiteSpace(_settings.FromName) ? "WoodLine" : _settings.FromName;
            var timeoutSeconds = _settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 30;

            _logger.LogInformation($"[SMTP] Config loaded: Host={host}, Port={port}, EnableSsl={_settings.EnableSsl}, FromAddress={fromAddress}, FromName={fromName}, Timeout={timeoutSeconds}s");
            _logger.LogInformation($"[SMTP] Creating SMTP client...");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = _settings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                Timeout = timeoutSeconds * 1000
            };

            _logger.LogInformation($"[SMTP] SMTP client created successfully");
            _logger.LogInformation($"[SMTP] Creating mail message from {fromAddress} ({fromName}) to {email}");

            using var mailMessage = new MailMessage(from: fromAddress, to: email, subject, htmlMessage)
            {
                From = new MailAddress(fromAddress, fromName),
                IsBodyHtml = true
            };

            _logger.LogInformation($"[SMTP] Mail message created. Body length: {htmlMessage.Length} chars");
            _logger.LogInformation($"[SMTP] Calling SendMailAsync...");
            
            await client.SendMailAsync(mailMessage);
            
            _logger.LogInformation($"[SMTP] ✓ EmailSetting.SendEmailAsync completed successfully for {email}");
            _logger.LogInformation($"[SMTP] ========== EMAIL SEND COMPLETED ==========");
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, $"[SMTP-ERROR] SMTP error sending to {email}. Status Code: {smtpEx.StatusCode}, Message: {smtpEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[SMTP-ERROR] {ex.GetType().FullName} error sending to {email}: {ex.Message}. StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}