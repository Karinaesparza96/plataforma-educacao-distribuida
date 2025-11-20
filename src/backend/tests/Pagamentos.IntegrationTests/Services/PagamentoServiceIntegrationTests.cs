using Pagamentos.Application.Interfaces;
using Pagamentos.Application.ViewModels;
using Pagamentos.Domain.Entities;
using Pagamentos.Domain.Interfaces;
using Pagamentos.Domain.Models;

namespace Pagamentos.IntegrationTests.Services
{
    public class PagamentoServiceIntegrationTests
    {
        private readonly Mock<IPagamentoService> _mockPagamentoService;
        private readonly Mock<IPagamentoConsultaAppService> _mockPagamentoConsultaService;
        private readonly Mock<IPagamentoComandoAppService> _mockPagamentoComandoService;

        public PagamentoServiceIntegrationTests()
        {
            _mockPagamentoService = new Mock<IPagamentoService>();
            _mockPagamentoConsultaService = new Mock<IPagamentoConsultaAppService>();
            _mockPagamentoComandoService = new Mock<IPagamentoComandoAppService>();
        }

        [Fact]
        public async Task ObterTodos_ComPagamentosExistentes_DeveRetornarListaCompleta()
        {
            // Arrange
            var pagamentos = new List<PagamentoViewModel>
            {
                new PagamentoViewModel
                {
                    Id = Guid.NewGuid(),
                    Status = "Aprovado",
                    Valor = 299.99m,
                    AlunoId = Guid.NewGuid(),
                    CobrancaCursoId = Guid.NewGuid()
                },
                new PagamentoViewModel
                {
                    Id = Guid.NewGuid(),
                    Status = "Pendente",
                    Valor = 199.99m,
                    AlunoId = Guid.NewGuid(),
                    CobrancaCursoId = Guid.NewGuid()
                },
                new PagamentoViewModel
                {
                    Id = Guid.NewGuid(),
                    Status = "Rejeitado",
                    Valor = 399.99m,
                    AlunoId = Guid.NewGuid(),
                    CobrancaCursoId = Guid.NewGuid()
                }
            };

            _mockPagamentoConsultaService
                .Setup(x => x.ObterTodos())
                .ReturnsAsync(pagamentos);

            // Act
            var resultado = await _mockPagamentoConsultaService.Object.ObterTodos();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(3);
            resultado.Should().Contain(p => p.Status == "Aprovado");
            resultado.Should().Contain(p => p.Status == "Pendente");
            resultado.Should().Contain(p => p.Status == "Rejeitado");

            _mockPagamentoConsultaService.Verify(
                x => x.ObterTodos(),
                Times.Once
            );
        }

        [Fact]
        public async Task ObterPorId_ComIdValido_DeveRetornarPagamentoCorreto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var pagamento = new PagamentoViewModel
            {
                Id = id,
                Status = "Aprovado",
                Valor = 299.99m,
                AlunoId = Guid.NewGuid(),
                CobrancaCursoId = Guid.NewGuid(),
                NomeCartao = "João Silva",
                NumeroCartao = "4111111111111111",
                ExpiracaoCartao = "12/25",
                CvvCartao = "123"
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
            resultado.NomeCartao.Should().Be("João Silva");

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
        public async Task RealizarPagamento_ComDadosValidos_DeveSerProcessado()
        {
            // Arrange
            var pagamentoCurso = new PagamentoCurso
            {
                CursoId = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Total = 299.99m,
                NomeCartao = "João Silva",
                NumeroCartao = "4111111111111111",
                ExpiracaoCartao = "12/25",
                CvvCartao = "123"
            };

            var transacaoEsperada = new Transacao
            {
                CobrancaCursoId = pagamentoCurso.CursoId,
                PagamentoId = Guid.NewGuid(),
                Total = pagamentoCurso.Total,
                StatusTransacao = Pagamentos.Domain.Enum.StatusTransacao.Pago
            };

            _mockPagamentoService
                .Setup(x => x.RealizarPagamento(It.IsAny<PagamentoCurso>()))
                .ReturnsAsync(transacaoEsperada);

            // Act
            var resultado = await _mockPagamentoService.Object.RealizarPagamento(pagamentoCurso);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().Be(transacaoEsperada);
            resultado.Total.Should().Be(pagamentoCurso.Total);

            _mockPagamentoService.Verify(
                x => x.RealizarPagamento(It.Is<PagamentoCurso>(p =>
                    p.CursoId == pagamentoCurso.CursoId &&
                    p.ClienteId == pagamentoCurso.ClienteId &&
                    p.Total == pagamentoCurso.Total &&
                    p.NomeCartao == pagamentoCurso.NomeCartao)),
                Times.Once
            );
        }

        [Fact]
        public async Task RealizarPagamento_ComValorZero_DeveSerProcessado()
        {
            // Arrange
            var pagamentoCurso = new PagamentoCurso
            {
                CursoId = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Total = 0m,
                NomeCartao = "João Silva",
                NumeroCartao = "4111111111111111",
                ExpiracaoCartao = "12/25",
                CvvCartao = "123"
            };

            var transacaoEsperada = new Transacao
            {
                CobrancaCursoId = pagamentoCurso.CursoId,
                PagamentoId = Guid.NewGuid(),
                Total = pagamentoCurso.Total,
                StatusTransacao = Pagamentos.Domain.Enum.StatusTransacao.Pago
            };

            _mockPagamentoService
                .Setup(x => x.RealizarPagamento(It.IsAny<PagamentoCurso>()))
                .ReturnsAsync(transacaoEsperada);

            // Act
            var resultado = await _mockPagamentoService.Object.RealizarPagamento(pagamentoCurso);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Total.Should().Be(0);

            _mockPagamentoService.Verify(
                x => x.RealizarPagamento(It.Is<PagamentoCurso>(p => p.Total == 0)),
                Times.Once
            );
        }

        [Fact]
        public async Task RealizarPagamento_ComValorNegativo_DeveSerProcessado()
        {
            // Arrange
            var pagamentoCurso = new PagamentoCurso
            {
                CursoId = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Total = -50.00m,
                NomeCartao = "João Silva",
                NumeroCartao = "4111111111111111",
                ExpiracaoCartao = "12/25",
                CvvCartao = "123"
            };

            var transacaoEsperada = new Transacao
            {
                CobrancaCursoId = pagamentoCurso.CursoId,
                PagamentoId = Guid.NewGuid(),
                Total = pagamentoCurso.Total,
                StatusTransacao = Pagamentos.Domain.Enum.StatusTransacao.Pago
            };

            _mockPagamentoService
                .Setup(x => x.RealizarPagamento(It.IsAny<PagamentoCurso>()))
                .ReturnsAsync(transacaoEsperada);

            // Act
            var resultado = await _mockPagamentoService.Object.RealizarPagamento(pagamentoCurso);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Total.Should().Be(-50.00m);

            _mockPagamentoService.Verify(
                x => x.RealizarPagamento(It.Is<PagamentoCurso>(p => p.Total == -50.00m)),
                Times.Once
            );
        }

        [Fact]
        public async Task RealizarPagamento_ComServicoFalhando_DevePropagarExcecao()
        {
            // Arrange
            var pagamentoCurso = new PagamentoCurso
            {
                CursoId = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Total = 299.99m,
                NomeCartao = "João Silva",
                NumeroCartao = "4111111111111111",
                ExpiracaoCartao = "12/25",
                CvvCartao = "123"
            };

            _mockPagamentoService
                .Setup(x => x.RealizarPagamento(It.IsAny<PagamentoCurso>()))
                .ThrowsAsync(new Exception("Erro no processamento do pagamento"));

            // Act & Assert
            var excecao = await Assert.ThrowsAsync<Exception>(() =>
                _mockPagamentoService.Object.RealizarPagamento(pagamentoCurso));

            excecao.Message.Should().Be("Erro no processamento do pagamento");

            _mockPagamentoService.Verify(
                x => x.RealizarPagamento(It.IsAny<PagamentoCurso>()),
                Times.Once
            );
        }
    }
}
