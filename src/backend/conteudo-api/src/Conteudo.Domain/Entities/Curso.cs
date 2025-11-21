using Conteudo.Domain.ValueObjects;
using Core.DomainObjects;
using Plataforma.Educacao.Core.Exceptions;

namespace Conteudo.Domain.Entities;

public class Curso : Entidade, IRaizAgregacao
{
    public string Nome { get; private set; }
    public decimal Valor { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime? ValidoAte { get; private set; }
    public ConteudoProgramatico ConteudoProgramatico { get; private set; }
    public Guid? CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }
    public int DuracaoHoras { get; private set; }
    public string Nivel { get; private set; }
    public string ImagemUrl { get; private set; }
    public string Instrutor { get; private set; }
    public int VagasMaximas { get; private set; }
    public int VagasOcupadas { get; private set; }

    private readonly List<Aula> _aulas = [];
    public IReadOnlyCollection<Aula> Aulas => _aulas.AsReadOnly();

    protected Curso()
    { }

    public Curso(
        string nome,
        decimal valor,
        ConteudoProgramatico conteudoProgramatico,
        int duracaoHoras,
        string nivel,
        string instrutor,
        int vagasMaximas,
        string imagemUrl = "",
        DateTime? validoAte = null,
        Guid? categoriaId = null)
    {
        ValidarDados(nome, valor, conteudoProgramatico, duracaoHoras, nivel, instrutor, vagasMaximas);

        Nome = nome;
        Valor = valor;
        ConteudoProgramatico = conteudoProgramatico;
        DuracaoHoras = duracaoHoras;
        Nivel = nivel;
        Instrutor = instrutor;
        VagasMaximas = vagasMaximas;
        VagasOcupadas = 0;
        ImagemUrl = imagemUrl;
        ValidoAte = validoAte;
        CategoriaId = categoriaId;
        Ativo = true;
    }

    private static void ValidarDados(string nome, decimal valor, ConteudoProgramatico conteudoProgramatico,
        int duracaoHoras, string nivel, string instrutor, int vagasMaximas)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do curso é obrigatório");

        if (nome.Length > 200)
            throw new DomainException("Nome do curso não pode ter mais de 200 caracteres");

        if (valor < 0)
            throw new DomainException("Valor do curso não pode ser negativo");

        if (conteudoProgramatico == null)
            throw new DomainException("Conteúdo programático é obrigatório");

        if (duracaoHoras <= 0)
            throw new DomainException("Duração do curso deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(nivel))
            throw new DomainException("Nível do curso é obrigatório");

        if (string.IsNullOrWhiteSpace(instrutor))
            throw new DomainException("Instrutor é obrigatório");

        if (vagasMaximas <= 0)
            throw new DomainException("Número de vagas deve ser maior que zero");
    }

    public void AtualizarInformacoes(
        string nome,
        decimal valor,
        ConteudoProgramatico conteudoProgramatico,
        int duracaoHoras,
        string nivel,
        string instrutor,
        int vagasMaximas,
        string imagemUrl = "",
        DateTime? validoAte = null,
        Guid? categoriaId = null)
    {
        ValidarDados(nome, valor, conteudoProgramatico, duracaoHoras, nivel, instrutor, vagasMaximas);

        Nome = nome;
        Valor = valor;
        ConteudoProgramatico = conteudoProgramatico;
        DuracaoHoras = duracaoHoras;
        Nivel = nivel;
        Instrutor = instrutor;
        VagasMaximas = vagasMaximas;
        ImagemUrl = imagemUrl;
        ValidoAte = validoAte;
        CategoriaId = categoriaId;

        AtualizarDataModificacao();
    }

    public void AdicionarMatricula()
    {
        if (VagasOcupadas >= VagasMaximas)
            throw new DomainException("Não há vagas disponíveis para este curso");

        VagasOcupadas++;
        AtualizarDataModificacao();
    }

    public bool TemVagasDisponiveis => VagasOcupadas < VagasMaximas;
    public int VagasDisponiveis => VagasMaximas - VagasOcupadas;
    public bool EstaExpirado => ValidoAte.HasValue && ValidoAte.Value < DateTime.Now;
    public bool PodeSerMatriculado => Ativo && !EstaExpirado && TemVagasDisponiveis;
}
