using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceNowUnlockWorkflow.Configuration;

namespace ServiceNowUnlockWorkflow.Services;

public class SmtpNotificationService
{
    private readonly AppSettings _settings;
    private readonly ILogger<SmtpNotificationService> _logger;

    public SmtpNotificationService(IOptions<AppSettings> settings, ILogger<SmtpNotificationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task NotifyAsync(ServiceNowIncident incident, UnlockResult result, CancellationToken cancellationToken)
    {
        var smtp = _settings.Smtp;
        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            Credentials = new NetworkCredential(smtp.UserName, smtp.Password)
        };

        using var message = new MailMessage(smtp.FromAddress, incident.CallerEmail)
        {
            Subject = $"AD Unlock - Incident {incident.Number}",
            Body = $"Hello,\n\nYour AD account unlock request has been {(result.Success ? "completed successfully" : "processed with errors")}" +
                   $". Details: {result.Message}\n\nThank you."
        };

        _logger.LogInformation("Sending notification for {Incident}", incident.Number);
        await client.SendMailAsync(message, cancellationToken);
    }
}
