namespace Conteudo.API.Configuration;

public sealed class ResilienceSettings
{
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerThreshold { get; set; } = 3;
    public int CircuitBreakerDurationSeconds { get; set; } = 5;
}
