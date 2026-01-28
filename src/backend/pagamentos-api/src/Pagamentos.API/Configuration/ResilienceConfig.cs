using Polly;
using System.Net;

namespace Pagamentos.API.Configuration;

public static class ResilienceConfig
{
    public static WebApplicationBuilder AddResilienceConfiguration(this WebApplicationBuilder builder)
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
