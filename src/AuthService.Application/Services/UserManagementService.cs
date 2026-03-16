using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Constants;
using AuthService.Domain.Entitis;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Services;

public class UserManagementService(IUserRepository users, IRoleRepository roles, ICloudinaryService cloudinary) : IUserManagementService
{
    public async Task<UserResponseDto> UpdateUserRoleAsync(string userId, string roleName)
    {
        roleName = roleName?.Trim().ToUpperInvariant() ?? string.Empty;
        
        if (!RoleConstants.AllowedRoles.Contains(roleName))
            throw new InvalidOperationException("Role not allowed.");

        var user = await users.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");

        var isUserAdmin = user.UserRoles.Any(r => r.Role.Name == RoleConstants.ADMIN_ROLE);
        if (isUserAdmin && roleName != RoleConstants.ADMIN_ROLE)
        {
            var adminCount = await roles.CountUsersInRoleAsync(RoleConstants.ADMIN_ROLE);
            if (adminCount <= 1) throw new InvalidOperationException("Cannot remove last admin");
        }

        var role = await roles.GetByNameAsync(roleName) ?? throw new InvalidOperationException("Role not found");
        await users.UpdateUserRoleAsync(userId, role.Id);
        
        var updatedUser = await users.GetByIdAsync(userId);
        return MapToResponse(updatedUser!, role.Name);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId)
    {
        return await roles.GetUserRoleNamesAsync(userId);
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetUsersByRoleAsync(string roleName)
    {
        roleName = roleName?.Trim().ToUpperInvariant() ?? string.Empty;
        var usersInRole = await roles.GetUsersByRoleAsync(roleName);
        return usersInRole.Select(u => MapToResponse(u, roleName)).ToList();
    }

    private UserResponseDto MapToResponse(User u, string roleName)
    {
        return new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.SurName,
            Username = u.UserName,
            Email = u.Email,
            ProfilePicture = cloudinary.GetFullImageUrl(u.Profile?.ProfilePicture ?? string.Empty),
            Phone = u.Profile?.Phone ?? string.Empty,
            Role = roleName,
            Status = u.Status,
            IsEmailVerified = u.UserEmail?.EmailVerified ?? false,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        };
    }
}