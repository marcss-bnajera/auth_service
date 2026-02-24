using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using AuthService.Application.Interfaces;

namespace AuthService.Application.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    // 1. Implementación exacta de tu interfaz para Verificación de Email
    public async Task<bool> SendEmailAsync(string email, string username, string token)
    {
        var subject = "Verify your email address";
        var verificationUrl = $"{configuration["AppSettings:FrontendUrl"]}/verify-email?token={token}";

        var body = $@"
            <h2>Welcome {username}!</h2>
            <p>Please verify your email address by clicking the link below:</p>
            <a href='{verificationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                Verify Email
            </a>
            <p>If you cannot click the link, copy and paste this URL into your browser:</p>
            <p>{verificationUrl}</p>";

        return await ExecuteSendEmailAsync(email, subject, body);
    }

    // 2. Implementación exacta de tu interfaz para Reset Password
    public async Task<bool> SendPasswordResetAsync(string email, string username, string token)
    {
        var subject = "Reset your password";
        var resetUrl = $"{configuration["AppSettings:FrontendUrl"]}/reset-password?token={token}";

        var body = $@"
            <h2>Password Reset Request</h2>
            <p>Hello {username},</p>
            <p>You requested to reset your password. Click the link below to reset it:</p>
            <a href='{resetUrl}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                Reset Password
            </a>";

        return await ExecuteSendEmailAsync(email, subject, body);
    }

    // 3. Implementación exacta de tu interfaz para Bienvenida
    public async Task<bool> SendWelcomeEmailAsync(string email, string username)
    {
        var subject = "Welcome to AuthDotnet!";

        var body = $@"
            <h2>Welcome to AuthDotnet, {username}!</h2>
            <p>Your account has been successfully verified and activated.</p>";

        return await ExecuteSendEmailAsync(email, subject, body);
    }

    private async Task<bool> ExecuteSendEmailAsync(string to, string subject, string body)
    {
        var smtpSettings = configuration.GetSection("SmtpSettings");

        try
        {
            var enabled = bool.Parse(smtpSettings["Enabled"] ?? "true");
            if (!enabled)
            {
                logger.LogInformation("Email disabled in configuration. Skipping send");
                return true;
            }

            using var client = new SmtpClient();
            client.CheckCertificateRevocation = false;
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var user = smtpSettings["Username"];
            var pass = smtpSettings["Password"];

            if (port == 465)
                await client.ConnectAsync(host, port, SecureSocketOptions.SslOnConnect);
            else
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(user, pass);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpSettings["FromName"] ?? "Auth System", smtpSettings["FromEmail"] ?? "noreply@auth.com"));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email to {Email}", to);
            return false;
        }
    }
}