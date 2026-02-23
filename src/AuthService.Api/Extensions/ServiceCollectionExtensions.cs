using AuthService.Domain.Entitis;
using AuthService.Domain.Constants;
using AuthService.Persistence.Data;
using Npgsql.Replication;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());
        
        services.AddHealthChecks();

        return services;
    }
}