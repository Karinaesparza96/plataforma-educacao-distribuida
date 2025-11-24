using Auth.API.Configuration;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Auth.API.Extensions;

[ExcludeFromCodeCoverage]
public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var databaseSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>() ?? new DatabaseSettings();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var isDevelopment = environment.IsDevelopment() || environment.IsEnvironment("Docker");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(connectionString ?? databaseSettings.ProductionConnection,
                b => b.MigrationsAssembly("Auth.Infrastructure")));

        return services;
    }
}
