using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Auth.UnitTests.Domain.Entities;

public class ApplicationUserTests : TestBase
{
    [Fact]
    public void ApplicationUser_DeveCriarUsuarioComDadosValidos()
    {
        // Arrange
        var nome = Faker.Person.FullName;
        var email = Faker.Person.Email;
        var cpf = Faker.Random.Replace("###.###.###-##");
        var dataNascimento = Faker.Person.DateOfBirth;
        var telefone = Faker.Phone.PhoneNumber();
        var genero = Faker.PickRandom("Masculino", "Feminino", "Outro");
        var cidade = Faker.Address.City();
        var estado = Faker.Address.State();
        var cep = Faker.Random.Replace("#####-###");

        // Act
        var user = new ApplicationUser
        {
            Nome = nome,
            UserName = email,
            Email = email,
            CPF = cpf,
            DataNascimento = dataNascimento,
            Telefone = telefone,
            Genero = genero,
            Cidade = cidade,
            Estado = estado,
            CEP = cep
        };

        // Assert
        user.Should().NotBeNull();
        user.Nome.Should().Be(nome);
        user.Email.Should().Be(email);
        user.CPF.Should().Be(cpf);
        user.DataNascimento.Should().Be(dataNascimento);
        user.Telefone.Should().Be(telefone);
        user.Genero.Should().Be(genero);
        user.Cidade.Should().Be(cidade);
        user.Estado.Should().Be(estado);
        user.CEP.Should().Be(cep);
        user.DataCadastro.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        user.Ativo.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_DeveDefinirDataCadastroAutomaticamente()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.DataCadastro.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ApplicationUser_DeveIniciarAtivoComoTrue()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.Ativo.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_DevePermitirAlterarStatusAtivo()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.Ativo = false;

        // Assert
        user.Ativo.Should().BeFalse();
    }

    [Fact]
    public void ApplicationUser_DevePermitirDefinirRefreshToken()
    {
        // Arrange
        var user = new ApplicationUser();
        var refreshToken = Guid.NewGuid();
        var expiryTime = DateTime.Now.AddHours(1);

        // Act
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiryTime;

        // Assert
        user.RefreshToken.Should().Be(refreshToken);
        user.RefreshTokenExpiryTime.Should().Be(expiryTime);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("abcd")]
    [InlineData("abcde")]
    [InlineData("abcdef")]
    [InlineData("abcdefg")]
    [InlineData("abcdefgh")]
    [InlineData("abcdefghi")]
    [InlineData("abcdefghij")]
    public void ApplicationUser_NomeDeveTerMinimo11Caracteres(string nome)
    {
        // Arrange & Act
        var user = new ApplicationUser { Nome = nome };

        // Assert
        user.Nome.Should().Be(nome);
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("123.456.789-01")]
    public void ApplicationUser_CPFDeveTerTamanhoValido(string cpf)
    {
        // Arrange & Act
        var user = new ApplicationUser { CPF = cpf };

        // Assert
        user.CPF.Should().Be(cpf);
    }

    [Fact]
    public void Deve_herdar_de_IdentityUser_e_possuir_defaults()
    {
        var u = new ApplicationUser();

        u.Should().BeAssignableTo<IdentityUser>();

        // defaults simples
        u.Nome.Should().Be(string.Empty);
        u.CPF.Should().Be(string.Empty);
        u.Telefone.Should().Be(string.Empty);
        u.Genero.Should().Be(string.Empty);
        u.Cidade.Should().Be(string.Empty);
        u.Estado.Should().Be(string.Empty);
        u.CEP.Should().Be(string.Empty);
        u.Foto.Should().BeNull();

        // DataCadastro ~ UtcNow
        var antes = DateTime.Now.AddSeconds(-1);
        u.DataCadastro.Should().BeOnOrAfter(antes);

        // flags/tokens
        u.Ativo.Should().BeTrue();
        u.RefreshToken.Should().BeNull();
        u.RefreshTokenExpiryTime.Should().BeNull();
    }

    [Fact]
    public void DataAnnotations_devem_refletir_regras_declaredas()
    {
        var t = typeof(ApplicationUser);

        // Nome: [Required], [StringLength(100)]
        t.GetProperty(nameof(ApplicationUser.Nome))!
            .GetCustomAttributes(typeof(RequiredAttribute), true).Should().NotBeEmpty();
        t.GetProperty(nameof(ApplicationUser.Nome))!
            .GetCustomAttributes(typeof(StringLengthAttribute), true)
            .Cast<StringLengthAttribute>().Single().MaximumLength.Should().Be(100);

        // DataNascimento: [Required]
        t.GetProperty(nameof(ApplicationUser.DataNascimento))!
            .GetCustomAttributes(typeof(RequiredAttribute), true).Should().NotBeEmpty();

        // CPF: [Required], [StringLength(14, MinimumLength=11)]
        var cpfLen = t.GetProperty(nameof(ApplicationUser.CPF))!
            .GetCustomAttributes(typeof(StringLengthAttribute), true)
            .Cast<StringLengthAttribute>().Single();
        cpfLen.MaximumLength.Should().Be(14);
        cpfLen.MinimumLength.Should().Be(11);
        t.GetProperty(nameof(ApplicationUser.CPF))!
            .GetCustomAttributes(typeof(RequiredAttribute), true).Should().NotBeEmpty();

        // Telefone: [StringLength(20)]
        t.GetProperty(nameof(ApplicationUser.Telefone))!
            .GetCustomAttributes(typeof(StringLengthAttribute), true)
            .Cast<StringLengthAttribute>().Single().MaximumLength.Should().Be(20);

        // Genero: [StringLength(20)]
        t.GetProperty(nameof(ApplicationUser.Genero))!
            .GetCustomAttributes(typeof(StringLengthAttribute), true)
            .Cast<StringLengthAttribute>().Single().MaximumLength.Should().Be(20);

        // Cidade: [StringLength(100)]
        t.GetProperty(nameof(ApplicationUser.Cidade))!
            .GetCustomAttributes(typeof(StringLengthAttribute), true)
            .Cast<StringLengthAttribute>().Single().MaximumLength.Should().Be(100);

        // Estado: [StringLength(50)]
        t.GetProperty(nameof(ApplicationUser.Estado))!
            .GetCustomAttributes(typeof(StringLengthAttribute), true)
            .Cast<StringLengthAttribute>().Single().MaximumLength.Should().Be(50);

        // CEP: [StringLength(10)]
        t.GetProperty(nameof(ApplicationUser.CEP))!
            .GetCustomAttributes(typeof(StringLengthAttribute), true)
            .Cast<StringLengthAttribute>().Single().MaximumLength.Should().Be(10);

        // Foto: [StringLength(500)]
        t.GetProperty(nameof(ApplicationUser.Foto))!
            .GetCustomAttributes(typeof(StringLengthAttribute), true)
            .Cast<StringLengthAttribute>().Single().MaximumLength.Should().Be(500);
    }

    [Fact]
    public void Setters_de_refreshToken_devem_funcionar()
    {
        var u = new ApplicationUser
        {
            RefreshToken = Guid.NewGuid(),
            RefreshTokenExpiryTime = DateTime.Now.AddDays(5)
        };

        u.RefreshToken.Should().NotBeNull();
        u.RefreshTokenExpiryTime.Should().NotBeNull();
        u.RefreshTokenExpiryTime!.Value.Should().BeAfter(DateTime.Now.AddDays(4));
    }
}
