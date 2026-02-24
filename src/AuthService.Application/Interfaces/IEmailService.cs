namespace AuthService.Application.Interfaces;
 
public interface IEmailService
{
    Task<bool> SendEmailAsync(string email, string username, string token);
 
    Task<bool> SendPasswordResetAsync(string email, string username, string token);
 
    Task<bool> SendWelcomeEmailAsync(string email, string username);
}