using BFF.API.Configuration;
using BFF.API.Extensions;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddApiConfiguration();

        builder.Services.AddJsonConfiguration();

        builder.Services.AddAuthorization();

        builder.Services.AddHealthChecks();

        var urls = builder.Configuration["Urls"];
        if (!string.IsNullOrEmpty(urls))
        {
            builder.WebHost.UseUrls(urls);
        }

        var app = builder.Build();

        if (!app.Environment.IsProduction())
        {
            app.UseSwaggerConfiguration();
        }

        app.UseCors("AllowedOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
