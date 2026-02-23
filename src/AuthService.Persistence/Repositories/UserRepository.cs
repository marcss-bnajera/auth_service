using AuthService.Domain.Entitis;
using AuthService.Domain.Interfaces;
using AuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<User> GetByIdAsync(string id)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.UserEmail)
            .Include(u => u.UserRoles)
            .Include(u => u.PasswordReset)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user ?? throw new InvalidOperationException($"Usuario con id {id} no encontrado");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.UserEmail)
            .Include(u => u.UserRoles)
            .Include(u => u.PasswordReset)
            .FirstOrDefaultAsync(u => EF.Functions.Like(u.UserEmail.Email, email));
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.UserEmail)
            .Include(u => u.UserRoles)
            .Include(u => u.PasswordReset)
            .FirstOrDefaultAsync(u => EF.Functions.Like(u.UserName, username));
    }

    public async Task<User?> GetByEmailVerificationTokenAsync(string token)
    {
        return await _context.Users
            .Include(u => u.UserEmail)
            .FirstOrDefaultAsync(u => u.UserEmail != null && u.UserEmail.EmailVerificationToken == token); 
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token)
    {
        return await _context.Users
            .Include(u => u.PasswordReset)
            .FirstOrDefaultAsync(u => u.PasswordReset != null && u.PasswordReset.Token == token);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(user.Id);
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(user.Id);
    }

    public async Task<User> DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => EF.Functions.Like(u.UserEmail.Email, email));
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => EF.Functions.Like(u.UserName, username));
    }

    public async Task UpdateUserRoleAsync(string userId, string roleId)
    {
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
        
        _context.UserRoles.RemoveRange(existingRoles);
        
        var newUserRole = new UserRole
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 16), 
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.UserRoles.Add(newUserRole);
        await _context.SaveChangesAsync();
    }
}
