using Core.Data;
using Core.Mediator;
using Core.Messages.Integration;
using global::Pagamentos.Domain.Entities;
using global::Pagamentos.Domain.Enum;
using global::Pagamentos.Domain.Interfaces;
using global::Pagamentos.Domain.Models;
using Pagamentos.Domain.Services;
using Core.Messages;

namespace Pagamentos.UnitTests.Applications;
public class PagamentoServiceTests
{
    private readonly Mock<IPagamentoCartaoCreditoFacade> _facade = new();
    private readonly Mock<IPagamentoRepository> _repo = new();
    private readonly Mock<IMediatorHandler> _mediator = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private PagamentoService CriarSut()
    {
        _uow.Setup(u => u.Commit()).ReturnsAsync(true);
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        return new PagamentoService(_facade.Object, _repo.Object, _mediator.Object);
    }

    private static PagamentoCurso Cmd() => new()
    {
        CursoId = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(),
        Total = 250,
        NomeCartao = "Dev",
        NumeroCartao = "4111111111111111",
        ExpiracaoCartao = "12/30",
        CvvCartao = "123"
    };

    [Fact]
    public async Task Quando_pago_deve_publicar_evento_persistir_e_commit()
    {
        var sut = CriarSut();
        var cmd = Cmd();
        var transacao = new Transacao
        {
            PagamentoId = Guid.NewGuid(),
            CobrancaCursoId = cmd.CursoId,
            Total = cmd.Total,
            StatusTransacao = StatusTransacao.Pago
        };

        Domain.Entities.Pagamento? pagamentoAdicionado = null;
        Transacao? transacaoAdicionada = null;

        _facade.Setup(f => f.RealizarPagamento(It.IsAny<CobrancaCurso>(), It.IsAny<Domain.Entities.Pagamento>()))
               .Returns(transacao);

        _repo.Setup(r => r.Adicionar(It.IsAny<  Domain.Entities.Pagamento>()))
             .Callback<Domain.Entities.Pagamento>(p => pagamentoAdicionado = p);
        _repo.Setup(r => r.AdicionarTransacao(It.IsAny<Transacao>()))
             .Callback<Transacao>(t => transacaoAdicionada = t);

        var result = await sut.RealizarPagamento(cmd);

        // Persistência
        pagamentoAdicionado.Should().NotBeNull();
        transacaoAdicionada.Should().BeSameAs(transacao);
        pagamentoAdicionado!.Status.Should().Be(StatusTransacao.Pago.ToString());
        pagamentoAdicionado.Transacao.Should().BeSameAs(transacao);

        _uow.Verify(u => u.Commit(), Times.Once);

        // Evento positivo
        _mediator.Verify(m => m.PublicarEvento(It.IsAny<PagamentoRealizadoEvent>()), Times.Once);
        _mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.Never);

        result.Should().BeSameAs(transacao);
    }

    [Fact]
    public async Task Quando_recusado_deve_publicar_notificacao_e_evento_sem_persistir()
    {
        var sut = CriarSut();
        var cmd = Cmd();
        var transacao = new Transacao
        {
            PagamentoId = Guid.NewGuid(),
            CobrancaCursoId = cmd.CursoId,
            Total = cmd.Total,
            StatusTransacao = StatusTransacao.Recusado
        };

        _facade.Setup(f => f.RealizarPagamento(It.IsAny<CobrancaCurso>(), It.IsAny<Domain.Entities.Pagamento>()))
               .Returns(transacao);

        var result = await sut.RealizarPagamento(cmd);

        _repo.Verify(r => r.Adicionar(It.IsAny<Domain.Entities.Pagamento>()), Times.Never);
        _repo.Verify(r => r.AdicionarTransacao(It.IsAny<Transacao>()), Times.Never);
        _uow.Verify(u => u.Commit(), Times.Never);

        _mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.Once);
        _mediator.Verify(m => m.PublicarEvento(It.IsAny<PagamentoRecusadoEvent>()), Times.Once);

        result.Should().BeSameAs(transacao);
    }
}
