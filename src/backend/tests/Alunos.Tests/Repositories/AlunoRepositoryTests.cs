
using Alunos.Domain.Entities;
using Alunos.Domain.ValueObjects;
using Alunos.Infrastructure.Data;
using Alunos.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Alunos.Tests.Repositories.Infra;

namespace Alunos.Tests.Repositories;

public class AlunoRepositoryTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AlunoDbContext _ctx;
    private readonly AlunoRepository _repo;

    public AlunoRepositoryTests()
    {
        var (options, conn) = EfSqliteInMemoryFactory.Create();
        _conn = conn;
        _ctx = new AlunoDbContext(options);
        _repo = new AlunoRepository(_ctx);
    }

    // ----------------- Helpers -----------------
    private static Aluno NovoAluno(
        Guid? codigoAuth = null,
        string nome = "Fulano de Tal",
        string email = "fulano@exemplo.com",
        string cpf = "12345678909")
    {
        return new Aluno(
            codigoAuth ?? Guid.NewGuid(),
            nome,
            email,
            cpf,
            new DateTime(1995, 1, 1),
            "M",
            "São Paulo",
            "SP",
            "12345-678",
            "foto.png"
        );
    }

    private static MatriculaCurso NovaMatricula(Guid alunoId, Guid? cursoId = null, string nomeCurso = "Curso DDD")
        => new MatriculaCurso(alunoId, cursoId ?? Guid.NewGuid(), nomeCurso, 100m, "obs");

    private static Certificado NovoCertificado(Guid matriculaId, string nomeCurso = "Curso DDD")
        => new Certificado(matriculaId, nomeCurso,
                           DateTime.Now.AddDays(-1), // solicitação não-futura
                           null,                        // sem emissão
                           5, 8m, "certs/1.pdf", "Instrutor");

    private static HistoricoAprendizado NovoHistorico(Guid matriculaId, Guid cursoId, Guid aulaId, DateTime inicio, DateTime? termino = null)
        => new HistoricoAprendizado(matriculaId, cursoId, aulaId, "Aula 1", 5, inicio, termino);

    // ----------------- Tests -----------------

    [Fact]
    public async Task Adicionar_Commit_e_Obter_por_variantes_devem_funcionar()
    {
        var aluno = NovoAluno();
        await _repo.AdicionarAsync(aluno);
        (await _repo.UnitOfWork.Commit()).Should().BeTrue(); // Commit do DbContext (IUnitOfWork) :contentReference[oaicite:6]{index=6}

        // Obter por "Id" do repo (usa CodigoUsuarioAutenticacao como chave de consulta)
        var byId = await _repo.ObterPorIdAsync(aluno.CodigoUsuarioAutenticacao); // Include de Matrículas/Certificado + AsNoTracking :contentReference[oaicite:7]{index=7}
        byId.Should().NotBeNull();
        byId!.Email.Should().Be(aluno.Email);

        // Obter por Email
        var byEmail = await _repo.ObterPorEmailAsync(aluno.Email);
        byEmail.Should().NotBeNull();
        byEmail!.CodigoUsuarioAutenticacao.Should().Be(aluno.CodigoUsuarioAutenticacao);

        // Obter por Código de Usuário (mesma condição de chave) 
        var byCodigo = await _repo.ObterPorCodigoUsuarioAsync(aluno.CodigoUsuarioAutenticacao);
        byCodigo.Should().NotBeNull();
    }

    [Fact]
    public async Task Atualizar_deve_persistir_mudancas_e_chamar_Update()
    {
        var aluno = NovoAluno();
        await _repo.AdicionarAsync(aluno);
        await _repo.UnitOfWork.Commit();

        aluno = await _repo.ObterPorEmailAsync(aluno.Email, false);
        aluno.InativarAluno(); 

        await _repo.AtualizarAsync(aluno);
        await _repo.UnitOfWork.Commit();

        var reload = await _repo.ObterPorCodigoUsuarioAsync(aluno.CodigoUsuarioAutenticacao, false);
        reload.Ativo.Should().Be(false);

        aluno.AtivarAluno();
        await _repo.AtualizarAsync(aluno);
        await _repo.UnitOfWork.Commit();

        reload = await _repo.ObterPorCodigoUsuarioAsync(aluno.CodigoUsuarioAutenticacao, false);
        reload.Ativo.Should().Be(true);
    }

    [Fact]
    public async Task Adicionar_matricula_e_certificado_devem_refletir_nos_includes()
    {
        var aluno = NovoAluno();
        await _repo.AdicionarAsync(aluno);
        await _repo.UnitOfWork.Commit();

        // matricula
        var matricula = NovaMatricula(aluno.Id, Guid.NewGuid(), "Curso Clean Architecture");
        await _repo.AdicionarMatriculaCursoAsync(matricula);
        await _repo.UnitOfWork.Commit();

        // certificado
        var cert = NovoCertificado(matricula.Id, matricula.NomeCurso);
        await _repo.AdicionarCertificadoMatriculaCursoAsync(cert);
        await _repo.UnitOfWork.Commit();

        var loaded = await _repo.ObterPorCodigoUsuarioAsync(aluno.CodigoUsuarioAutenticacao); // Include(matriculas).ThenInclude(certificado) :contentReference[oaicite:9]{index=9}
        loaded!.MatriculasCursos.Should().ContainSingle(m => m.Id == matricula.Id);
        loaded.MatriculasCursos.First().Certificado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterMatriculaPorId_deve_incluir_historico_e_certificado()
    {
        var aluno = NovoAluno();
        await _repo.AdicionarAsync(aluno);
        await _repo.UnitOfWork.Commit();

        var cursoId = Guid.NewGuid();
        var aulaId = Guid.NewGuid();

        // cria matrícula já com 1 histórico em andamento
        var matricula = NovaMatricula(aluno.Id, cursoId, "Curso Avançado DDD");
        matricula.RegistrarPagamentoMatricula();
        var inicio = DateTime.Now.AddDays(-2);
        var historico = NovoHistorico(matricula.Id, cursoId, aulaId, inicio);
        matricula.RegistrarHistoricoAprendizado(aulaId, "Aula 1 - O que devo saber antes de iniciar", 5); // complemento de domínio

        await _repo.AdicionarMatriculaCursoAsync(matricula);
        await _repo.UnitOfWork.Commit();

        // adiciona certificado
        var cert = NovoCertificado(matricula.Id, "Curso DDD");
        await _repo.AdicionarCertificadoMatriculaCursoAsync(cert);
        await _repo.UnitOfWork.Commit();

        // consulta (repo faz Include(HistoricoAprendizado, Certificado))
        var loaded = await _repo.ObterMatriculaPorIdAsync(matricula.Id, false); // :contentReference[oaicite:10]{index=10}
        loaded.Should().NotBeNull();
        loaded!.HistoricoAprendizado.Should().NotBeNull();
        loaded.Certificado.Should().NotBeNull();
    }

    [Fact]
    public async Task AtualizarEstadoHistoricoAprendizado_deve_substituir_value_object()
    {
        var aluno = NovoAluno();
        await _repo.AdicionarAsync(aluno);
        await _repo.UnitOfWork.Commit();

        var cursoId = Guid.NewGuid();
        var aulaId = Guid.NewGuid();

        // cria matrícula + VO inicial (em andamento)
        var matricula = NovaMatricula(aluno.Id, cursoId, "Curso Especialista DDD");
        matricula.RegistrarPagamentoMatricula();
        var inicio = DateTime.Now.AddDays(-3);
        var hAntigo = NovoHistorico(matricula.Id, cursoId, aulaId, inicio, termino: null);
        // registra via domínio para garantir consistência
        matricula.RegistrarHistoricoAprendizado(aulaId, "Aula 1 - Introdução", 5);

        await _repo.AdicionarMatriculaCursoAsync(matricula);
        await _repo.UnitOfWork.Commit();

        // re-carrega o VO salvo (no-tracking) para usar como "antigo"
        var mReload = await _repo.ObterMatriculaPorIdAsync(matricula.Id, false);
        var antigo = mReload!.HistoricoAprendizado!.Single();

        // cria VO novo (concluído)
        var hNovo = NovoHistorico(matricula.Id, cursoId, aulaId, inicio, termino: DateTime.Now.AddDays(-1));

        await _repo.AtualizarEstadoHistoricoAprendizadoAsync(antigo, hNovo); // usa _context.AtualizarEstadoValueObject(...) :contentReference[oaicite:11]{index=11}
        await _repo.UnitOfWork.Commit();

        var after = await _repo.ObterMatriculaPorIdAsync(matricula.Id, false);
        var registro = after!.HistoricoAprendizado!.Single();
        registro.DataInicio.Should().Be(inicio);
        registro.DataTermino.Should().NotBeNull();
    }

    public void Dispose() => _conn.Dispose();
}
