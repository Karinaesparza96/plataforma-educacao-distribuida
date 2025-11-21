using Alunos.Application.Commands.RegistrarHistoricoAprendizado;
using Alunos.Application.Events.RegistrarProblemaHistorico;
using Alunos.Domain.Entities;
using Alunos.Domain.Interfaces;
using Alunos.Domain.ValueObjects;
using Core.Data;
using Core.Mediator;
using Core.Messages;
using FluentAssertions;
using Moq;

namespace Alunos.Tests.Applications.RegistrarHistoricoAprendizado;
public class RegistrarHistoricoAprendizadoCommandHandlerTests
{
    private readonly Mock<IAlunoRepository> _repo = new();
    private readonly Mock<IMediatorHandler> _mediator = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private RegistrarHistoricoAprendizadoCommandHandler CriarSut()
    {
        _uow.Setup(u => u.Commit()).ReturnsAsync(true);
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        return new RegistrarHistoricoAprendizadoCommandHandler(_repo.Object, _mediator.Object);
    }

    private static Aluno NovoAlunoComMatricula(out MatriculaCurso matricula)
    {
        var aluno = new Aluno(Guid.NewGuid(), "Fulano", "f@e.com", "12345678909", new DateTime(1990, 1, 1), "M", "SP", "SP", "01001000", "foto");
        aluno.AtivarAluno();
        matricula = aluno.MatricularAlunoEmCurso(Guid.NewGuid(), "Curso avançado de DDD", 100, "obs");
        matricula.RegistrarPagamentoMatricula();

        return aluno;
    }

    [Fact]
    public async Task Deve_retornar_invalido_e_notificar_quando_command_invalido()
    {
        var cmd = new RegistrarHistoricoAprendizadoCommand(Guid.Empty, Guid.Empty, Guid.Empty, null!, 0);
        var sut = CriarSut();

        var res = await sut.Handle(cmd, CancellationToken.None);

        res.IsValid.Should().BeFalse();
        _mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.AtLeastOnce);
        _repo.Verify(r => r.AtualizarEstadoHistoricoAprendizadoAsync(It.IsAny<HistoricoAprendizado?>(), It.IsAny<HistoricoAprendizado?>()), Times.Never);
        _uow.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task Deve_notificar_quando_aluno_nao_encontrado()
    {
        var cmd = new RegistrarHistoricoAprendizadoCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "A1", 10);
        var sut = CriarSut();

        _repo.Setup(r => r.ObterPorIdAsync(cmd.AlunoId, true)).ReturnsAsync((Aluno?)null);

        var res = await sut.Handle(cmd, CancellationToken.None);

        res.IsValid.Should().BeTrue();
        res.Data.Should().BeNull();
        _mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.Once);
        _uow.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task HappyPath_deve_registrar_historico_atualizar_e_commit()
    {
        var cmd = new RegistrarHistoricoAprendizadoCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Modulo A1 :: Introcução", 10, DateTime.Now);
        var sut = CriarSut();

        var aluno = NovoAlunoComMatricula(out var matricula);
        // garantir que o comando usa a matrícula do aluno
        cmd = new RegistrarHistoricoAprendizadoCommand(aluno.Id, matricula.Id, Guid.NewGuid(), "Modulo A1 :: Introcução", 10, DateTime.Now);

        _repo.Setup(r => r.ObterPorIdAsync(aluno.Id, true)).ReturnsAsync(aluno);

        _repo.Setup(r => r.AtualizarEstadoHistoricoAprendizadoAsync(It.IsAny<HistoricoAprendizado?>(), It.IsAny<HistoricoAprendizado?>()))
             .Returns(Task.CompletedTask);

        var res = await sut.Handle(cmd, CancellationToken.None);

        _repo.Verify(r => r.AtualizarEstadoHistoricoAprendizadoAsync(It.IsAny<HistoricoAprendizado?>(), It.IsAny<HistoricoAprendizado?>()), Times.Once);
        _uow.Verify(u => u.Commit(), Times.Once);
        res.IsValid.Should().BeTrue();
        res.Data.Should().Be(true);
    }

    [Fact]
    public async Task Commit_false_deixa_Data_nula()
    {
        var sut = CriarSut();
        var aluno = NovoAlunoComMatricula(out var matricula);
        var cmd = new RegistrarHistoricoAprendizadoCommand(aluno.Id, matricula.Id, Guid.NewGuid(), "Modulo A1 :: Introcução", 10);

        _repo.Setup(r => r.ObterPorIdAsync(aluno.Id, true)).ReturnsAsync(aluno);
        _uow.Setup(u => u.Commit()).ReturnsAsync(false);

        var res = await sut.Handle(cmd, CancellationToken.None);

        res.IsValid.Should().BeTrue();
        res.Data.Should().BeNull();
    }

    [Fact]
    public async Task Excecao_deve_publicar_evento_e_relancar()
    {
        var sut = CriarSut();
        var aluno = NovoAlunoComMatricula(out var matricula);
        var cmd = new RegistrarHistoricoAprendizadoCommand(aluno.Id, matricula.Id, Guid.NewGuid(), "Modulo A1 :: Introcução", 10);

        _repo.Setup(r => r.ObterPorIdAsync(aluno.Id, true)).ReturnsAsync(aluno);
        _repo.Setup(r => r.AtualizarEstadoHistoricoAprendizadoAsync(It.IsAny<HistoricoAprendizado?>(), It.IsAny<HistoricoAprendizado?>()))
             .ThrowsAsync(new InvalidOperationException("boom"));

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _mediator.Verify(m => m.PublicarEvento(It.IsAny<RegistrarProblemaHistoricoAprendizadoEvent>()), Times.Once);
        _uow.Verify(u => u.Commit(), Times.Never);
    }
}

