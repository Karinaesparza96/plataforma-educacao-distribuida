using Core.DomainObjects;

namespace Core.Tests.DomainObjects;

public class EntidadeTests
{
    private class EntidadeTeste : Entidade
    {
        public string Nome { get; set; } = string.Empty;
    }

    [Fact]
    public void Entidade_DeveCriarComIdAutomatico()
    {
        // Arrange & Act
        var entidade = new EntidadeTeste();

        // Assert
        entidade.Should().NotBeNull();
        entidade.Id.Should().NotBe(Guid.Empty);
        entidade.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        entidade.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Entidade_DeveDefinirIdManual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entidade = new EntidadeTeste();

        // Act
        entidade.DefinirId(id);

        // Assert
        entidade.Id.Should().Be(id);
    }

    [Fact]
    public void Entidade_DeveAtualizarDataModificacao()
    {
        // Arrange
        var entidade = new EntidadeTeste();
        var dataOriginal = entidade.UpdatedAt;

        // Act
        entidade.AtualizarDataModificacao();

        // Assert
        entidade.UpdatedAt.Should().NotBeNull();
        entidade.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Entidade_DeveSerIgualPorId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entidade1 = new EntidadeTeste();
        var entidade2 = new EntidadeTeste();

        entidade1.DefinirId(id);
        entidade2.DefinirId(id);

        // Act & Assert
        entidade1.Should().Be(entidade2);
        entidade1.GetHashCode().Should().Be(entidade2.GetHashCode());
    }

    [Fact]
    public void Entidade_DeveSerDiferentePorId()
    {
        // Arrange
        var entidade1 = new EntidadeTeste();
        var entidade2 = new EntidadeTeste();

        // Act & Assert
        entidade1.Should().NotBe(entidade2);
        entidade1.GetHashCode().Should().NotBe(entidade2.GetHashCode());
    }

    [Fact]
    public void Entidade_DeveSerDiferenteDeNull()
    {
        // Arrange
        var entidade = new EntidadeTeste();

        // Act & Assert
        entidade.Should().NotBeNull();
        entidade.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Entidade_DeveSerDiferenteDeTipoDiferente()
    {
        // Arrange
        var entidade = new EntidadeTeste();
        var outroObjeto = new object();

        // Act & Assert
        entidade.Equals(outroObjeto).Should().BeFalse();
    }

    [Fact]
    public void Ctor_deve_definir_Id_e_CreatedAt_e_UpdatedAt_nulo()
    {
        var now = DateTime.Now;
        var e = new EntidadeTeste();

        e.Id.Should().NotBe(Guid.Empty);
        e.CreatedAt.Should().BeOnOrAfter(now).And.BeOnOrBefore(DateTime.Now.AddSeconds(1));
        e.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void DefinirId_deve_substituir_Id()
    {
        var e = new EntidadeTeste();
        var novoId = Guid.NewGuid();

        e.DefinirId(novoId);
        e.Id.Should().Be(novoId);
    }

    [Fact]
    public async Task AtualizarDataModificacao_deve_setar_UpdatedAt_para_UtcNow()
    {
        var e = new EntidadeTeste();
        await Task.Delay(5); // garante diferença mínima
        e.AtualizarDataModificacao();

        e.UpdatedAt.Should().NotBeNull();
        e.UpdatedAt!.Value.Should().BeOnOrBefore(DateTime.Now);
    }
}
