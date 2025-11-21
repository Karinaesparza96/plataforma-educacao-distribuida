using Auth.Domain.Entities;

namespace Auth.UnitTests.API;

public class AuthControllerTests : TestBase
{
    [Fact]
    public void ApplicationUser_DeveSerCriadoCorretamente()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@teste.com";
        var userName = "joao.silva";

        // Act
        var user = new ApplicationUser
        {
            Nome = nome,
            Email = email,
            UserName = userName,
            EmailConfirmed = true
        };

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be(nome);
        user.Email.Should().Be(email);
        user.UserName.Should().Be(userName);
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_ComDadosMinimos_DeveSerCriado()
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            Nome = "Usuário Teste",
            Email = "teste@email.com"
        };

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be("Usuário Teste");
        user.Email.Should().Be("teste@email.com");
        user.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_DeveSerCriadoCorretamente()
    {
        // Arrange
        var token = Guid.NewGuid();
        var username = "joao@teste.com";
        var expirationDate = DateTime.Now.AddDays(7);

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
    public void RefreshToken_ComDadosMinimos_DeveSerCriado()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Username = "teste@email.com"
        };

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().NotBeEmpty();
        refreshToken.Username.Should().Be("teste@email.com");
        refreshToken.ExpirationDate.Should().Be(default(DateTime));
    }

    [Fact]
    public void ApplicationUser_ComDadosCompletos_DeveSerCriado()
    {
        // Arrange
        var nome = "Maria Santos";
        var email = "maria@teste.com";
        var userName = "maria.santos";
        var phoneNumber = "11987654321";
        var lockoutEnd = DateTime.Now.AddMinutes(30);

        // Act
        var user = new ApplicationUser
        {
            Nome = nome,
            Email = email,
            UserName = userName,
            EmailConfirmed = true,
            PhoneNumber = phoneNumber,
            LockoutEnd = lockoutEnd,
            AccessFailedCount = 3
        };

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be(nome);
        user.Email.Should().Be(email);
        user.UserName.Should().Be(userName);
        user.EmailConfirmed.Should().BeTrue();
        user.PhoneNumber.Should().Be(phoneNumber);
        user.LockoutEnd.Should().Be(lockoutEnd);
        user.AccessFailedCount.Should().Be(3);
    }

    [Fact]
    public void RefreshToken_ComDadosCompletos_DeveSerCriado()
    {
        // Arrange
        var token = Guid.NewGuid();
        var username = "admin@teste.com";
        var expirationDate = DateTime.Now.AddDays(30);

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
    public void ApplicationUser_ComDadosNulos_DeveSerCriado()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be(string.Empty);
        user.Email.Should().BeNull();
        user.UserName.Should().BeNull();
        user.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_ComDadosNulos_DeveSerCriado()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken();

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().NotBeEmpty();
        refreshToken.Username.Should().BeNull();
        refreshToken.ExpirationDate.Should().Be(default(DateTime));
    }

    [Fact]
    public void ApplicationUser_ComDadosVazios_DeveSerCriado()
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            Nome = "",
            Email = "",
            UserName = ""
        };

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be("");
        user.Email.Should().Be("");
        user.UserName.Should().Be("");
    }

    [Fact]
    public void RefreshToken_ComDadosVazios_DeveSerCriado()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Username = ""
        };

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Username.Should().Be("");
    }
}
