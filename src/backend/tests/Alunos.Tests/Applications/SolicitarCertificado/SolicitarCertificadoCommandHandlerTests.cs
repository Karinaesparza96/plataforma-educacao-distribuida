using Alunos.Application.Commands.SolicitarCertificado;
using Alunos.Domain.Entities;
using Alunos.Domain.Interfaces;
using Core.Data;
using Core.Mediator;
using FluentAssertions;
using Moq;
using Core.Messages;

namespace Alunos.Tests.Applications.SolicitarCertificado;
public class SolicitarCertificadoCommandHandlerTests
{
    private readonly Mock<IAlunoRepository> _repo = new();
    private readonly Mock<IMediatorHandler> _mediator = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private SolicitarCertificadoCommandHandler CriarSut()
    {
        _uow.Setup(u => u.Commit()).ReturnsAsync(true);
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        return new SolicitarCertificadoCommandHandler(_repo.Object, _mediator.Object);
    }

    private static Aluno NovoAlunoComMatricula(out MatriculaCurso matricula)
    {
        var aluno = new Aluno(Guid.NewGuid(), "Fulano", "f@e.com", "12345678909", new DateTime(1990, 1, 1), "M", "SP", "SP", "01001000", "foto");
        aluno.AtivarAluno();
        matricula = aluno.MatricularAlunoEmCurso(Guid.NewGuid(), "Curso avançado de DDD", 100, "obs");
        matricula.RegistrarPagamentoMatricula();

        matricula.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Modulo A1 :: Introcução", 10, DateTime.Now);
        matricula.ConcluirCurso();

        return aluno;
    }

    [Fact]
    public async Task Deve_retornar_invalido_e_notificar_quando_command_invalido()
    {
        var cmd = new SolicitarCertificadoCommand(Guid.Empty, Guid.Empty);
        var sut = CriarSut();

        var res = await sut.Handle(cmd, CancellationToken.None);

        res.IsValid.Should().BeFalse();
        _mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.AtLeastOnce);
        _repo.Verify(r => r.AdicionarCertificadoMatriculaCursoAsync(It.IsAny<Certificado>()), Times.Never);
        _uow.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task Deve_notificar_quando_aluno_nao_encontrado()
    {
        var cmd = new SolicitarCertificadoCommand(Guid.NewGuid(), Guid.NewGuid());
        var sut = CriarSut();

        _repo.Setup(r => r.ObterPorIdAsync(cmd.AlunoId, true)).ReturnsAsync((Aluno?)null);

        var res = await sut.Handle(cmd, CancellationToken.None);

        res.IsValid.Should().BeTrue();
        res.Data.Should().BeNull();
        _mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.Once);
        _uow.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task Deve_notificar_quando_matricula_nao_encontrada()
    {
        var cmd = new SolicitarCertificadoCommand(Guid.NewGuid(), Guid.NewGuid());
        var sut = CriarSut();

        _repo.Setup(r => r.ObterPorIdAsync(cmd.AlunoId, true)).ReturnsAsync(NovoAlunoComMatricula(out _));
        _repo.Setup(r => r.ObterMatriculaPorIdAsync(cmd.MatriculaCursoId, true)).ReturnsAsync((MatriculaCurso?)null);

        var res = await sut.Handle(cmd, CancellationToken.None);

        res.IsValid.Should().BeTrue();
        res.Data.Should().BeNull();
        _mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.Once);
        _uow.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    public async Task HappyPath_deve_adicionar_certificado_commit_e_retornar_Id()
    {
        var sut = CriarSut();
        var aluno = NovoAlunoComMatricula(out var matricula);
        var cmd = new SolicitarCertificadoCommand(aluno.Id, matricula.Id);

        _repo.Setup(r => r.ObterPorIdAsync(aluno.Id, true)).ReturnsAsync(aluno);
        _repo.Setup(r => r.ObterMatriculaPorIdAsync(matricula.Id, true)).ReturnsAsync(matricula);

        Certificado? adicionado = null;
        _repo.Setup(r => r.AdicionarCertificadoMatriculaCursoAsync(It.IsAny<Certificado>()))
             .Callback<Certificado>(c => adicionado = c)
             .Returns(Task.CompletedTask);

        var res = await sut.Handle(cmd, CancellationToken.None);

        adicionado.Should().NotBeNull();
        _uow.Verify(u => u.Commit(), Times.Once);

        res.IsValid.Should().BeTrue();
        res.Data.Should().Be(adicionado!.Id);
        _mediator.Verify(m => m.PublicarNotificacaoDominio(It.IsAny<DomainNotificacaoRaiz>()), Times.Never);
    }

    [Fact]
    public async Task Commit_false_deixa_Data_nula()
    {
        var sut = CriarSut();
        var aluno = NovoAlunoComMatricula(out var matricula);
        var cmd = new SolicitarCertificadoCommand(aluno.Id, matricula.Id);

        _repo.Setup(r => r.ObterPorIdAsync(aluno.Id, true)).ReturnsAsync(aluno);
        _repo.Setup(r => r.ObterMatriculaPorIdAsync(matricula.Id, true)).ReturnsAsync(matricula);
        _uow.Setup(u => u.Commit()).ReturnsAsync(false);

        var res = await sut.Handle(cmd, CancellationToken.None);

        res.IsValid.Should().BeTrue();
        res.Data.Should().BeNull();
    }
}
