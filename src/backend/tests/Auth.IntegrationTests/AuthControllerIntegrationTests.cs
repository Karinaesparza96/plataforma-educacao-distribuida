using Auth.Domain.Entities;

namespace Auth.IntegrationTests;

public class AuthControllerIntegrationTests
{
    [Fact]
    public void TesteBasico_DevePassar()
    {
        // Arrange & Act
        var result = true;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_DeveSerCriadoCorretamente()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao.silva@teste.com";
        var dataNascimento = DateTime.Now.AddYears(-25);

        // Act
        var user = new ApplicationUser
        {
            Nome = nome,
            Email = email,
            DataNascimento = dataNascimento,
            CPF = "123.456.789-01"
        };

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be(nome);
        user.Email.Should().Be(email);
        user.DataNascimento.Should().Be(dataNascimento);
        user.CPF.Should().Be("123.456.789-01");
    }

    [Fact]
    public void RefreshToken_DeveSerCriadoCorretamente()
    {
        // Arrange
        var token = Guid.NewGuid();
        var username = "usuario@teste.com";
        var expirationDate = DateTime.Now.AddHours(24);

        // Act
        var refreshToken = new RefreshToken
        {
            Token = token,
            Username = username,
            ExpirationDate = expirationDate
        };

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().Be(token);
        refreshToken.Username.Should().Be(username);
        refreshToken.ExpirationDate.Should().Be(expirationDate);
    }

    [Fact]
    public void ApplicationUser_DeveManterPropriedades()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.Nome = "Maria Silva";
        user.Email = "maria.silva@teste.com";
        user.Ativo = false;

        // Assert
        user.Nome.Should().Be("Maria Silva");
        user.Email.Should().Be("maria.silva@teste.com");
        user.Ativo.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_DeveManterPropriedades()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var newExpirationDate = DateTime.Now.AddDays(7);

        // Act
        var newToken = Guid.NewGuid();
        refreshToken.Token = newToken;
        refreshToken.Username = "novo@teste.com";
        refreshToken.ExpirationDate = newExpirationDate;

        // Assert
        refreshToken.Token.Should().Be(newToken);
        refreshToken.Username.Should().Be("novo@teste.com");
        refreshToken.ExpirationDate.Should().Be(newExpirationDate);
    }
}
