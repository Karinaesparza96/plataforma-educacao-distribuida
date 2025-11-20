using Pagamento.AntiCorruption.Interfaces;
using Pagamento.AntiCorruption.Services;
using Pagamentos.Domain.Enum;
using Pagamentos.Domain.Models;

namespace Pagamentos.UnitTests.Applications;
public class PagamentoCartaoCreditoFacadeTests
{
    [Fact]
    public void Quando_gateway_aprova_deve_retornar_transacao_Paga()
    {
        var gateway = new Mock<IPayPalGateway>();
        var config = new Mock<IConfigurationManager>();

        config.Setup(c => c.GetValue("apiKey")).Returns("AK");
        config.Setup(c => c.GetValue("encriptionKey")).Returns("EK");

        gateway.Setup(g => g.GetPayPalServiceKey("AK", "EK")).Returns("svc");
        gateway.Setup(g => g.GetCardHashKey("svc", "4111111111111111")).Returns("hash");
        gateway.Setup(g => g.CommitTransaction("hash", It.IsAny<string>(), 100m)).Returns(true);

        var facade = new PagamentoCartaoCreditoFacade(gateway.Object, config.Object);

        var cobranca = new CobrancaCurso { Id = Guid.NewGuid(), Valor = 100m };
        var pagamento = new Domain.Entities.Pagamento { NumeroCartao = "4111111111111111", Valor = 100m };

        var tx = facade.RealizarPagamento(cobranca, pagamento);

        tx.StatusTransacao.Should().Be(StatusTransacao.Pago);
        tx.CobrancaCursoId.Should().Be(cobranca.Id);
        tx.Total.Should().Be(cobranca.Valor);
        tx.PagamentoId.Should().Be(pagamento.Id);

        gateway.Verify(g => g.CommitTransaction("hash", cobranca.Id.ToString(), 100m), Times.Once);
    }

    [Fact]
    public void Quando_gateway_recusa_deve_retornar_transacao_Recusada()
    {
        var gateway = new Mock<IPayPalGateway>();
        var config = new Mock<IConfigurationManager>();

        config.Setup(c => c.GetValue("apiKey")).Returns("AK");
        config.Setup(c => c.GetValue("encriptionKey")).Returns("EK");

        gateway.Setup(g => g.GetPayPalServiceKey("AK", "EK")).Returns("svc");
        gateway.Setup(g => g.GetCardHashKey("svc", It.IsAny<string>())).Returns("hash");
        gateway.Setup(g => g.CommitTransaction("hash", It.IsAny<string>(), It.IsAny<decimal>())).Returns(false);

        var facade = new PagamentoCartaoCreditoFacade(gateway.Object, config.Object);

        var tx = facade.RealizarPagamento(new CobrancaCurso { Id = Guid.NewGuid(), Valor = 50m },
                                          new Domain.Entities.Pagamento { NumeroCartao = "5555", Valor = 50m });

        tx.StatusTransacao.Should().Be(StatusTransacao.Recusado);
    }
}
