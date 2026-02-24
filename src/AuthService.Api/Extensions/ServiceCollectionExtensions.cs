using AuthService.Domain.Entitis;
using AuthService.Domain.Constants;
using AuthService.Persistence.Data;
using Npgsql.Replication;
using Microsoft.EntityFrameworkCore;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;

namespace AuthService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => 
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), 
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history")) // <--- IMPORTANTE
        .UseSnakeCaseNamingConvention());

        services.AddScoped<IEmailService, EmailService>();
        services.AddHealthChecks();
        return services;
    }
}