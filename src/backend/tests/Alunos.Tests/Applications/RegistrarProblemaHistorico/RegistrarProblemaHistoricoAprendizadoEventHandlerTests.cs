using Alunos.Application.Events.RegistrarProblemaHistorico;
using Core.Mediator;
using Core.Messages;
using Moq;

namespace Alunos.Tests.Applications.RegistrarProblemaHistorico;
public class RegistrarProblemaHistoricoAprendizadoEventHandlerTests
{
    [Fact]
    public async Task Deve_publicar_notificacoes_para_cada_erro_quando_evento_invalido()
    {
        var mediator = new Mock<IMediatorHandler>();
        var handler = new RegistrarProblemaHistoricoAprendizadoEventHandler(mediator.Object);

        // Evento inválido (todos vazios)
        var evt = new RegistrarProblemaHistoricoAprendizadoEvent(Guid.Empty, Guid.Empty, Guid.Empty, null, "x");

        await handler.Handle(evt, CancellationToken.None);

        // Validator possui 3 regras -> 3 notificações
        mediator.Verify(m => m.PublicarNotificacaoDominio(It.Is<DomainNotificacaoRaiz>(n =>
            n.Chave == "Aluno" &&               // nameof(Domain.Entities.Aluno)
            n.RaizAgregacao == evt.RaizAgregacao &&
            !string.IsNullOrWhiteSpace(n.Valor)
        )), Times.Exactly(3));
    }

    [Fact]
    public async Task Evento_valido_nao_deve_publicar_notificacao()
    {
        var mediator = new Mock<IMediatorHandler>();
        var handler = new RegistrarProblemaHistoricoAprendizadoEventHandler(mediator.Object);

        var evt = new RegistrarProblemaHistoricoAprendizadoEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "x");

        await handler.Handle(evt, CancellationToken.None);

        mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.Never);
    }
}
