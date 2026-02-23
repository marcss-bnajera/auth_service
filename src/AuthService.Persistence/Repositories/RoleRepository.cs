using AuthService.Domain.Entitis;
using AuthService.Domain.Interfaces;
using AuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Repositories;

public class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _context.Roles
        .Include(r => r.UserRoles)
        .FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<int> CountUsersInRoleAsync(string roleName)
    {
        return await _context.UserRoles
        .Where(ur => ur.Role.Name == roleName)
        .CountAsync();
    }

    public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(string roleName)
    {
        return await _context.UserRoles
        .Where(ur => ur.Role.Name == roleName)
        .Select(ur => ur.User)
        .Include(u => u.Profile)
        .Include(u => u.UserEmail)
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .ToListAsync()
        .ContinueWith(t => (IReadOnlyList<User>)t.Result);
    }

    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(string userId)
    {
        return await _context.UserRoles
        .Where(ur => ur.UserId == userId)
        .Select(ur => ur.Role.Name)
        .ToListAsync()
        .ContinueWith(t => (IReadOnlyList<string>)t.Result);
    }

}

