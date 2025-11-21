using BFF.API.Models.Response;
using FluentAssertions;

namespace BFF.UnitTests;

public class BffApiTests
{
    [Fact]
    public void HealthCheckResponse_DeveSerCriadoCorretamente()
    {
        // Arrange
        var status = "Healthy";
        var timestamp = DateTime.Now;
        var version = "1.0.0";
        var environment = "Development";

        // Act
        var response = new HealthCheckResponse
        {
            Status = status,
            Timestamp = timestamp,
            Version = version,
            Environment = environment,
            Services = new List<ServiceHealthResponse>
            {
                new() { Name = "Test Service", Status = "Healthy", ResponseTime = "5ms", LastCheck = timestamp }
            }
        };

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be(status);
        response.Timestamp.Should().Be(timestamp);
        response.Version.Should().Be(version);
        response.Environment.Should().Be(environment);
        response.Services.Should().HaveCount(1);
        response.Services.First().Name.Should().Be("Test Service");
    }

    [Fact]
    public void ServiceHealthResponse_DeveSerCriadoCorretamente()
    {
        // Arrange
        var name = "Test Service";
        var status = "Healthy";
        var responseTime = "10ms";
        var lastCheck = DateTime.Now;

        // Act
        var service = new ServiceHealthResponse
        {
            Name = name,
            Status = status,
            ResponseTime = responseTime,
            LastCheck = lastCheck
        };

        // Assert
        service.Should().NotBeNull();
        service.Name.Should().Be(name);
        service.Status.Should().Be(status);
        service.ResponseTime.Should().Be(responseTime);
        service.LastCheck.Should().Be(lastCheck);
    }

    [Fact]
    public void ApiStatusResponse_DeveSerCriadoCorretamente()
    {
        // Arrange
        var name = "Test API";
        var version = "2.0.0";
        var environment = "Production";
        var startTime = DateTime.Now.AddHours(-2);
        var uptime = TimeSpan.FromHours(2);
        var status = "Running";

        // Act
        var response = new ApiStatusResponse
        {
            Name = name,
            Version = version,
            Environment = environment,
            StartTime = startTime,
            Uptime = uptime,
            Status = status,
            Configuration = new Dictionary<string, object>
            {
                { "LogLevel", "Warning" },
                { "Caching", "Disabled" }
            }
        };

        // Assert
        response.Should().NotBeNull();
        response.Name.Should().Be(name);
        response.Version.Should().Be(version);
        response.Environment.Should().Be(environment);
        response.StartTime.Should().Be(startTime);
        response.Uptime.Should().Be(uptime);
        response.Status.Should().Be(status);
        response.Configuration.Should().HaveCount(2);
    }

    [Fact]
    public void HealthCheckResponse_ComDadosMinimos_DeveSerCriado()
    {
        // Arrange & Act
        var response = new HealthCheckResponse
        {
            Status = "Unhealthy",
            Timestamp = DateTime.Now,
            Version = "0.1.0",
            Environment = "Test",
            Services = new List<ServiceHealthResponse>()
        };

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be("Unhealthy");
        response.Version.Should().Be("0.1.0");
        response.Environment.Should().Be("Test");
        response.Services.Should().HaveCount(0);
    }

    [Fact]
    public void ServiceHealthResponse_ComDadosMinimos_DeveSerCriado()
    {
        // Arrange & Act
        var service = new ServiceHealthResponse
        {
            Name = "Minimal Service",
            Status = "Unknown",
            ResponseTime = "0ms",
            LastCheck = DateTime.MinValue
        };

        // Assert
        service.Should().NotBeNull();
        service.Name.Should().Be("Minimal Service");
        service.Status.Should().Be("Unknown");
        service.ResponseTime.Should().Be("0ms");
        service.LastCheck.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void ApiStatusResponse_ComDadosMinimos_DeveSerCriado()
    {
        // Arrange & Act
        var response = new ApiStatusResponse
        {
            Name = "Minimal API",
            Version = "0.0.1",
            Environment = "Local",
            StartTime = DateTime.Now,
            Uptime = TimeSpan.Zero,
            Status = "Starting",
            Configuration = new Dictionary<string, object>()
        };

        // Assert
        response.Should().NotBeNull();
        response.Name.Should().Be("Minimal API");
        response.Version.Should().Be("0.0.1");
        response.Environment.Should().Be("Local");
        response.Status.Should().Be("Starting");
        response.Configuration.Should().HaveCount(0);
    }

    [Fact]
    public void HealthCheckResponse_ComMultiplosServicos_DeveSerCriado()
    {
        // Arrange
        var services = new List<ServiceHealthResponse>
        {
            new() { Name = "Service 1", Status = "Healthy", ResponseTime = "5ms", LastCheck = DateTime.Now },
            new() { Name = "Service 2", Status = "Unhealthy", ResponseTime = "100ms", LastCheck = DateTime.Now.AddMinutes(-5) },
            new() { Name = "Service 3", Status = "Degraded", ResponseTime = "50ms", LastCheck = DateTime.Now.AddMinutes(-1) }
        };

        // Act
        var response = new HealthCheckResponse
        {
            Status = "Degraded",
            Timestamp = DateTime.Now,
            Version = "1.0.0",
            Environment = "Production",
            Services = services
        };

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be("Degraded");
        response.Services.Should().HaveCount(3);
        response.Services.Should().Contain(s => s.Name == "Service 1" && s.Status == "Healthy");
        response.Services.Should().Contain(s => s.Name == "Service 2" && s.Status == "Unhealthy");
        response.Services.Should().Contain(s => s.Name == "Service 3" && s.Status == "Degraded");
    }

    [Fact]
    public void ApiStatusResponse_ComConfiguracaoCompleta_DeveSerCriado()
    {
        // Arrange
        var configuration = new Dictionary<string, object>
        {
            { "LogLevel", "Information" },
            { "Caching", "Enabled" },
            { "Compression", "Enabled" },
            { "Database", "Connected" },
            { "Redis", "Connected" },
            { "MessageBus", "Connected" }
        };

        // Act
        var response = new ApiStatusResponse
        {
            Name = "Full API",
            Version = "2.1.0",
            Environment = "Staging",
            StartTime = DateTime.Now.AddDays(-1),
            Uptime = TimeSpan.FromDays(1),
            Status = "Running",
            Configuration = configuration
        };

        // Assert
        response.Should().NotBeNull();
        response.Name.Should().Be("Full API");
        response.Version.Should().Be("2.1.0");
        response.Environment.Should().Be("Staging");
        response.Status.Should().Be("Running");
        response.Configuration.Should().HaveCount(6);
        response.Configuration.Should().ContainKey("LogLevel");
        response.Configuration.Should().ContainKey("Caching");
        response.Configuration.Should().ContainKey("Compression");
        response.Configuration.Should().ContainKey("Database");
        response.Configuration.Should().ContainKey("Redis");
        response.Configuration.Should().ContainKey("MessageBus");
    }
}
