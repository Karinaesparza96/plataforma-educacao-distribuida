using Alunos.Application.Commands.RegistrarHistoricoAprendizado;
using FluentAssertions;

namespace Alunos.Tests.Applications.RegistrarHistoricoAprendizado;
public class RegistrarHistoricoAprendizadoCommandValidatorTests
{
    [Fact]
    public void Deve_invalidar_campos_quebrados()
    {
        var cmd = new RegistrarHistoricoAprendizadoCommand(Guid.Empty, Guid.Empty, Guid.Empty, null!, 0);
        var result = new RegistrarHistoricoAprendizadoCommandValidator().Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Deve_ser_valido_quando_campos_ok()
    {
        var cmd = new RegistrarHistoricoAprendizadoCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "A1", 10, DateTime.Now);
        new RegistrarHistoricoAprendizadoCommandValidator().Validate(cmd).IsValid.Should().BeTrue();
    }
}
