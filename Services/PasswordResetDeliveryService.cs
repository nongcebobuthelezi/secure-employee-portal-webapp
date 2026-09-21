// Delivers password-reset links through SMTP or a private local development mailbox.
using System.Net;
using System.Net.Mail;

namespace SecureEmployeePortal.Services;

public sealed class PasswordResetDeliveryService(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<PasswordResetDeliveryService> logger)
{
    public async Task SendAsync(string email, string resetLink)
    {
        var section = configuration.GetSection("Smtp");
        var host = section["Host"];
        var from = section["From"];

        // Development can work without external email credentials. The token-bearing
        // message is written outside wwwroot and is excluded from Git instead of being
        // printed to application logs.
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException("SMTP configuration is required outside Development.");
            }

            var mailboxDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "development-mail");
            Directory.CreateDirectory(mailboxDirectory);

            var fileName = $"password-reset-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.txt";
            var messagePath = Path.Combine(mailboxDirectory, fileName);
            var body = $"To: {email}{Environment.NewLine}" +
                       $"Subject: Secure Employee Portal password reset{Environment.NewLine}{Environment.NewLine}" +
                       $"Use this secure link to reset your password:{Environment.NewLine}{Environment.NewLine}" +
                       resetLink + Environment.NewLine + Environment.NewLine +
                       "If you did not request this, you can ignore this message.";

            await File.WriteAllTextAsync(messagePath, body);
            logger.LogInformation("Development password-reset message written to {MessagePath}.", messagePath);
            return;
        }

        var port = int.TryParse(section["Port"], out var configuredPort) ? configuredPort : 587;
        var username = section["Username"];
        var password = section["Password"];
        var enableSsl = !bool.TryParse(section["EnableSsl"], out var ssl) || ssl;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        using var message = new MailMessage(from, email)
        {
            Subject = "Secure Employee Portal password reset",
            Body = $"Use this secure link to reset your password:{Environment.NewLine}{Environment.NewLine}" +
                   resetLink + Environment.NewLine + Environment.NewLine +
                   "If you did not request this, you can ignore this message."
        };

        await client.SendMailAsync(message);
    }
}
