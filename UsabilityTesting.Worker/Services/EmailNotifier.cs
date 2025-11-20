using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using UsabilityTesting.Worker.Models;

namespace UsabilityTesting.Worker.Services;

public class EmailNotifier
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailNotifier> _logger;

    public EmailNotifier(IOptions<SmtpSettings> smtpSettings, ILogger<EmailNotifier> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendAlertAsync(string subject, string body, string recipients)
    {
        if (string.IsNullOrWhiteSpace(recipients))
        {
            _logger.LogWarning("No recipients provided for alert: {Subject}", subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.DisplayName, _smtpSettings.FromEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        message.Body = builder.ToMessageBody();

        var emailList = recipients.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var email in emailList)
        {
            if (MailboxAddress.TryParse(email, out var address))
            {
                message.To.Add(address);
            }
            else
            {
                _logger.LogWarning("Invalid email address: {Email}", email);
            }
        }

        if (message.To.Count == 0) return;

        try
        {
            using var client = new SmtpClient();
            // Accept all SSL certificates (use with caution in production, but common for internal SMTP)
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.EnableSsl);
            
            if (!string.IsNullOrEmpty(_smtpSettings.UserName))
            {
                await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Email sent to {Count} recipients. Subject: {Subject}", message.To.Count, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email alert.");
        }
    }
}
