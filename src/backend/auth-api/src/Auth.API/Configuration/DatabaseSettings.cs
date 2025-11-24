using System.Diagnostics.CodeAnalysis;

namespace Auth.API.Configuration;

[ExcludeFromCodeCoverage]
public class DatabaseSettings
{
    public string DefaultConnection { get; set; } = string.Empty;
    public string DevelopmentConnection { get; set; } = "Server=localhost;Database=AuthDB;Trusted_Connection=true;TrustServerCertificate=true;";
    public string ProductionConnection { get; set; } = "Server=localhost;Database=AuthDB;Trusted_Connection=true;TrustServerCertificate=true;";
}
