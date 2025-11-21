using Core.Messages;
using FluentValidation.Results;

namespace Core.Tests.Messages;

public class CommandRaizTests 
{
    private class ComandoTeste : RaizCommand
    {
        public string Nome { get; set; } = string.Empty;
    }

    [Fact]
    public void RaizCommand_DeveCriarComPropriedadesPadrao()
    {
        // Arrange & Act
        var comando = new ComandoTeste();

        // Assert
        comando.Should().NotBeNull();
        comando.RaizAgregacao.Should().Be(Guid.Empty);
        comando.DataHora.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        comando.Validacao.Should().NotBeNull();
        comando.Resultado.Should().NotBeNull();
        comando.Erros.Should().BeEmpty();
        comando.EstaValido().Should().BeTrue();
    }

    [Fact]
    public void RaizCommand_DeveDefinirRaizAgregacao()
    {
        // Arrange
        var comando = new ComandoTeste();
        var raizAgregacao = Guid.NewGuid();

        // Act
        comando.DefinirRaizAgregacao(raizAgregacao);

        // Assert
        comando.RaizAgregacao.Should().Be(raizAgregacao);
    }

    [Fact]
    public void RaizCommand_DeveDefinirValidacao()
    {
        // Arrange
        var comando = new ComandoTeste();
        var validacao = new ValidationResult();
        validacao.Errors.Add(new ValidationFailure("", "Erro de teste"));

        // Act
        comando.DefinirValidacao(validacao);

        // Assert
        comando.Validacao.Should().Be(validacao);
        comando.Resultado.ObterValidationResult().Should().Be(validacao);
        comando.Erros.Should().Contain("Erro de teste");
        comando.EstaValido().Should().BeFalse();
    }

    [Fact]
    public void RaizCommand_DeveSerValidoSemErros()
    {
        // Arrange
        var comando = new ComandoTeste();

        // Act & Assert
        comando.EstaValido().Should().BeTrue();
        comando.Erros.Should().BeEmpty();
    }

    [Fact]
    public void RaizCommand_DeveSerInvalidoComErros()
    {
        // Arrange
        var comando = new ComandoTeste();
        var validacao = new ValidationResult();
        validacao.Errors.Add(new ValidationFailure("", "Erro 1"));
        validacao.Errors.Add(new ValidationFailure("", "Erro 2"));

        // Act
        comando.DefinirValidacao(validacao);

        // Assert
        comando.EstaValido().Should().BeFalse();
        comando.Erros.Should().HaveCount(2);
        comando.Erros.Should().Contain("Erro 1");
        comando.Erros.Should().Contain("Erro 2");
    }

    [Fact]
    public void RaizCommand_DeveManterDataHoraOriginal()
    {
        // Arrange
        var comando = new ComandoTeste();
        var dataHoraOriginal = comando.DataHora;

        // Act
        Thread.Sleep(100);
        comando.DefinirRaizAgregacao(Guid.NewGuid());

        // Assert
        comando.DataHora.Should().Be(dataHoraOriginal);
    }

    [Fact]
    public void RaizCommand_DevePermitirValidacaoNull()
    {
        // Arrange
        var comando = new ComandoTeste();

        // Act
        comando.DefinirValidacao(null!);

        // Assert
        comando.Validacao.Should().BeNull();
        comando.Erros.Should().BeEmpty();
        comando.EstaValido().Should().BeTrue();
    }

    [Fact]
    public void Ctor_deve_inicializar_DataHora_UTC_e_Resultado_com_Validacao_vazia()
    {
        var antes = DateTime.Now.AddSeconds(-1);
        var cmd = new ComandoTeste();
        var depois = DateTime.Now.AddSeconds(1);

        cmd.DataHora.Should().BeOnOrAfter(antes).And.BeOnOrBefore(depois);
        cmd.Resultado.Should().NotBeNull();
        cmd.Erros.Should().BeEmpty();
        cmd.EstaValido().Should().BeTrue(); // Validacao.IsValid == true por padrão
    }

    [Fact]
    public void DefinirRaizAgregacao_deve_setar_propriedade()
    {
        var id = Guid.NewGuid();
        var cmd = new ComandoTeste();

        cmd.DefinirRaizAgregacao(id);

        cmd.RaizAgregacao.Should().Be(id);
    }

    [Fact]
    public void DefinirValidacao_deve_trocar_ValidationResult_e_refletir_em_Resultado()
    {
        var invalido = new ValidationResult(new[] { new ValidationFailure("a", "b") });
        var cmd = new ComandoTeste();

        cmd.DefinirValidacao(invalido);

        cmd.Validacao.Should().BeSameAs(invalido);
        cmd.Resultado.ObterValidationResult().Should().BeSameAs(invalido);
        cmd.EstaValido().Should().BeFalse();
        cmd.Erros.Should().Contain("b");
    }

    [Fact]
    public void EhValido_true_quando_Validacao_null()
    {
        var cmd = new ComandoTeste();
        cmd.DefinirValidacao(null!); // força null
        cmd.EstaValido().Should().BeTrue();
    }
}
