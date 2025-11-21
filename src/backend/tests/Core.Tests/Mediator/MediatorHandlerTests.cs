using Core.Communication;
using Core.Mediator;
using Core.Messages;
using FluentValidation.Results;
using MediatR;

namespace Core.Tests.Mediator;

public class MediatorHandlerTests 
{
    private class ComandoTeste : RaizCommand
    {
        public string Nome { get; set; } = string.Empty;
    }

    private class EventoTeste : EventRaiz
    {
        public string Mensagem { get; set; } = string.Empty;
    }

    private class NotificacaoTeste : DomainNotificacaoRaiz
    {
        public NotificacaoTeste(string chave, string valor) : base(chave, valor)
        {
        }
    }

    [Fact]
    public void MediatorHandler_DeveImplementarIMediatorHandler()
    {
        // Arrange & Act
        var handler = typeof(MediatorHandler);

        // Assert
        handler.Should().Implement<IMediatorHandler>();
    }

    [Fact]
    public void ComandoTeste_DeveHerdarDeRaizCommand()
    {
        // Arrange & Act
        var comando = new ComandoTeste { Nome = "Teste" };

        // Assert
        comando.Should().NotBeNull();
        comando.Should().BeAssignableTo<RaizCommand>();
        comando.Nome.Should().Be("Teste");
    }

    [Fact]
    public void EventoTeste_DeveHerdarDeEventRaiz()
    {
        // Arrange & Act
        var evento = new EventoTeste { Mensagem = "Evento teste" };

        // Assert
        evento.Should().NotBeNull();
        evento.Should().BeAssignableTo<EventRaiz>();
        evento.Mensagem.Should().Be("Evento teste");
    }

    [Fact]
    public void NotificacaoTeste_DeveHerdarDeDomainNotificacaoRaiz()
    {
        // Arrange & Act
        var notificacao = new NotificacaoTeste("chave", "valor");

        // Assert
        notificacao.Should().NotBeNull();
        notificacao.Should().BeAssignableTo<DomainNotificacaoRaiz>();
        notificacao.Chave.Should().Be("chave");
        notificacao.Valor.Should().Be("valor");
    }

    [Fact]
    public void RaizCommand_DeveTerPropriedadesCorretas()
    {
        // Arrange & Act
        var comando = new ComandoTeste();

        // Assert
        comando.Should().NotBeNull();
        comando.RaizAgregacao.Should().Be(Guid.Empty);
        comando.DataHora.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        comando.Validacao.Should().NotBeNull();
        comando.Resultado.Should().NotBeNull();
    }

    [Fact]
    public void EventRaiz_DeveTerPropriedadesCorretas()
    {
        // Arrange & Act
        var evento = new EventoTeste();

        // Assert
        evento.Should().NotBeNull();
        evento.RaizAgregacao.Should().Be(Guid.Empty);
        evento.DataHora.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        evento.Validacao.Should().BeNull();
    }

    [Fact]
    public void DomainNotificacaoRaiz_DeveTerPropriedadesCorretas()
    {
        // Arrange & Act
        var notificacao = new NotificacaoTeste("chave", "valor");

        // Assert
        notificacao.Should().NotBeNull();
        notificacao.Chave.Should().Be("chave");
        notificacao.Valor.Should().Be("valor");
        notificacao.NotificacaoId.Should().NotBe(Guid.Empty);
        notificacao.DataHora.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task EnviarComando_deve_retornar_ValidationResult_do_CommandResult()
    {
        var mediator = new Mock<IMediator>();
        var sut = new MediatorHandler(mediator.Object);

        var vr = new ValidationResult(new[] { new ValidationFailure("a", "b") });
        var cr = new CommandResult(vr);

        mediator.Setup(m => m.Send(It.IsAny<ComandoTeste>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cr);

        var retorno = await sut.EnviarComando(new ComandoTeste());

        retorno.Should().BeSameAs(vr);
    }

    [Fact]
    public async Task ExecutarComando_deve_retornar_CommandResult_do_mediator()
    {
        var mediator = new Mock<IMediator>();
        var sut = new MediatorHandler(mediator.Object);

        var cr = new CommandResult(new ValidationResult());
        mediator.Setup(m => m.Send(It.IsAny<ComandoTeste>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cr);

        var retorno = await sut.ExecutarComando(new ComandoTeste());

        retorno.Should().BeSameAs(cr);
    }

    [Fact]
    public async Task PublicarEvento_e_PublicarNotificacaoDominio_devem_invocar_mediator_Publish()
    {
        var mediator = new Mock<IMediator>();
        var sut = new MediatorHandler(mediator.Object);

        var evt = new EventoTeste();
        var not = new NotificacaoTeste(Guid.NewGuid().ToString(), "Conteudo enviado na notificação");

        await sut.PublicarEvento(evt);
        await sut.PublicarNotificacaoDominio(not);

        mediator.Verify(m => m.Publish(evt, It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(m => m.Publish(not, It.IsAny<CancellationToken>()), Times.Once);
    }
}
