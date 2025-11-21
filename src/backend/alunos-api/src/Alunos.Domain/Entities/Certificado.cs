using Core.DomainObjects;
using Core.DomainValidations;
using Plataforma.Educacao.Core.Exceptions;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Alunos.Domain.Entities;

public class Certificado : Entidade
{
    public Guid MatriculaCursoId { get; }
    public string NomeCurso { get; }
    public DateTime DataSolicitacao { get; }
    public DateTime? DataEmissao { get; private set; }
    public short CargaHoraria { get; private set; }
    public decimal NotaFinal { get; private set; }
    public string PathCertificado { get; private set; }
    public string NomeInstrutor { get; private set; }

    [JsonIgnore]
    public MatriculaCurso MatriculaCurso { get; private set; }

    protected Certificado()
    { }

    public Certificado(Guid matriculaId,
        string nomeCurso,
        DateTime? dataSolicitacao,
        DateTime? dataEmissao,
        short cargaHoraria,
        decimal notaFinal,
        string pathCertificado,
        string nomeInstrutor)
    {
        MatriculaCursoId = matriculaId;
        NomeCurso = nomeCurso?.Trim() ?? string.Empty;
        DataSolicitacao = dataSolicitacao ?? DateTime.Now;
        DataEmissao = dataEmissao;
        CargaHoraria = cargaHoraria;
        NotaFinal = notaFinal;
        PathCertificado = pathCertificado?.Trim() ?? string.Empty;
        NomeInstrutor = nomeInstrutor?.Trim() ?? "ONLINE";

        ValidarIntegridadeCertificado();
    }

    internal void AtualizarDataEmissao(DateTime dataEmissao)
    {
        ValidarIntegridadeCertificado(novaDataEmissao: dataEmissao);
        DataEmissao = dataEmissao;
    }

    internal void AtualizarCargaHoraria(short cargaHoraria)
    {
        ValidarSePodeModificarEstadoCertificado();
        ValidarIntegridadeCertificado(novaCargaHoraria: cargaHoraria);
        CargaHoraria = cargaHoraria;
    }

    internal void AtualizarNotaFinal(byte notaFinal)
    {
        ValidarSePodeModificarEstadoCertificado();
        ValidarIntegridadeCertificado(novaNotaFinal: notaFinal);
        NotaFinal = notaFinal;
    }

    internal void AtualizarPathCertificado(string path)
    {
        ValidarSePodeModificarEstadoCertificado();
        ValidarIntegridadeCertificado(novoPathCertificado: path ?? string.Empty);
        PathCertificado = path.Trim();
    }

    internal void AtualizarNomeInstrutor(string nomeInstrutor)
    {
        ValidarSePodeModificarEstadoCertificado();
        ValidarIntegridadeCertificado(novoNomeInstrutor: nomeInstrutor ?? string.Empty);
        NomeInstrutor = nomeInstrutor.Trim();
    }

    private void ValidarSePodeModificarEstadoCertificado()
    {
        if (DataEmissao.HasValue) { throw new DomainException("Certificado foi emitido e não pode sofrer alterações"); }
    }

    private void ValidarIntegridadeCertificado(DateTime? novaDataEmissao = null,
        int? novaCargaHoraria = null,
        decimal? novaNotaFinal = null,
        string novoPathCertificado = null,
        string novoNomeInstrutor = null)
    {
        var validacao = new ResultadoValidacao<Certificado>();
        novaDataEmissao ??= DataEmissao;
        novaCargaHoraria ??= CargaHoraria;
        novaNotaFinal ??= NotaFinal;
        novoPathCertificado ??= PathCertificado;
        novoNomeInstrutor ??= NomeInstrutor;

        ValidacaoGuid.DeveSerValido(MatriculaCursoId, "Matrícula do curso deve ser informada", validacao);
        ValidacaoTexto.DevePossuirConteudo(NomeCurso, "Nome do curso não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(NomeCurso, 1, 200, "Path do certificado deve ter no máximo 200 caracteres", validacao);
        ValidacaoData.DeveSerValido(DataSolicitacao, "Data de solicitação deve ser válida", validacao);
        ValidacaoData.DeveSerMenorQue(DataSolicitacao, DateTime.Now, "Data de solicitação não pode ser superior à data atual", validacao);
        ValidacaoTexto.DevePossuirConteudo(novoPathCertificado, "Path do certificado não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(novoPathCertificado, 1, 1024, "Path do certificado deve ter no máximo 1024 caracteres", validacao);

        if (novaDataEmissao.HasValue)
        {
            ValidacaoData.DeveSerValido(novaDataEmissao.Value, "Data de emissão deve ser válida", validacao);
            ValidacaoData.DeveSerMaiorQue(DateTime.Now, novaDataEmissao.Value, "Data de emissão deve ser igual ou superior à data atual", validacao);
        }

        ValidacaoNumerica.DeveSerMaiorQueZero(novaCargaHoraria.Value, "Carga horária deve ser maior que zero", validacao);
        ValidacaoNumerica.DeveEstarEntre(novaCargaHoraria.Value, 1, 10000, "Carga horária deve estar entre 1 e 10.000 horas", validacao);

        if (novaNotaFinal.HasValue) { ValidacaoNumerica.DeveEstarEntre(novaNotaFinal.Value, 0m, 10m, "Nota final deve estar entre 0 e 10", validacao); }

        ValidacaoTexto.DevePossuirConteudo(novoNomeInstrutor, "Nome do instrutor não pode ser nulo ou vazio", validacao);
        ValidacaoTexto.DevePossuirTamanho(novoNomeInstrutor, 1, 100, "Nome do instrutor deve ter no máximo 100 caracteres", validacao);

        validacao.DispararExcecaoDominioSeInvalido();
    }

    public override string ToString() =>
    $"Certificado do curso {NomeCurso} (matrícula {MatriculaCursoId}) com total de {CargaHoraria} horas e nota final {NotaFinal.ToString("0.##", new CultureInfo("pt-BR"))} solicitado em {DataSolicitacao:dd/MM/yyyy}";

}
