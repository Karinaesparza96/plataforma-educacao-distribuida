using Auth.Domain.Entities;

namespace Auth.UnitTests.Domain.Entities;

public class RefreshTokenTests : TestBase
{
    [Fact]
    public void RefreshToken_DeveSerCriadoComSucesso()
    {
        // Arrange
        var token = Guid.NewGuid();
        var username = Faker.Person.Email;
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
    public void RefreshToken_DevePermitirAlteracaoDePropriedades()
    {
        // Arrange
        var refreshToken = new RefreshToken();

        // Act
        var newToken = Guid.NewGuid();
        var newUsername = Faker.Person.Email;
        var newExpirationDate = DateTime.Now.AddDays(7);

        refreshToken.Token = newToken;
        refreshToken.Username = newUsername;
        refreshToken.ExpirationDate = newExpirationDate;

        // Assert
        refreshToken.Token.Should().Be(newToken);
        refreshToken.Username.Should().Be(newUsername);
        refreshToken.ExpirationDate.Should().Be(newExpirationDate);
    }

    [Fact]
    public void RefreshToken_DeveAceitarUsernameVazio()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Username = string.Empty
        };

        // Assert
        refreshToken.Username.Should().Be(string.Empty);
    }

    [Fact]
    public void RefreshToken_DeveAceitarUsernameNull()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Username = null!
        };

        // Assert
        refreshToken.Username.Should().BeNull();
    }

    [Fact]
    public void RefreshToken_DeveAceitarDataExpiracaoNoPassado()
    {
        // Arrange
        var pastDate = DateTime.Now.AddHours(-1);

        // Act
        var refreshToken = new RefreshToken
        {
            ExpirationDate = pastDate
        };

        // Assert
        refreshToken.ExpirationDate.Should().Be(pastDate);
    }

    [Fact]
    public void RefreshToken_DeveAceitarDataExpiracaoNoFuturo()
    {
        // Arrange
        var futureDate = DateTime.Now.AddHours(1);

        // Act
        var refreshToken = new RefreshToken
        {
            ExpirationDate = futureDate
        };

        // Assert
        refreshToken.ExpirationDate.Should().Be(futureDate);
    }

    [Fact]
    public void RefreshToken_DeveAceitarDataExpiracaoAgora()
    {
        // Arrange
        var now = DateTime.Now;

        // Act
        var refreshToken = new RefreshToken
        {
            ExpirationDate = now
        };

        // Assert
        refreshToken.ExpirationDate.Should().Be(now);
    }

    [Theory]
    [InlineData("usuario@teste.com")]
    [InlineData("admin@empresa.com.br")]
    [InlineData("teste123@dominio.org")]
    [InlineData("a@b.c")]
    [InlineData("usuario+tag@exemplo.com")]
    public void RefreshToken_DeveAceitarDiferentesFormatosDeEmail(string email)
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Username = email
        };

        // Assert
        refreshToken.Username.Should().Be(email);
    }

    [Fact]
    public void RefreshToken_DeveSerComparavelPorToken()
    {
        // Arrange
        var token1 = Guid.NewGuid();
        var token2 = Guid.NewGuid();
        var token3 = Guid.NewGuid();

        var refreshToken1 = new RefreshToken { Token = token1 };
        var refreshToken2 = new RefreshToken { Token = token2 };
        var refreshToken3 = new RefreshToken { Token = token3 };

        // Act & Assert
        refreshToken1.Token.Should().NotBe(refreshToken2.Token);
        refreshToken2.Token.Should().NotBe(refreshToken3.Token);
        refreshToken1.Token.Should().NotBe(refreshToken3.Token);
    }

    [Fact]
    public void RefreshToken_DevePermitirCriacaoComConstrutorPadrao()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken();

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().NotBe(Guid.Empty);
        refreshToken.Id.Should().NotBe(Guid.Empty);
        refreshToken.Username.Should().BeNull();
        refreshToken.ExpirationDate.Should().Be(default);
    }

    [Fact]
    public void RefreshToken_DevePermitirCriacaoComPropriedadesIniciais()
    {
        // Arrange
        var token = Guid.NewGuid();
        var username = Faker.Person.Email;
        var expirationDate = DateTime.Now.AddHours(12);

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
    public void Ctor_deve_gerar_Id_e_Token()
    {
        var t = new RefreshToken();

        t.Id.Should().NotBe(Guid.Empty);
        t.Token.Should().NotBe(Guid.Empty);

        // defaults
        t.Username.Should().BeNull();
        t.ExpirationDate.Should().Be(default);
    }

    [Fact]
    public void Setters_devem_preencher_campos()
    {
        var t = new RefreshToken
        {
            Username = "user",
            ExpirationDate = DateTime.Now.AddDays(7)
        };

        t.Username.Should().Be("user");
        t.ExpirationDate.Should().BeAfter(DateTime.Now.AddDays(6)).And.BeBefore(DateTime.Now.AddDays(8));
    }
}
