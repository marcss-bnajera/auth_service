using AuthService.Domain.Entitis;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserEmail> UserEmails { get; set; }
    public DbSet<UserPasswordReset> UserPasswordResets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Esto es VITAL para que EF no se confunda con los nombres de las tablas del sistema
        optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__ef_migrations_history"));
        
        // Esto le dice que use snake_case de forma profesional (sin métodos manuales)
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de relaciones
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique(); 

            entity.HasOne(e => e.Profile).WithOne(p => p.User).HasForeignKey<UserProfile>(p => p.UserId);
            entity.HasMany(e => e.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId);
            entity.HasOne(e => e.UserEmail).WithOne(ue => ue.User).HasForeignKey<UserEmail>(ue => ue.UserId);
            entity.HasOne(e => e.PasswordReset).WithOne(upr => upr.User).HasForeignKey<UserPasswordReset>(upr => upr.UserId);
        });

        modelBuilder.Entity<UserRole>().HasKey(e => e.Id);
        modelBuilder.Entity<Role>().HasKey(e => e.Id);
    }
}