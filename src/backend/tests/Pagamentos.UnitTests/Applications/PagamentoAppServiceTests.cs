using Pagamentos.Application.Services;
using Pagamentos.Domain.Interfaces;

namespace Pagamentos.UnitTests.Applications;
public class PagamentoAppServiceTests
{
    [Fact]
    public async Task ObterPorId_deve_chamar_repo_e_retornar_ViewModel()
    {
        var repo = new Mock<IPagamentoRepository>();
        var svc = new PagamentoAppService(repo.Object);

        var id = Guid.NewGuid();
        repo.Setup(r => r.ObterPorId(id)).ReturnsAsync(new Domain.Entities.Pagamento { Valor = 10 });

        var vm = await svc.ObterPorId(id);

        vm.Should().NotBeNull();
        repo.Verify(r => r.ObterPorId(id), Times.Once);
    }

    [Fact]
    public async Task ObterTodos_deve_chamar_repo_e_mapear_lista()
    {
        var repo = new Mock<IPagamentoRepository>();
        var svc = new PagamentoAppService(repo.Object);

        repo.Setup(r => r.ObterTodos()).ReturnsAsync(new List<Domain.Entities.Pagamento>
        {
            new() { Valor = 1 },
            new() { Valor = 2 }
        });

        var vms = (await svc.ObterTodos()).ToList();

        vms.Should().HaveCount(2);
        repo.Verify(r => r.ObterTodos(), Times.Once);
    }
}
