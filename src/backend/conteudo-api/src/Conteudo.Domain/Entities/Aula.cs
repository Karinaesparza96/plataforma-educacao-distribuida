using Core.DomainObjects;
using Plataforma.Educacao.Core.Exceptions;

namespace Conteudo.Domain.Entities;

public class Aula : Entidade, IRaizAgregacao
{
    public Guid CursoId { get; private set; }
    public Curso Curso { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public int Numero { get; private set; }
    public int DuracaoMinutos { get; private set; }
    public string VideoUrl { get; private set; }
    public string TipoAula { get; private set; }
    public bool IsObrigatoria { get; private set; }
    public bool IsPublicada { get; private set; }
    public DateTime? DataPublicacao { get; private set; }
    public string Observacoes { get; private set; }

    private readonly List<Material> _materiais = [];
    public IReadOnlyCollection<Material> Materiais => _materiais.AsReadOnly();

    protected Aula()
    { }

    public Aula(
        Guid cursoId,
        string nome,
        string descricao,
        int numero,
        int duracaoMinutos,
        string videoUrl,
        string tipoAula,
        bool isObrigatoria = true,
        string observacoes = "")
    {
        ValidarDados(nome, descricao, numero, duracaoMinutos, videoUrl, tipoAula);

        CursoId = cursoId;
        Nome = nome;
        Descricao = descricao;
        Numero = numero;
        DuracaoMinutos = duracaoMinutos;
        VideoUrl = videoUrl;
        TipoAula = tipoAula;
        IsObrigatoria = isObrigatoria;
        Observacoes = observacoes;
        IsPublicada = false;
    }

    private static void ValidarDados(string nome, string descricao, int numero, int duracaoMinutos, string videoUrl, string tipoAula)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome da aula é obrigatório");

        if (nome.Length > 200)
            throw new DomainException("Nome da aula não pode ter mais de 200 caracteres");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição da aula é obrigatória");

        if (numero <= 0)
            throw new DomainException("Número da aula deve ser maior que zero");

        if (duracaoMinutos <= 0)
            throw new DomainException("Duração da aula deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new DomainException("URL do vídeo é obrigatória");

        if (string.IsNullOrWhiteSpace(tipoAula))
            throw new DomainException("Tipo da aula é obrigatório");
    }

    public void AtualizarInformacoes(
        string nome,
        string descricao,
        int numero,
        int duracaoMinutos,
        string videoUrl,
        string tipoAula,
        bool isObrigatoria,
        string observacoes = "")
    {
        ValidarDados(nome, descricao, numero, duracaoMinutos, videoUrl, tipoAula);

        Nome = nome;
        Descricao = descricao;
        Numero = numero;
        DuracaoMinutos = duracaoMinutos;
        VideoUrl = videoUrl;
        TipoAula = tipoAula;
        IsObrigatoria = isObrigatoria;
        Observacoes = observacoes;

        AtualizarDataModificacao();
    }

    public void Publicar()
    {
        if (IsPublicada)
            throw new DomainException("Aula já está publicada");

        IsPublicada = true;
        DataPublicacao = DateTime.Now;
        AtualizarDataModificacao();
    }

    public void Despublicar()
    {
        if (!IsPublicada)
            throw new DomainException("Aula não está publicada");

        IsPublicada = false;
        DataPublicacao = null;
        AtualizarDataModificacao();
    }
}
