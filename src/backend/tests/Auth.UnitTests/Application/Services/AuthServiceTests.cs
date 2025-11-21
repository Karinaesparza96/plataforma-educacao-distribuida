using System.Text;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Application.Settings;
using Auth.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core.Interfaces;
using System.Security.Claims;

namespace Auth.UnitTests.Application.Services;
public class AuthServiceTests
{
    // ---------------------------
    // Helpers de infraestrutura
    // ---------------------------

    private class FakeEfContext(DbContextOptions<AuthServiceTests.FakeEfContext> options) : DbContext(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    }

    // Cria um AuthService com dependências mockadas e um DbContext InMemory por trás do IAuthDbContext
    private static AuthService CriarSut(
        out Mock<UserManager<ApplicationUser>> userManager,
        out Mock<SignInManager<ApplicationUser>> signInManager,
        out Mock<IJwtService> jwt,
        out Mock<IAuthDbContext> authDb,
        out FakeEfContext ef,
        out IHttpContextAccessor accessor,
        int refreshTokenExpirationHours = 2)
    {
        // UserManager/SignInManager
        userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

        // EF InMemory por trás do IAuthDbContext (só precisamos de RefreshTokens + SaveChangesAsync)
        var options = new DbContextOptionsBuilder<FakeEfContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ef = new FakeEfContext(options);
        ef.Database.EnsureCreated();
        var efLocal = ef;

        authDb = new Mock<IAuthDbContext>();
        authDb.SetupGet(c => c.RefreshTokens).Returns(ef.RefreshTokens);
        authDb.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
              .Returns<CancellationToken>(ct => efLocal.SaveChangesAsync(ct));

        // JwtService devolve credencial de assinatura
        jwt = new Mock<IJwtService>();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super_secret_key_for_tests_123456789"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        jwt.Setup(j => j.GetCurrentSigningCredentials()).ReturnsAsync(creds);

        // HttpContextAccessor com Scheme/Host para definir Issuer
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("api.local");
        accessor = new HttpContextAccessor { HttpContext = httpContext };

        // AppTokenSettings
        var opts = Options.Create(new AppTokenSettings { RefreshTokenExpiration = refreshTokenExpirationHours });

        return new AuthService(
            userManager.Object,
            signInManager.Object,
            authDb.Object,
            jwt.Object,
            accessor,
            opts);
    }

    // --------------------------------
    //           TESTES
    // --------------------------------

    [Fact]
    public async Task GerarJwt_deve_criar_token_e_refreshToken_e_remover_antigos()
    {
        // Arrange
        var sut = CriarSut(out var um, out var sm, out var jwt, out var authDb, out var ef, out var accessor);

        var email = "user@test.com";
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = email };

        // pré-carrega tokens antigos do mesmo usuário para garantir remoção
        ef.RefreshTokens.AddRange(
        [
            new RefreshToken { Username = email, ExpirationDate = DateTime.UtcNow.AddHours(1) },
            new RefreshToken { Username = email, ExpirationDate = DateTime.UtcNow.AddHours(2) },
            new RefreshToken { Username = "outro@x.com", ExpirationDate = DateTime.UtcNow.AddHours(2) }
        ]);
        await ef.SaveChangesAsync();

        // UserManager: usuário, claims iniciais e roles
        var baseClaims = new List<Claim> { new("custom", "v1") };
        um.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(user);
        um.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(baseClaims);
        um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["Admin", "Student"]);

        // Act
        var dto = await sut.GerarJwt(email);

        // Assert — AccessToken e DTO básicos
        dto.Should().NotBeNull();
        dto.AccessToken.Should().NotBeNullOrWhiteSpace();
        dto.RefreshToken.Should().NotBe(Guid.Empty);
        dto.ExpiresIn.Should().Be(TimeSpan.FromHours(1).TotalSeconds);

        dto.UsuarioToken.Should().NotBeNull();
        dto.UsuarioToken.Id.Should().Be(user.Id);
        dto.UsuarioToken.Email.Should().Be(user.Email);

        // IMPORTANTE: UsuarioToken.Claims vem dos claims ORIGINAIS do UserManager (não inclui roles adicionadas internamente)
        dto.UsuarioToken.Claims.Should().ContainSingle(c => c.Type == "custom" && c.Value == "v1");

        // RefreshTokens: removeu os antigos do mesmo usuário e deixou só 1 (o novo)
        var tokensDoUsuario = ef.RefreshTokens.Where(t => t.Username == email).ToList();
        tokensDoUsuario.Should().HaveCount(1);
        tokensDoUsuario.Single().Token.Should().Be(dto.RefreshToken);

        // Garantia de que roles foram consultadas (ObterClaimsUsuario foi executado)
        um.Verify(m => m.GetRolesAsync(user), Times.Once);
        jwt.Verify(j => j.GetCurrentSigningCredentials(), Times.Once);
    }

    [Fact]
    public async Task ObterRefreshToken_deve_retornar_token_quando_nao_expirado()
    {
        // Arrange
        var sut = CriarSut(out var _, out var _, out var _, out var _, out var ef, out _);

        var token = new RefreshToken
        {
            Username = "x@y.com",
            ExpirationDate = DateTime.Now.AddMinutes(10) // não expirado
        };
        ef.RefreshTokens.Add(token);
        await ef.SaveChangesAsync();

        // Act
        var found = await sut.ObterRefreshToken(token.Token);

        // Assert
        found.Should().NotBeNull();
        found!.Token.Should().Be(token.Token);
    }

    [Fact]
    public async Task ObterRefreshToken_deve_retornar_null_quando_expirado()
    {
        // Arrange
        var sut = CriarSut(out var _, out var _, out var _, out var _, out var ef, out var _);

        var token = new RefreshToken
        {
            Username = "x@y.com",
            ExpirationDate = DateTime.Now.AddMinutes(-1) // expirado
        };
        ef.RefreshTokens.Add(token);
        await ef.SaveChangesAsync();

        // Act
        var found = await sut.ObterRefreshToken(token.Token);

        // Assert
        found.Should().BeNull();
    }
}
