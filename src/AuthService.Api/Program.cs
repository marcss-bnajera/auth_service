using AuthService.Persistence.Data;
using AuthService.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- INICIALIZACIÓN AUTOMÁTICA DE LA BASE DE DATOS ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Iniciando la inicialización de la base de datos...");

        // 1. FORZAR CREACIÓN DE LA BASE DE DATOS FÍSICA
        // Esto crea el "archivo" auth_db en Postgres si no existe.
        var databaseCreator = context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
        if (databaseCreator != null)
        {
            if (!databaseCreator.Exists())
            {
                logger.LogInformation("La base de datos no existe. Creándola...");
                await databaseCreator.CreateAsync();
            }
        }

        // 2. APLICAR MIGRACIONES (Crear tablas)
        logger.LogInformation("Aplicando migraciones...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migración completada exitosamente.");

        // 3. CARGAR DATOS INICIALES (Seed)
        await DataSeeder.SeedAsync(context);
        logger.LogInformation("Datos iniciales cargados exitosamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error crítico al inicializar la base de datos.");
        // Opcional: throw ex; si quieres que la app no arranque si falla la BD
    }
}

// Minimal API de ejemplo
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}