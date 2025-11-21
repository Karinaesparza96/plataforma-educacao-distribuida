using Conteudo.Domain.Entities;
using Conteudo.Domain.ValueObjects;
using Plataforma.Educacao.Core.Exceptions;

namespace Conteudo.UnitTests.Domains;
public class CursoTests
{
    // -------------------- Happy path & defaults --------------------
    [Fact]
    public void Deve_criar_curso_valido()
    {
        var c = new CursoBuilder().Build();

        c.Id.Should().NotBeEmpty();
        c.Nome.Should().Be("Arquitetura Limpa e DDD");
        c.Valor.Should().Be(499.90m);
        c.ConteudoProgramatico.Should().NotBeNull();
        c.DuracaoHoras.Should().Be(40);
        c.Nivel.Should().Be("Intermediário");
        c.Instrutor.Should().Be("Tio Bob");
        c.VagasMaximas.Should().Be(30);
        c.VagasOcupadas.Should().Be(0);
        c.ImagemUrl.Should().Be("");      // default do ctor
        c.ValidoAte.Should().BeNull();
        c.CategoriaId.Should().BeNull();
        c.Ativo.Should().BeTrue();        // default do ctor
        c.Aulas.Should().NotBeNull();
        c.Aulas.Count.Should().Be(0);
        c.TemVagasDisponiveis.Should().BeTrue();
        c.VagasDisponiveis.Should().Be(30);
        c.EstaExpirado.Should().BeFalse();
        c.PodeSerMatriculado.Should().BeTrue();
    }

    [Fact]
    public void Deve_aceitar_parametros_opcionais()
    {
        var ate = new DateTime(2030, 12, 31);
        var cat = Guid.NewGuid();
        var c = new CursoBuilder()
            .ComImagem("https://cdn.exemplo.com/img.png")
            .ValidoAte(ate)
            .ComCategoria(cat)
            .Build();

        c.ImagemUrl.Should().Be("https://cdn.exemplo.com/img.png");
        c.ValidoAte.Should().Be(ate);
        c.CategoriaId.Should().Be(cat);
    }

    // -------------------- Validações de construção --------------------
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_nome_vazio(string nome)
    {
        Action act = () => new CursoBuilder().ComNome(nome).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Nome do curso é obrigatório*");
    }

    [Fact]
    public void Deve_falhar_quando_nome_maior_que_200()
    {
        var nome = new string('x', 201);

        Action act = () => new CursoBuilder().ComNome(nome).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Nome do curso não pode ter mais de 200 caracteres*");
    }

    [Fact]
    public void Deve_falhar_quando_valor_negativo()
    {
        Action act = () => new CursoBuilder().ComValor(-0.01m).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Valor do curso não pode ser negativo*");
    }

    [Fact]
    public void Deve_falhar_quando_conteudo_programatico_nulo()
    {
        Action act = () => new CursoBuilder().ComConteudo(null!).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Conteúdo programático é obrigatório*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deve_falhar_quando_duracao_invalida(int horas)
    {
        Action act = () => new CursoBuilder().ComDuracao(horas).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Duração do curso deve ser maior que zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_nivel_vazio(string nivel)
    {
        Action act = () => new CursoBuilder().ComNivel(nivel).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Nível do curso é obrigatório*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_instrutor_vazio(string instrutor)
    {
        Action act = () => new CursoBuilder().ComInstrutor(instrutor).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Instrutor é obrigatório*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Deve_falhar_quando_vagas_maximas_invalida(int vagas)
    {
        Action act = () => new CursoBuilder().ComVagas(vagas).Build();

        act.Should().Throw<DomainException>()
           .WithMessage("*Número de vagas deve ser maior que zero*");
    }

    // -------------------- AtualizarInformacoes --------------------
    [Fact]
    public void AtualizarInformacoes_deve_substituir_campos_e_opcionais()
    {
        var c = new CursoBuilder().Build();
        var novoConteudo = new ConteudoProgramatico(
            "Novo resumo", "Nova descrição", "Novos objetivos",
            "PR", "PA", "MET", "REC", "AV", "BIB");

        var ate = new DateTime(2028, 05, 01);
        var cat = Guid.NewGuid();

        c.AtualizarInformacoes(
            nome: "CQRS e Event Sourcing",
            valor: 799.00m,
            conteudoProgramatico: novoConteudo,
            duracaoHoras: 60,
            nivel: "Avançado",
            instrutor: "Gregor",
            vagasMaximas: 100,
            imagemUrl: "https://cdn.exemplo.com/cqrs.png",
            validoAte: ate,
            categoriaId: cat
        );

        c.Nome.Should().Be("CQRS e Event Sourcing");
        c.Valor.Should().Be(799.00m);
        c.ConteudoProgramatico.Should().BeSameAs(novoConteudo);
        c.DuracaoHoras.Should().Be(60);
        c.Nivel.Should().Be("Avançado");
        c.Instrutor.Should().Be("Gregor");
        c.VagasMaximas.Should().Be(100);
        c.ImagemUrl.Should().Be("https://cdn.exemplo.com/cqrs.png");
        c.ValidoAte.Should().Be(ate);
        c.CategoriaId.Should().Be(cat);
    }

    [Theory]
    [InlineData("", 10, 10, "Intermediário", "Instrutor", 10, "Nome do curso é obrigatório")]
    [InlineData("ok", -1, 10, "Intermediário", "Instrutor", 10, "Valor do curso não pode ser negativo")]
    [InlineData("ok", 10, 0, "Intermediário", "Instrutor", 10, "Duração do curso deve ser maior que zero")]
    [InlineData("ok", 10, 10, "", "Instrutor", 10, "Nível do curso é obrigatório")]
    [InlineData("ok", 10, 10, "Intermediário", "", 10, "Instrutor é obrigatório")]
    [InlineData("ok", 10, 10, "Intermediário", "Instrutor", 0, "Número de vagas deve ser maior que zero")]
    public void AtualizarInformacoes_deve_validar_campos(
        string nome, decimal valor, int horas, string nivel, string instrutor, int vagas, string msg)
    {
        var c = new CursoBuilder().Build();
        var conteudo = new ConteudoProgramatico("r", "d", "o", "pr", "pa", "m", "r", "a", "b");

        c.Invoking(x => x.AtualizarInformacoes(
                nome, valor, conteudo, horas, nivel, instrutor, vagas))
         .Should().Throw<DomainException>()
         .WithMessage($"*{msg}*");
    }

    [Fact]
    public void AtualizarInformacoes_deve_validar_conteudo_programatico_nulo()
    {
        var c = new CursoBuilder().Build();

        c.Invoking(x => x.AtualizarInformacoes(
                "ok", 10, null!, 10, "ok", "ok", 10))
         .Should().Throw<DomainException>()
         .WithMessage("*Conteúdo programático é obrigatório*");
    }

    [Fact]
    public void AtualizarInformacoes_deve_validar_limite_de_nome()
    {
        var c = new CursoBuilder().Build();
        var nome = new string('x', 201);
        var conteudo = new ConteudoProgramatico("r", "d", "o", "pr", "pa", "m", "r", "a", "b");

        c.Invoking(x => x.AtualizarInformacoes(
                nome, 10, conteudo, 10, "ok", "ok", 10))
         .Should().Throw<DomainException>()
         .WithMessage("*Nome do curso não pode ter mais de 200 caracteres*");
    }

    // -------------------- Matrículas e lotação --------------------
    [Fact]
    public void AdicionarMatricula_deve_incrementar_ate_lotacao_e_depois_falhar()
    {
        var c = new CursoBuilder().ComVagas(2).Build();

        c.AdicionarMatricula();
        c.VagasOcupadas.Should().Be(1);
        c.TemVagasDisponiveis.Should().BeTrue();
        c.VagasDisponiveis.Should().Be(1);
        c.PodeSerMatriculado.Should().BeTrue();

        c.AdicionarMatricula();
        c.VagasOcupadas.Should().Be(2);
        c.TemVagasDisponiveis.Should().BeFalse();
        c.VagasDisponiveis.Should().Be(0);
        c.PodeSerMatriculado.Should().BeFalse();

        c.Invoking(x => x.AdicionarMatricula())
         .Should().Throw<DomainException>()
         .WithMessage("*Não há vagas disponíveis para este curso*");
    }

    // -------------------- Derivadas: expiração e matrícula possível --------------------
    [Fact]
    public void EstaExpirado_e_PodeSerMatriculado_devem_refletir_validade()
    {
        var ontem = DateTime.Now.AddDays(-1);
        var c = new CursoBuilder().ValidoAte(ontem).Build();

        c.EstaExpirado.Should().BeTrue();
        c.PodeSerMatriculado.Should().BeFalse();
    }

    // -------------------- Aulas coleção somente leitura --------------------
    [Fact]
    public void Aulas_deve_ser_somente_leitura_e_vazia_no_inicio()
    {
        var c = new CursoBuilder().Build();

        c.Aulas.Should().BeAssignableTo<System.Collections.Generic.IReadOnlyCollection<Aula>>();
        c.Aulas.Count.Should().Be(0);
    }
}
