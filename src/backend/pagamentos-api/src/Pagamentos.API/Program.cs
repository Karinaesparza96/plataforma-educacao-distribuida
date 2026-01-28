using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Pagamentos.API.Configuration;
using Pagamentos.API.Configuration.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApiConfig()
    .AddCorsConfig()
    .AddSwaggerConfig()
    .AddMessageBusConfiguration(builder.Configuration)
    .AddResilienceConfiguration()
    .AddDbContextConfig()
    .AddJwtConfiguration()
    .AddMapsterConfiguration()
    .AddMediatrConfig()
    .AddDependencyInjectionConfig();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerConfig(apiVersionDescriptionProvider);
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", DateTime = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithOpenApi();

app.UseDbMigrationHelper();

app.Run();
