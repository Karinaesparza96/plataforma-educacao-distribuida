using Alunos.Application.Events.RegistrarProblemaHistorico;
using FluentAssertions;

namespace Alunos.Tests.Applications.RegistrarProblemaHistorico;
public class RegistrarProblemaHistoricoAprendizadoEventValidatorTests
{
    [Fact]
    public void Deve_invalidar_quando_algum_Id_estiver_vazio()
    {
        var evt = new RegistrarProblemaHistoricoAprendizadoEvent(
            Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "x");

        var result = new RegistrarProblemaHistoricoAprendizadoEventValidator().Validate(evt);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Deve_ser_valido_quando_todos_campos_ok()
    {
        var evt = new RegistrarProblemaHistoricoAprendizadoEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "erro");

        new RegistrarProblemaHistoricoAprendizadoEventValidator().Validate(evt).IsValid.Should().BeTrue();
    }
}
