using Alunos.API.Configurations;
using Alunos.API.Filters;
using Alunos.Application.Commands.AtualizarPagamento;
using Core.Identidade;
using Mapster;
using Polly;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace Alunos.API.Configurations;

[ExcludeFromCodeCoverage]
public static class ApiConfiguration
{
    public static WebApplicationBuilder AddApiConfiguration(this WebApplicationBuilder builder)
    {
        return builder.AddConfiguration()
            .AddDbContextConfiguration()
            .AddControllersConfiguration()
            .AddHttpContextAccessorConfiguration()
            .AddCorsConfiguration()
            .AddMediatRConfiguration()
            .AddServicesConfiguration()
            .AddMapsterConfiguration()
            .AddJwtConfiguration()
            .AddResilienceConfiguration()
            .AddSwaggerConfigurationExtension();
    }

    private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables();
        return builder;
    }

    private static WebApplicationBuilder AddControllersConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<DomainExceptionFilter>();
            options.Filters.Add<ExceptionFilter>();
        }).ConfigureApiBehaviorOptions(opt =>
        {
            opt.SuppressModelStateInvalidFilter = true;
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = false;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

        return builder;
    }

    private static WebApplicationBuilder AddHttpContextAccessorConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        return builder;
    }

    private static WebApplicationBuilder AddCorsConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
        return builder;
    }

    private static WebApplicationBuilder AddMediatRConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(AtualizarPagamentoMatriculaCommand).Assembly));
        return builder;
    }

    private static WebApplicationBuilder AddServicesConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.RegisterServices();
        return builder;
    }

    private static WebApplicationBuilder AddMapsterConfiguration(this WebApplicationBuilder builder)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
        return builder;
    }

    private static WebApplicationBuilder AddSwaggerConfigurationExtension(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerConfiguration();
        return builder;
    }

    private static WebApplicationBuilder AddJwtConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddJwtConfiguration(builder.Configuration);
        return builder;
    }

    private static WebApplicationBuilder AddResilienceConfiguration(this WebApplicationBuilder builder)
    {
        var settings = builder.Configuration.GetSection("ResilienceSettings").Get<ResilienceSettings>() ?? new ResilienceSettings();

        builder.Services.AddHttpClient("Default")
            .AddPolicyHandler(GetRetryPolicy(settings))
            .AddPolicyHandler(GetCircuitBreakerPolicy(settings));

        return builder;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ResilienceSettings settings)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(r =>
                r.StatusCode == HttpStatusCode.BadGateway ||
                r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == HttpStatusCode.GatewayTimeout ||
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                (int)r.StatusCode == 429)
            .WaitAndRetryAsync(
                settings.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ResilienceSettings settings)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(r =>
                r.StatusCode == HttpStatusCode.BadGateway ||
                r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == HttpStatusCode.GatewayTimeout ||
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                (int)r.StatusCode == 429)
            .CircuitBreakerAsync(
                settings.CircuitBreakerThreshold,
                TimeSpan.FromSeconds(settings.CircuitBreakerDurationSeconds));
    }
}
