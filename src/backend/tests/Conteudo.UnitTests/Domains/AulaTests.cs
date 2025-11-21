using Plataforma.Educacao.Core.Exceptions;

namespace Conteudo.UnitTests.Domains;
public class AulaTests
{
    // ---------- Happy path ----------
    [Fact]
    public void Deve_criar_aula_valida()
    {
        var a = new AulaBuilder().Build();

        a.Id.Should().NotBeEmpty();
        a.CursoId.Should().NotBeEmpty();
        a.Nome.Should().Be("Aula 01 - Introdução ao DDD");
        a.Descricao.Should().Be("Visão geral, termos e blocos táticos.");
        a.Numero.Should().Be(1);
        a.DuracaoMinutos.Should().Be(75);
        a.VideoUrl.Should().Be("https://cdn.exemplo.com/videos/aula01.mp4");
        a.TipoAula.Should().Be("Vídeo");
        a.IsObrigatoria.Should().BeTrue();
        a.Observacoes.Should().Be("Sem pré-requisitos");
        a.IsPublicada.Should().BeFalse();
        a.DataPublicacao.Should().BeNull();
    }

    // ---------- Validações de construção ----------
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_nome_vazio(string nome)
    {
        Action act = () => new AulaBuilder().ComNome(nome).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Nome da aula é obrigatório*");
    }

    [Fact]
    public void Deve_falhar_quando_nome_maior_que_200()
    {
        var nome = new string('x', 201);

        Action act = () => new AulaBuilder().ComNome(nome).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Nome da aula não pode ter mais de 200 caracteres*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_descricao_vazia(string desc)
    {
        Action act = () => new AulaBuilder().ComDescricao(desc).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Descrição da aula é obrigatória*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deve_falhar_quando_numero_invalido(int numero)
    {
        Action act = () => new AulaBuilder().ComNumero(numero).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Número da aula deve ser maior que zero*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Deve_falhar_quando_duracao_invalida(int minutos)
    {
        Action act = () => new AulaBuilder().ComDuracao(minutos).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Duração da aula deve ser maior que zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_url_vazia(string url)
    {
        Action act = () => new AulaBuilder().ComVideoUrl(url).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*URL do vídeo é obrigatória*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_tipo_vazio(string tipo)
    {
        Action act = () => new AulaBuilder().ComTipo(tipo).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Tipo da aula é obrigatório*");
    }

    // ---------- Comportamento atual: CursoId vazio é aceito ----------
    [Fact]
    public void Permite_cursoId_vazio_comportamento_atual()
    {
        var a = new AulaBuilder().ComCursoId(Guid.Empty).Build();
        a.CursoId.Should().Be(Guid.Empty);
    }

    // ---------- AtualizarInformacoes ----------
    [Fact]
    public void AtualizarInformacoes_deve_substituir_campos()
    {
        var a = new AulaBuilder().Build();

        a.AtualizarInformacoes(
            nome: "Aula 02 - Value Objects",
            descricao: "Imutabilidade e igualdade semântica",
            numero: 2,
            duracaoMinutos: 55,
            videoUrl: "https://cdn.exemplo.com/videos/aula02.mp4",
            tipoAula: "Vídeo",
            isObrigatoria: false,
            observacoes: "Exige leitura prévia"
        );

        a.Nome.Should().Be("Aula 02 - Value Objects");
        a.Descricao.Should().Be("Imutabilidade e igualdade semântica");
        a.Numero.Should().Be(2);
        a.DuracaoMinutos.Should().Be(55);
        a.VideoUrl.Should().Be("https://cdn.exemplo.com/videos/aula02.mp4");
        a.TipoAula.Should().Be("Vídeo");
        a.IsObrigatoria.Should().BeFalse();
        a.Observacoes.Should().Be("Exige leitura prévia");
    }

    [Theory]
    [InlineData("", "ok", 1, 10, "url", "tipo", "Nome da aula é obrigatório")]
    [InlineData("ok", "", 1, 10, "url", "tipo", "Descrição da aula é obrigatória")]
    [InlineData("ok", "ok", 0, 10, "url", "tipo", "Número da aula deve ser maior que zero")]
    [InlineData("ok", "ok", 1, 0, "url", "tipo", "Duração da aula deve ser maior que zero")]
    [InlineData("ok", "ok", 1, 10, "", "tipo", "URL do vídeo é obrigatória")]
    [InlineData("ok", "ok", 1, 10, "url", "", "Tipo da aula é obrigatório")]
    public void AtualizarInformacoes_deve_validar_campos(
        string nome, string desc, int numero, int minutos, string url, string tipo, string msg)
    {
        var a = new AulaBuilder().Build();

        a.Invoking(x => x.AtualizarInformacoes(nome, desc, numero, minutos, url, tipo, true))
         .Should().Throw<DomainException>()
         .WithMessage($"*{msg}*");
    }

    [Fact]
    public void AtualizarInformacoes_deve_validar_limite_de_nome()
    {
        var a = new AulaBuilder().Build();
        var nome = new string('x', 201);

        a.Invoking(x => x.AtualizarInformacoes(
                nome, "desc", 1, 10, "url", "tipo", true))
         .Should().Throw<DomainException>()
         .WithMessage("*Nome da aula não pode ter mais de 200 caracteres*");
    }

    // ---------- Publicar / Despublicar ----------
    [Fact]
    public void Publicar_deve_marcar_publicada_e_definir_data()
    {
        var a = new AulaBuilder().Build();

        a.Publicar();

        a.IsPublicada.Should().BeTrue();
        a.DataPublicacao.Should().NotBeNull();
        a.DataPublicacao!.Value.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Publicar_duas_vezes_deve_falhar()
    {
        var a = new AulaBuilder().Build();
        a.Publicar();

        a.Invoking(x => x.Publicar())
         .Should().Throw<DomainException>()
         .WithMessage("*Aula já está publicada*");
    }

    [Fact]
    public void Despublicar_deve_zerar_publicacao_quando_publicada()
    {
        var a = new AulaBuilder().Build();
        a.Publicar();

        a.Despublicar();

        a.IsPublicada.Should().BeFalse();
        a.DataPublicacao.Should().BeNull();
    }

    [Fact]
    public void Despublicar_sem_estar_publicada_deve_falhar()
    {
        var a = new AulaBuilder().Build();

        a.Invoking(x => x.Despublicar())
         .Should().Throw<DomainException>()
         .WithMessage("*Aula não está publicada*");
    }
}
