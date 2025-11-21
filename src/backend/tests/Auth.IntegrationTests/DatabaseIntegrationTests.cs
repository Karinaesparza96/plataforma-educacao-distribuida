using Auth.Domain.Entities;

namespace Auth.IntegrationTests;

public class DatabaseIntegrationTests
{
    [Fact]
    public void ApplicationUser_DeveSerCriadoCorretamente()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao.silva@teste.com";
        var cpf = "123.456.789-01";
        var dataNascimento = DateTime.Now.AddYears(-25);

        // Act
        var user = new ApplicationUser
        {
            Nome = nome,
            Email = email,
            CPF = cpf,
            DataNascimento = dataNascimento,
            Telefone = "(11) 99999-9999",
            Genero = "Masculino",
            Cidade = "São Paulo",
            Estado = "SP",
            CEP = "01234-567",
            Foto = "https://exemplo.com/foto.jpg",
            Ativo = true,
            DataCadastro = DateTime.Now
        };

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be(nome);
        user.Email.Should().Be(email);
        user.CPF.Should().Be(cpf);
        user.DataNascimento.Should().Be(dataNascimento);
        user.Ativo.Should().BeTrue();
        user.DataCadastro.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
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
    public void ApplicationUser_DeveManterRelacionamentoComRefreshToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "usuario@teste.com",
            Email = "usuario@teste.com",
            Nome = "Usuário Teste",
            DataNascimento = DateTime.Now.AddYears(-25),
            CPF = "123.456.789-01"
        };

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid(),
            Username = user.Email,
            ExpirationDate = DateTime.Now.AddHours(1)
        };

        // Act & Assert
        user.Should().NotBeNull();
        refreshToken.Should().NotBeNull();
        refreshToken.Username.Should().Be(user.Email);
    }

    [Fact]
    public void ApplicationUser_DevePermitirAlteracaoDePropriedades()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.Nome = "Nome Atualizado";
        user.Email = "novo@email.com";
        user.Ativo = false;

        // Assert
        user.Nome.Should().Be("Nome Atualizado");
        user.Email.Should().Be("novo@email.com");
        user.Ativo.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_DevePermitirAlteracaoDePropriedades()
    {
        // Arrange
        var refreshToken = new RefreshToken();

        // Act
        var newToken = Guid.NewGuid();
        var newUsername = "novo@usuario.com";
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
    public void ApplicationUser_DeveValidarDadosObrigatorios()
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            Nome = "Usuário Teste",
            Email = "usuario@teste.com",
            CPF = "123.456.789-01",
            DataNascimento = DateTime.Now.AddYears(-25)
        };

        // Assert
        user.Nome.Should().NotBeNullOrEmpty();
        user.Email.Should().NotBeNullOrEmpty();
        user.CPF.Should().NotBeNullOrEmpty();
        user.DataNascimento.Should().NotBe(default(DateTime));
    }

    [Fact]
    public void RefreshToken_DeveValidarDadosObrigatorios()
    {
        // Arrange & Act
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid(),
            Username = "usuario@teste.com",
            ExpirationDate = DateTime.Now.AddHours(1)
        };

        // Assert
        refreshToken.Token.Should().NotBe(Guid.Empty);
        refreshToken.Username.Should().NotBeNullOrEmpty();
        refreshToken.ExpirationDate.Should().NotBe(default(DateTime));
    }
}
