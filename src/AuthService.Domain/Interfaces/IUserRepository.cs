using AuthService.Domain.Entitis;

namespace AuthService.Domain.Interfaces;

public interface IUserRepository
{
    // Metodos de consulta
    Task<User> CreateAsync(User user);
    Task<User> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailVerificationTokenAsync(string token);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<User> UpdateAsync(User user);
    Task<User> DeleteAsync(User user);

    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task UpdateUserRoleAsync(string userId, string roleId);
}