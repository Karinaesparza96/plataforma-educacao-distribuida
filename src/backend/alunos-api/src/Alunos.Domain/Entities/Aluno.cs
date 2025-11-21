using Alunos.Domain.ValueObjects;
using Core.DomainObjects;
using Core.DomainValidations;
using Plataforma.Educacao.Core.Exceptions;

namespace Alunos.Domain.Entities;

public class Aluno : Entidade, IRaizAgregacao
{
    public Guid CodigoUsuarioAutenticacao { get; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Cpf { get; }
    public DateTime DataNascimento { get; private set; }
    public string Telefone { get; private set; }
    public bool Ativo { get; private set; }
    public string Genero { get; private set; }
    public string Cidade { get; private set; }
    public string Estado { get; private set; }
    public string Cep { get; private set; }
    public string? Foto { get; private set; }

    private readonly List<MatriculaCurso> _matriculasCursos = [];
    public IReadOnlyCollection<MatriculaCurso> MatriculasCursos => _matriculasCursos.AsReadOnly();

    // EF Compatibility
    protected Aluno()
    { }

    public Aluno(Guid codigoUsuarioAutenticacao,
        string nome,
        string email,
        string cpf,
        DateTime dataNascimento,
        string genero,
        string cidade,
        string estado,
        string cep,
        string foto)
    {
        CodigoUsuarioAutenticacao = codigoUsuarioAutenticacao;
        Nome = nome?.Trim() ?? string.Empty;
        Email = email?.Trim().ToLowerInvariant() ?? string.Empty;
        Cpf = cpf?.Trim() ?? string.Empty;
        DataNascimento = dataNascimento.Date;
        Genero = genero?.Trim() ?? string.Empty;
        Cidade = cidade?.Trim() ?? string.Empty;
        Estado = estado?.Trim() ?? string.Empty;
        Cep = cep?.Trim().Replace("-", string.Empty).Replace(".", string.Empty) ?? string.Empty;
        Foto = foto?.Trim() ?? string.Empty;

        ValidarIntegridadeAluno();
    }

    public void AtivarAluno() => Ativo = true;

    public void InativarAluno() => Ativo = false;

    public int ObterQuantidadeAulasPendenteMatriculaCurso(Guid cursoId)
    {
        return _matriculasCursos.Count(m => m.CursoId == cursoId && m.PodeConcluirCurso() == false);
    }

    public MatriculaCurso ObterMatriculaCursoPeloId(Guid matriculaCursoId)
    {
        var matriculaCurso = _matriculasCursos.FirstOrDefault(m => m.Id == matriculaCursoId);
        if (matriculaCurso == null) { throw new DomainException("Matrícula não foi localizada"); }

        return matriculaCurso;
    }

    public MatriculaCurso MatricularAlunoEmCurso(Guid cursoId, string nomeCurso, decimal valor, string observacao)
    {
        if (_matriculasCursos.Any(m => m.CursoId == cursoId)) { throw new DomainException("Aluno já está matriculado neste curso"); }
        if (!Ativo) { throw new DomainException("Aluno inativo não pode ser matriculado em cursos"); }

        var novaMatricula = new MatriculaCurso(Id, cursoId, nomeCurso, valor, observacao);
        _matriculasCursos.Add(novaMatricula);

        return novaMatricula;
    }

    public void AtualizarPagamentoMatricula(Guid matriculaCursoId)
    {
        MatriculaCurso matriculaCurso = ObterMatriculaCursoPeloId(matriculaCursoId);
        matriculaCurso.RegistrarPagamentoMatricula();
    }

    public void ConcluirCurso(Guid matriculaCursoId)
    {
        MatriculaCurso matriculaCurso = ObterMatriculaCursoPeloId(matriculaCursoId);
        matriculaCurso.ConcluirCurso();
    }

    public void RegistrarHistoricoAprendizado(Guid matriculaCursoId, Guid aulaId, string nomeAula, int cargaHoraria, DateTime? dataTermino = null)
    {
        MatriculaCurso matriculaCurso = ObterMatriculaCursoPeloId(matriculaCursoId);
        matriculaCurso.RegistrarHistoricoAprendizado(aulaId, nomeAula, cargaHoraria, dataTermino);
    }

    public HistoricoAprendizado ObterHistoricoAprendizado(Guid matriculaCursoId, Guid aulaId)
    {
        var matriculaCurso = ObterMatriculaCursoPeloId(matriculaCursoId);
        var historico = matriculaCurso.HistoricoAprendizado.FirstOrDefault(h => h.AulaId == aulaId);

        return historico;
    }

    public int ObterQuantidadeAulasMatriculaCurso(Guid matriculaCursoId)
    {
        var matriculaCurso = ObterMatriculaCursoPeloId(matriculaCursoId);
        return matriculaCurso.ObterQuantidadeAulasRegistradas();
    }

    public void RequisitarCertificadoConclusao(Guid matriculaCursoId, decimal notaFinal, string pathCertificado, string nomeInstrutor)
    {
        MatriculaCurso matriculaCurso = ObterMatriculaCursoPeloId(matriculaCursoId);
        matriculaCurso.RequisitarCertificadoConclusao(notaFinal, pathCertificado, nomeInstrutor);
    }

    private void ValidarIntegridadeAluno(string novoNome = null,
        string novoEmail = null,
        string novoContato = null,
        DateTime? novaDataNascimento = null)
    {
        novoNome ??= Nome;
        novoEmail ??= Email;
        novoContato ??= Telefone;
        novaDataNascimento ??= DataNascimento;

        var validacao = new ResultadoValidacao<Aluno>();

        ValidacaoGuid.DeveSerValido(CodigoUsuarioAutenticacao, "Código de identificação deve ser informado", validacao);
        ValidacaoTexto.DevePossuirConteudo(novoNome, "Nome não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(novoNome, 3, 100, "Nome deve ter entre 3 e 100 caracteres", validacao);
        ValidacaoTexto.DevePossuirConteudo(novoEmail, "Email não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(novoEmail, 3, 100, "Email deve ter entre 3 e 100 caracteres", validacao);
        ValidacaoTexto.DeveAtenderRegex(novoEmail, @"^[\w\.\-]+@([\w\-]+\.)+[\w\-]{2,}$", "Email informado é inválido", validacao);
        ValidacaoTexto.DevePossuirConteudo(Cpf, "CPF não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DeveSerCpfValido(Cpf, "CPF informado é inválido", validacao);
        ValidacaoData.DeveSerValido(novaDataNascimento.Value, "Data de nascimento deve ser válida", validacao);
        ValidacaoData.DeveSerMenorQue(novaDataNascimento.Value, DateTime.Now, "Data de nascimento não pode ser superior à data atual", validacao);
        ValidacaoTexto.DevePossuirConteudo(Genero, "Genero não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(Genero, 1, 20, "Genero deve ter entre 1 e 20 caracteres", validacao);
        ValidacaoTexto.DevePossuirConteudo(Cidade, "Cidade não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(Cidade, 1, 50, "Cidade deve ter entre 1 e 50 caracteres", validacao);
        ValidacaoTexto.DevePossuirConteudo(Cidade, "Cidade não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(Estado, 2, 2, "Estado deve ter 2 caracteres", validacao);
        ValidacaoTexto.DevePossuirConteudo(Cep, "Cep não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(Cep, 1, 8, "Cep deve ter até 8 caracteres", validacao);

        if (!string.IsNullOrWhiteSpace(Foto))
        {
            ValidacaoTexto.DevePossuirTamanho(Foto, 1, 1024, "Foto deve ter entre 1 e 1024 caracteres", validacao);
        }

        if (!string.IsNullOrWhiteSpace(novoContato))
        {
            ValidacaoTexto.DevePossuirTamanho(novoContato, 1, 25, "Contato deve ter entre 1 e 25 caracteres", validacao);
        }

        validacao.DispararExcecaoDominioSeInvalido();
    }

    public override string ToString() => $"{Nome} (Email: {Email})";
}
