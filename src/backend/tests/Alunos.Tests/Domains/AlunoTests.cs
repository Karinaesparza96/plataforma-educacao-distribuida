using Alunos.Domain.Enumerators;
using FluentAssertions;
using Plataforma.Educacao.Core.Exceptions;

namespace Alunos.Tests.Domains;
public class AlunoTests
{
    // ------------------ Happy Path & Normalizações ------------------

    [Fact]
    public void Deve_criar_aluno_valido_e_normalizar_campos()
    {
        var a = new AlunoBuilder().Build();

        a.Id.Should().NotBeEmpty();
        a.Nome.Should().Be("Fulano de Tal");
        a.Email.Should().Be("fulano.teste@exemplo.com.br"); // lower
        a.Cpf.Should().Be("12345678909");
        a.Genero.Should().Be("M");
        a.Cidade.Should().Be("São Paulo");
        a.Estado.Should().Be("SP");
        a.Cep.Should().Be("12345678"); // sanitizado (sem -/.)
        a.Ativo.Should().BeFalse();
        a.ToString().Should().Contain("Fulano de Tal").And.Contain("fulano.teste@exemplo.com.br");
    }

    [Fact]
    public void Deve_aceitar_cpf_formatado_pontuado()
    {
        var a = new AlunoBuilder().ComCpf("123.456.789-09").Build();
        a.Cpf.Should().Be("123.456.789-09"); // validação usa apenas dígitos; propriedade mantém formato
    }

    // ------------------ Validações de construção ------------------

    [Fact]
    public void Deve_falhar_quando_codigo_auth_vazio()
    {
        Action act = () => new AlunoBuilder().ComCodigoAuth(Guid.Empty).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Código de identificação deve ser informado"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_nome_vazio(string nome)
    {
        Action act = () => new AlunoBuilder().ComNome(nome).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Nome não pode ser nulo ou vazio"));
    }

    [Theory]
    [InlineData("ab")]     // < 3
    [InlineData("a")]
    public void Deve_falhar_quando_nome_curto(string nome)
    {
        Action act = () => new AlunoBuilder().ComNome(nome).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Nome deve ter entre 3 e 100 caracteres"));
    }

    [Fact]
    public void Deve_falhar_quando_nome_longo_demais()
    {
        var nome = new string('x', 101);
        Action act = () => new AlunoBuilder().ComNome(nome).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Nome deve ter entre 3 e 100 caracteres"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_email_vazio(string email)
    {
        Action act = () => new AlunoBuilder().ComEmail(email).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Email não pode ser nulo ou vazio"));
    }

    [Theory]
    [InlineData("a@b")]        // sem TLD válido
    [InlineData("inv@")]       // incompleto
    [InlineData("inv")]        // sem @
    [InlineData("a@b.c")]      // TLD 1 char (regex exige 2+)
    public void Deve_falhar_quando_email_invalido(string email)
    {
        Action act = () => new AlunoBuilder().ComEmail(email).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Email informado é inválido"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_cpf_vazio(string cpf)
    {
        Action act = () => new AlunoBuilder().ComCpf(cpf).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("CPF não pode ser nulo ou vazio"));
    }

    [Theory]
    [InlineData("123")]           // < 11 dígitos
    [InlineData("1234567890")]    // 10
    [InlineData("123456789012")]  // 12
    public void Deve_falhar_quando_cpf_nao_tiver_11_digitos_numericos(string cpf)
    {
        Action act = () => new AlunoBuilder().ComCpf(cpf).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("CPF informado é inválido"));
    }

    [Theory]
    [InlineData("0001-01-01")]
    [InlineData("9999-12-31")]
    public void Deve_falhar_quando_data_nascimento_invalida(string data)
    {
        var d = DateTime.Parse(data);
        Action act = () => new AlunoBuilder().NascidoEm(d).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Data de nascimento"));
    }

    [Fact]
    public void Deve_falhar_quando_data_nascimento_futura()
    {
        var futuro = DateTime.Now.AddDays(1);
        Action act = () => new AlunoBuilder().NascidoEm(futuro).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Data de nascimento não pode ser superior à data atual"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_genero_vazio(string genero)
    {
        Action act = () => new AlunoBuilder().ComGenero(genero).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Genero não pode ser nulo ou vazio"));
    }

    [Fact]
    public void Deve_falhar_quando_genero_maior_que_20()
    {
        var genero = new string('x', 21);
        Action act = () => new AlunoBuilder().ComGenero(genero).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Genero deve ter entre 1 e 20 caracteres"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_cidade_vazia(string cidade)
    {
        Action act = () => new AlunoBuilder().NaCidade(cidade).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Cidade não pode ser nulo ou vazio"));
    }

    [Fact]
    public void Deve_falhar_quando_cidade_maior_que_50()
    {
        var cidade = new string('x', 51);
        Action act = () => new AlunoBuilder().NaCidade(cidade).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Cidade deve ter entre 1 e 50 caracteres"));
    }

    [Theory]
    [InlineData("S")]   // 1
    [InlineData("RJS")] // 3
    public void Deve_falhar_quando_estado_nao_tiver_2_chars(string uf)
    {
        Action act = () => new AlunoBuilder().NoEstado(uf).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Estado deve ter 2 caracteres"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_cep_vazio(string cep)
    {
        Action act = () => new AlunoBuilder().ComCep(cep).Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Cep não pode ser nulo ou vazio"));
    }

    [Fact]
    public void Deve_falhar_quando_cep_ultrapassar_8_apos_sanitizacao()
    {
        // 9 dígitos após remover máscara
        Action act = () => new AlunoBuilder().ComCep("12.345-6789").Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Cep deve ter até 8 caracteres"));
    }

    [Fact]
    public void Deve_aceitar_foto_nula_ou_vazia_e_validar_quando_presente()
    {
        // vazio -> ok
        var ok1 = new AlunoBuilder().ComFoto("").Build();
        ok1.Foto.Should().BeEmpty();

        // muito longa -> falha
        var longa = new string('f', 1025);
        Action act = () => new AlunoBuilder().ComFoto(longa).Build();
        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Foto deve ter entre 1 e 1024 caracteres"));
    }

    // ------------------ Ativar / Inativar ------------------

    [Fact]
    public void Ativar_e_inativar_aluno_devem_alterar_estado()
    {
        var a = new AlunoBuilder().Build();

        a.Ativo.Should().BeFalse();
        a.AtivarAluno();
        a.Ativo.Should().BeTrue();
        a.InativarAluno();
        a.Ativo.Should().BeFalse();
    }

    // ------------------ Matrícula em curso ------------------

    [Fact]
    public void Nao_deve_matricular_aluno_inativo()
    {
        var a = new AlunoBuilder().Build();
        a.Invoking(x => x.MatricularAlunoEmCurso(Guid.NewGuid(), "Curso DDD 2025", 100, "obs"))
         .Should().Throw<DomainException>()
         .WithMessage("*Aluno inativo não pode ser matriculado em cursos*");
    }

    [Fact]
    public void Deve_matricular_quando_ativo_e_barrar_duplicidade_mesmo_curso()
    {
        var a = new AlunoBuilder().BuildAtivo();
        var cursoId = Guid.NewGuid();

        var m = a.MatricularAlunoEmCurso(cursoId, "Curso DDD 2025", 100, "obs");
        m.CursoId.Should().Be(cursoId);

        a.Invoking(x => x.MatricularAlunoEmCurso(cursoId, "Outra desc", 200, "outra"))
         .Should().Throw<DomainException>()
         .WithMessage("*Aluno já está matriculado neste curso*");
    }

    // ------------------ Pagamento, histórico e quantidades ------------------

    [Fact]
    public void Deve_atualizar_pagamento_e_registrar_historico()
    {
        var a = new AlunoBuilder().BuildAtivo();
        var cursoId = Guid.NewGuid();
        var m = a.MatricularAlunoEmCurso(cursoId, "Curso DDD 2025", 100, "obs");

        a.AtualizarPagamentoMatricula(m.Id);

        // estado avançou
        m.EstadoMatricula.Should().Be(EstadoMatriculaCurso.PagamentoRealizado);

        var aulaId = Guid.NewGuid();
        a.RegistrarHistoricoAprendizado(m.Id, aulaId, "Aula 1", 8);

        var h = a.ObterHistoricoAprendizado(m.Id, aulaId);
        h.Should().NotBeNull();
        h!.AulaId.Should().Be(aulaId);

        // não encontrado -> null
        a.ObterHistoricoAprendizado(m.Id, Guid.NewGuid()).Should().BeNull();

        a.ObterQuantidadeAulasMatriculaCurso(m.Id).Should().Be(1);
    }

    [Fact]
    public void ObterQuantidadeAulasPendenteMatriculaCurso_deve_refletir_aulas_em_andamento()
    {
        var a = new AlunoBuilder().BuildAtivo();
        var cursoId = Guid.NewGuid();
        var m = a.MatricularAlunoEmCurso(cursoId, "Curso DDD 2025", 100, "obs");
        a.AtualizarPagamentoMatricula(m.Id);

        // Em andamento (sem término)
        a.RegistrarHistoricoAprendizado(m.Id, Guid.NewGuid(), "Aula 1", 5);

        a.ObterQuantidadeAulasPendenteMatriculaCurso(cursoId).Should().Be(1);
    }

    // ------------------ Conclusão e Certificado ------------------

    [Fact]
    public void Deve_concluir_curso_e_requisitar_certificado()
    {
        var a = new AlunoBuilder().BuildAtivo();
        var cursoId = Guid.NewGuid();
        var m = a.MatricularAlunoEmCurso(cursoId, "Curso DDD 2025", 100, "obs");
        a.AtualizarPagamentoMatricula(m.Id);

        // finaliza a aula e conclui
        a.RegistrarHistoricoAprendizado(m.Id, Guid.NewGuid(), "Aula 1", 5, DateTime.Now);
        a.ConcluirCurso(m.Id);

        m.EstadoMatricula.Should().Be(EstadoMatriculaCurso.Concluido);
        m.MatriculaCursoConcluido().Should().BeTrue();

        // solicita certificado
        a.RequisitarCertificadoConclusao(m.Id, 9m, "certs/ok.pdf", "Instrutor");
        m.Certificado.Should().NotBeNull();
        m.Certificado!.NomeCurso.Should().Be(m.NomeCurso);
    }
}
