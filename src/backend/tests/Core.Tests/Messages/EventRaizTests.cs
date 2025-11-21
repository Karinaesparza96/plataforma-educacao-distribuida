using Core.Messages;
using FluentValidation.Results;

namespace Core.Tests.Messages;

public class EventRaizTests 
{
    private class EventoTeste : EventRaiz
    {
        public string Nome { get; set; } = string.Empty;
    }

    [Fact]
    public void EventRaiz_DeveCriarComPropriedadesPadrao()
    {
        // Arrange & Act
        var evento = new EventoTeste();

        // Assert
        evento.Should().NotBeNull();
        evento.RaizAgregacao.Should().Be(Guid.Empty);
        evento.DataHora.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        evento.Validacao.Should().BeNull();
        evento.Erros.Should().BeEmpty();
        evento.EstaValido().Should().BeTrue();
    }

    [Fact]
    public void EventRaiz_DeveDefinirRaizAgregacao()
    {
        // Arrange
        var evento = new EventoTeste();
        var raizAgregacao = Guid.NewGuid();

        // Act
        evento.DefinirRaizAgregacao(raizAgregacao);

        // Assert
        evento.RaizAgregacao.Should().Be(raizAgregacao);
    }

    [Fact]
    public void EventRaiz_DeveDefinirValidacao()
    {
        // Arrange
        var evento = new EventoTeste();
        var validacao = new ValidationResult();
        validacao.Errors.Add(new ValidationFailure("", "Erro de teste"));

        // Act
        evento.DefinirValidacao(validacao);

        // Assert
        evento.Validacao.Should().Be(validacao);
        evento.Erros.Should().Contain("Erro de teste");
        evento.EstaValido().Should().BeFalse();
    }

    [Fact]
    public void EventRaiz_DeveSerValidoSemValidacao()
    {
        // Arrange
        var evento = new EventoTeste();

        // Act & Assert
        evento.EstaValido().Should().BeTrue();
        evento.Erros.Should().BeEmpty();
    }

    [Fact]
    public void EventRaiz_DeveSerValidoComValidacaoValida()
    {
        // Arrange
        var evento = new EventoTeste();
        var validacao = new ValidationResult();

        // Act
        evento.DefinirValidacao(validacao);

        // Assert
        evento.EstaValido().Should().BeTrue();
        evento.Erros.Should().BeEmpty();
    }

    [Fact]
    public void EventRaiz_DeveSerInvalidoComValidacaoInvalida()
    {
        // Arrange
        var evento = new EventoTeste();
        var validacao = new ValidationResult();
        validacao.Errors.Add(new ValidationFailure("", "Erro 1"));
        validacao.Errors.Add(new ValidationFailure("", "Erro 2"));

        // Act
        evento.DefinirValidacao(validacao);

        // Assert
        evento.EstaValido().Should().BeFalse();
        evento.Erros.Should().HaveCount(2);
        evento.Erros.Should().Contain("Erro 1");
        evento.Erros.Should().Contain("Erro 2");
    }

    [Fact]
    public void EventRaiz_DeveManterDataHoraOriginal()
    {
        // Arrange
        var evento = new EventoTeste();
        var dataHoraOriginal = evento.DataHora;

        // Act
        Thread.Sleep(100);
        evento.DefinirRaizAgregacao(Guid.NewGuid());

        // Assert
        evento.DataHora.Should().Be(dataHoraOriginal);
    }

    [Fact]
    public void EventRaiz_DevePermitirValidacaoNull()
    {
        // Arrange
        var evento = new EventoTeste();

        // Act
        evento.DefinirValidacao(null!);

        // Assert
        evento.Validacao.Should().BeNull();
        evento.Erros.Should().BeEmpty();
        evento.EstaValido().Should().BeTrue();
    }

    [Fact]
    public void Ctor_deve_definir_DataHora_UTC()
    {
        var antes = DateTime.Now.AddSeconds(-1);
        var e = new EventoTeste();
        var depois = DateTime.Now.AddSeconds(1);

        e.DataHora.Should().BeOnOrAfter(antes).And.BeOnOrBefore(depois);
    }

    [Fact]
    public void DefinirRaizAgregacao_e_DefinirValidacao_devem_setar_propriedades()
    {
        var id = Guid.NewGuid();
        var vr = new ValidationResult();

        var e = new EventoTeste();
        e.DefinirRaizAgregacao(id);
        e.DefinirValidacao(vr);

        e.RaizAgregacao.Should().Be(id);
        e.Validacao.Should().BeSameAs(vr);
        e.EstaValido().Should().BeTrue();
    }

    [Fact]
    public void EhValido_false_quando_validacao_invalida()
    {
        var e = new EventoTeste();
        e.DefinirValidacao(new ValidationResult(new[] { new ValidationFailure("x", "y") }));

        e.EstaValido().Should().BeFalse();
        e.Erros.Should().Contain("y");
    }
}
