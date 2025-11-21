using Alunos.Application.Events.RegistrarProblemaHistorico;
using FluentAssertions;

namespace Alunos.Tests.Applications.RegistrarProblemaHistorico;
public class RegistrarProblemaHistoricoAprendizadoEventTests
{
    [Fact]
    public void Construtor_deve_mapear_props_e_definir_RaizAgregacao_com_MatriculaCursoId()
    {
        var aluno = Guid.NewGuid();
        var matricula = Guid.NewGuid();
        var aula = Guid.NewGuid();
        var dt = DateTime.Now;

        var evt = new RegistrarProblemaHistoricoAprendizadoEvent(aluno, matricula, aula, dt, "falhou");

        evt.AlunoId.Should().Be(aluno);
        evt.MatriculaCursoId.Should().Be(matricula);
        evt.AulaId.Should().Be(aula);
        evt.DataTermino.Should().Be(dt);
        evt.MensagemErro.Should().Be("falhou");
        evt.RaizAgregacao.Should().Be(matricula); // vem de DefinirRaizAgregacao(matricula)
    }
}
