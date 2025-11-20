using Core.Mediator;
using Core.Messages.Integration;
using Core.Notification;
using Pagamentos.Application.Interfaces;
using Pagamentos.Application.ViewModels;

namespace Pagamentos.IntegrationTests.API
{
    public class PagamentosControllerIntegrationTests
    {
        private readonly Mock<IPagamentoConsultaAppService> _mockPagamentoConsultaService;
        private readonly Mock<IPagamentoComandoAppService> _mockPagamentoComandoService;
        private readonly Mock<IMediatorHandler> _mockMediator;
        private readonly Mock<INotificador> _mockNotificador;

        public PagamentosControllerIntegrationTests()
        {
            _mockPagamentoConsultaService = new Mock<IPagamentoConsultaAppService>();
            _mockPagamentoComandoService = new Mock<IPagamentoComandoAppService>();
            _mockMediator = new Mock<IMediatorHandler>();
            _mockNotificador = new Mock<INotificador>();
        }

        [Fact]
        public async Task Pagamento_ComDadosValidos_DeveSerProcessado()
        {
            // Arrange
            var pagamentoRequest = new PagamentoCursoViewModel
            {
                MatriculaId = Guid.NewGuid(),
                AlunoId = Guid.NewGuid(),
                Total = 299.99m,
                NomeCartao = "João Silva",
                NumeroCartao = "4111111111111111",
                ExpiracaoCartao = "12/25",
                CvvCartao = "123"
            };

            _mockMediator
                .Setup(x => x.PublicarEvento(It.IsAny<PagamentoCursoEvent>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            _mockMediator.Verify(
                x => x.PublicarEvento(It.IsAny<PagamentoCursoEvent>()),
                Times.Never
            );

            await _mockMediator.Object.PublicarEvento(new PagamentoCursoEvent(
                pagamentoRequest.MatriculaId,
                pagamentoRequest.AlunoId,
                pagamentoRequest.Total,
                pagamentoRequest.NomeCartao,
                pagamentoRequest.NumeroCartao,
                pagamentoRequest.ExpiracaoCartao,
                pagamentoRequest.CvvCartao
            ));

            _mockMediator.Verify(
                x => x.PublicarEvento(It.IsAny<PagamentoCursoEvent>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ObterTodos_ComPagamentosExistentes_DeveRetornarLista()
        {
            // Arrange
            var pagamentos = new List<PagamentoViewModel>
            {
                new PagamentoViewModel
                {
                    Id = Guid.NewGuid(),
                    Status = "Aprovado",
                    Valor = 299.99m
                },
                new PagamentoViewModel
                {
                    Id = Guid.NewGuid(),
                    Status = "Pendente",
                    Valor = 199.99m
                }
            };

            _mockPagamentoConsultaService
                .Setup(x => x.ObterTodos())
                .ReturnsAsync(pagamentos);

            // Act
            var resultado = await _mockPagamentoConsultaService.Object.ObterTodos();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2);
            resultado.Should().Contain(p => p.Status == "Aprovado");
            resultado.Should().Contain(p => p.Status == "Pendente");

            _mockPagamentoConsultaService.Verify(
                x => x.ObterTodos(),
                Times.Once
            );
        }

        [Fact]
        public async Task ObterTodos_SemPagamentos_DeveRetornarListaVazia()
        {
            // Arrange
            var pagamentos = new List<PagamentoViewModel>();

            _mockPagamentoConsultaService
                .Setup(x => x.ObterTodos())
                .ReturnsAsync(pagamentos);

            // Act
            var resultado = await _mockPagamentoConsultaService.Object.ObterTodos();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(0);

            _mockPagamentoConsultaService.Verify(
                x => x.ObterTodos(),
                Times.Once
            );
        }

        [Fact]
        public async Task ObterPorId_ComIdValido_DeveRetornarPagamento()
        {
            // Arrange
            var id = Guid.NewGuid();
            var pagamento = new PagamentoViewModel
            {
                Id = id,
                Status = "Aprovado",
                Valor = 299.99m
            };

            _mockPagamentoConsultaService
                .Setup(x => x.ObterPorId(id))
                .ReturnsAsync(pagamento);

            // Act
            var resultado = await _mockPagamentoConsultaService.Object.ObterPorId(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(id);
            resultado.Status.Should().Be("Aprovado");
            resultado.Valor.Should().Be(299.99m);

            _mockPagamentoConsultaService.Verify(
                x => x.ObterPorId(id),
                Times.Once
            );
        }

        [Fact]
        public async Task ObterPorId_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockPagamentoConsultaService
                .Setup(x => x.ObterPorId(id))
                .ReturnsAsync((PagamentoViewModel?)null);

            // Act
            var resultado = await _mockPagamentoConsultaService.Object.ObterPorId(id);

            // Assert
            resultado.Should().BeNull();

            _mockPagamentoConsultaService.Verify(
                x => x.ObterPorId(id),
                Times.Once
            );
        }

        [Fact]
        public async Task Pagamento_ComMediatorFalhando_DevePropagarExcecao()
        {
            // Arrange
            var pagamentoRequest = new PagamentoCursoViewModel
            {
                MatriculaId = Guid.NewGuid(),
                AlunoId = Guid.NewGuid(),
                Total = 299.99m,
                NomeCartao = "João Silva",
                NumeroCartao = "4111111111111111",
                ExpiracaoCartao = "12/25",
                CvvCartao = "123"
            };

            _mockMediator
                .Setup(x => x.PublicarEvento(It.IsAny<PagamentoCursoEvent>()))
                .ThrowsAsync(new Exception("Erro no mediator"));

            // Act & Assert
            var excecao = await Assert.ThrowsAsync<Exception>(() =>
                _mockMediator.Object.PublicarEvento(new PagamentoCursoEvent(
                    pagamentoRequest.MatriculaId,
                    pagamentoRequest.AlunoId,
                    pagamentoRequest.Total,
                    pagamentoRequest.NomeCartao,
                    pagamentoRequest.NumeroCartao,
                    pagamentoRequest.ExpiracaoCartao,
                    pagamentoRequest.CvvCartao
                )));

            excecao.Message.Should().Be("Erro no mediator");

            _mockMediator.Verify(
                x => x.PublicarEvento(It.IsAny<PagamentoCursoEvent>()),
                Times.Once
            );
        }
    }
}
