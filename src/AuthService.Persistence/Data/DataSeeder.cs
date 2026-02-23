using AuthService.Domain.Entitis;
using AuthService.Application.Services;
using AuthService.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Verificar si ya existen roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new() {
                    Id = UuidGenerator.GenerateRoleId(),
                        Name = "ADMIN"
                },
                new() {
                    Id = UuidGenerator.GenerateRoleId(),
                        Name = "USER"
                }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }
        // Seed de un usuario administrador por defecto SOLO si no existen usuarios todavía
        if (!await context.Users.AnyAsync())
        {
            // Buscar rol admin existente
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "ADMIN");
            if (adminRole != null)
            {
                //var passwordHasher = new PasswordHashService();
                var userId = UuidGenerator.GenerateUserId();
                var profileId = UuidGenerator.GenerateUserId();
                var emailId = UuidGenerator.GenerateUserId();
                var userRoleId = UuidGenerator.GenerateUserId();
                var adminUser = new User
                {
                    Id = userId,
                    Name = "Admin",
                    SurName = "User",
                    UserName = "admin",
                    Email = "admin@ksports.local",
                    Password = "12345678",
                    Status = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Profile = new UserProfile
                    {
                        Id = profileId,
                        UserId = userId,
                    },
                    UserEmail = new UserEmail
                    {
                        Id = emailId,
                        UserId = userId,
                        Email = "admin@ksports.local",
                        EmailVerified = true,
                        EmailVerificationToken = string.Empty,
                        EmailVerificationTokenExpiry = DateTime.UtcNow.AddYears(100)
                    },
                    UserRoles =
                    [
                        new UserRole
                        {
                            Id = userRoleId,
                            UserId = userId,
                            RoleId = adminRole.Id
                        }
                    ]
                };
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
