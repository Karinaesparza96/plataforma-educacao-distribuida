using Alunos.Domain.Enumerators;
using FluentAssertions;
using Plataforma.Educacao.Core.Exceptions;

namespace Alunos.Tests.Domains;
public class MatriculaCursoTests
{
    // ---------- Happy path ----------
    [Fact]
    public void Deve_criar_matricula_valida()
    {
        var m = new MatriculaCursoBuilder().Build();

        m.Id.Should().NotBeEmpty();
        m.AlunoId.Should().NotBeEmpty();
        m.CursoId.Should().NotBeEmpty();
        m.NomeCurso.Should().Be("Curso Completo de DDD");
        m.Valor.Should().Be(1000.50m);
        m.DataMatricula.Date.Should().Be(DateTime.Now.Date); // tolerante a horário
        m.EstadoMatricula.Should().Be(EstadoMatriculaCurso.PendentePagamento);
        m.MatriculaCursoConcluido().Should().BeFalse();
        m.PagamentoPodeSerRealizado().Should().BeTrue();
        m.HistoricoAprendizado.Should().BeEmpty();
    }

    // ---------- Validações: GUIDs ----------
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Deve_falhar_quando_guids_invalidos(bool zeraAluno, bool zeraCurso)
    {
        var builder = new MatriculaCursoBuilder()
            .ComAlunoId(zeraAluno ? Guid.Empty : Guid.NewGuid())
            .ComCursoId(zeraCurso ? Guid.Empty : Guid.NewGuid());

        Action act = () => builder.Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e =>
                (zeraAluno && e.Contains("Aluno deve ser informado")) ||
                (zeraCurso && e.Contains("Curso deve ser informado")));
    }

    // ---------- Validações: Nome do curso ----------
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_falhar_quando_nome_curso_vazio(string nome)
    {
        var builder = new MatriculaCursoBuilder().ComNomeCurso(nome);

        Action act = () => builder.Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Nome do curso deve ser informado"));
    }

    [Theory]
    [InlineData("123456789")] // 9 chars
    [InlineData("abc")]
    public void Deve_falhar_quando_nome_curso_curto(string nome)
    {
        var builder = new MatriculaCursoBuilder().ComNomeCurso(nome);

        Action act = () => builder.Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Nome do curso deve ter entre 10 e 200 caracteres"));
    }

    [Fact]
    public void Deve_falhar_quando_nome_curso_maior_que_200()
    {
        var nome = new string('x', 201);
        var builder = new MatriculaCursoBuilder().ComNomeCurso(nome);

        Action act = () => builder.Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Nome do curso deve ter entre 10 e 200 caracteres"));
    }

    // ---------- Validações: Valor ----------
    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Deve_falhar_quando_valor_nao_positivo(double valor)
    {
        var builder = new MatriculaCursoBuilder().ComValor((decimal)valor);

        Action act = () => builder.Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Valor da matrícula deve ser maior que zero"));
    }

    // ---------- Validações: Observação ----------
    [Fact]
    public void Deve_falhar_quando_observacao_ultrapassar_2000()
    {
        var obs = new string('o', 2001);
        var builder = new MatriculaCursoBuilder().ComObservacao(obs);

        Action act = () => builder.Build();

        act.Should().Throw<DomainException>()
           .Which.Errors.Should().Contain(e => e.Contains("Observações devem ter no máximo 2000 caracteres"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_aceitar_observacao_vazia_ou_nula(string? obs)
    {
        var m = new MatriculaCursoBuilder().ComObservacao(obs ?? string.Empty).Build();
        m.Observacao.Should().Be(obs ?? string.Empty);
    }

    // ---------- Estado/Pagamento ----------
    [Fact]
    public void RegistrarPagamento_deve_mudar_estado_para_pagamento_realizado()
    {
        var m = new MatriculaCursoBuilder().Build();

        m.RegistrarPagamentoMatricula();

        m.EstadoMatricula.Should().Be(EstadoMatriculaCurso.PagamentoRealizado);
        m.PagamentoPodeSerRealizado().Should().BeFalse();
        m.MatriculaCursoDisponivel().Should().BeTrue();
    }

    // ---------- Registro de histórico ----------
    [Fact]
    public void Deve_falhar_ao_registrar_historico_se_matricula_nao_disponivel()
    {
        var m = new MatriculaCursoBuilder().Build(); // ainda pendente de pagamento

        Action act = () => m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 10);

        act.Should().Throw<DomainException>()
           .WithMessage("*Matrícula não está disponível para registrar histórico de aprendizado*");
    }

    [Fact]
    public void Deve_registrar_historico_quando_pago()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        var aulaId = Guid.NewGuid();

        m.RegistrarHistoricoAprendizado(aulaId, "Aula 1", 8);

        m.HistoricoAprendizado.Should().HaveCount(1);
        var h = m.HistoricoAprendizado.Single();
        h.AulaId.Should().Be(aulaId);
        h.NomeAula.Should().Be("Aula 1");
        h.CargaHoraria.Should().Be(8);
        h.DataTermino.Should().BeNull();
    }

    [Fact]
    public void ReRegistro_da_mesma_aula_deve_preservar_data_inicio_e_atualizar_registro()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        var aulaId = Guid.NewGuid();

        m.RegistrarHistoricoAprendizado(aulaId, "Aula 1", 5);
        var h1 = m.HistoricoAprendizado.Single();
        var inicioOriginal = h1.DataInicio;

        // Re-registra em andamento (sem término) — substitui, preservando DataInicio
        m.RegistrarHistoricoAprendizado(aulaId, "Aula 1 - atualização", 6);

        m.HistoricoAprendizado.Should().HaveCount(1);
        var h2 = m.HistoricoAprendizado.Single();
        h2.DataInicio.Should().Be(inicioOriginal); // preservado
        h2.CargaHoraria.Should().Be(6);
        h2.NomeAula.Should().Be("Aula 1 - atualização");
    }

    [Fact]
    public void Nao_deve_permitir_reRegistro_de_aula_ja_concluida()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        var aulaId = Guid.NewGuid();

        m.RegistrarHistoricoAprendizado(aulaId, "Aula 1", 5);
        // concluir aula
        m.RegistrarHistoricoAprendizado(aulaId, "Aula 1", 5, DateTime.Now);

        Action act = () => m.RegistrarHistoricoAprendizado(aulaId, "Aula 1", 5);

        act.Should().Throw<DomainException>()
           .WithMessage("*Esta aula já foi concluída*");
    }

    [Fact]
    public void Quantidades_de_aulas_em_andamento_e_finalizadas_devem_refletir_registros()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        var aula1 = Guid.NewGuid();
        var aula2 = Guid.NewGuid();

        m.RegistrarHistoricoAprendizado(aula1, "Aula 1", 5); // em andamento
        m.RegistrarHistoricoAprendizado(aula2, "Aula 2", 7, DateTime.Now); // finalizada

        m.QuantidadeAulasEmAndamento().Should().Be(1);
        m.QuantidadeAulasFinalizadas().Should().Be(1);
        m.ObterQuantidadeAulasRegistradas().Should().Be(2);
        m.QuantidadeTotalCargaHoraria().Should().Be((short)12);
    }

    // ---------- Conclusão de curso ----------
    [Fact]
    public void Deve_falhar_concluir_quando_existirem_aulas_em_andamento()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 5); // em andamento

        m.Invoking(x => x.ConcluirCurso())
         .Should().Throw<DomainException>()
         .WithMessage("*Não é possível concluir o curso, existem aulas não finalizadas*");
    }

    [Fact]
    public void Deve_concluir_curso_quando_todas_aulas_finalizadas()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 5, DateTime.Now);

        m.Invoking(x => x.ConcluirCurso()).Should().NotThrow();

        m.MatriculaCursoConcluido().Should().BeTrue();
        m.EstadoMatricula.Should().Be(EstadoMatriculaCurso.Concluido);
        m.DataConclusao.Should().NotBeNull();
        m.MatriculaCursoDisponivel().Should().BeFalse();
    }

    [Fact]
    public void Nao_deve_concluir_duas_vezes()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 5, DateTime.Now);
        m.ConcluirCurso();

        m.Invoking(x => x.ConcluirCurso())
         .Should().Throw<DomainException>()
         .WithMessage("*Curso já foi concluído*");
    }

    // ---------- Certificado ----------
    [Fact]
    public void Deve_falhar_solicitar_certificado_antes_de_concluir()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 5, DateTime.Now);

        m.Invoking(x => x.RequisitarCertificadoConclusao(8m, "certs/1.pdf", "Instrutor"))
         .Should().Throw<DomainException>()
         .WithMessage("*Certificado só pode ser solicitado após a conclusão do curso*");
    }

    [Fact]
    public void Deve_solicitar_certificado_apos_concluir_e_bloquear_nova_solicitacao()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 5, DateTime.Now);
        m.ConcluirCurso();

        m.Invoking(x => x.RequisitarCertificadoConclusao(9m, "certs/ok.pdf", "Instrutor"))
         .Should().NotThrow();

        m.Certificado.Should().NotBeNull();
        m.Certificado!.NomeCurso.Should().Be(m.NomeCurso);
        m.Certificado.CargaHoraria.Should().Be(5);
        m.Certificado.NotaFinal.Should().Be(9m);

        m.Invoking(x => x.RequisitarCertificadoConclusao(7m, "certs/2.pdf", "Instrutor"))
         .Should().Throw<DomainException>()
         .WithMessage("*Certificado já foi solicitado para esta matrícula*");
    }

    [Fact]
    public void AtualizarNotaFinal_deve_validar_e_exigir_certificado()
    {
        var m = new MatriculaCursoBuilder().BuildPago();

        // Sem certificado
        m.Invoking(x => x.AtualizarNotaFinalCurso(9))
         .Should().Throw<DomainException>()
         .WithMessage("*Certificado não foi solicitado para esta matrícula*");

        // Com certificado
        m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 5, DateTime.Now);
        m.ConcluirCurso();
        m.RequisitarCertificadoConclusao(8m, "certs/ok.pdf", "Instrutor");

        // Nota inválida
        m.Invoking(x => x.AtualizarNotaFinalCurso(255)) // >10
         .Should().Throw<DomainException>()
         .Which.Errors.Should().Contain(e => e.Contains("Nota final deve estar entre 0 e 10"));

        // Nota válida
        m.Invoking(x => x.AtualizarNotaFinalCurso(9))
         .Should().NotThrow();

        m.Certificado!.NotaFinal.Should().Be(9);
    }

    // ---------- Cálculo de média ----------
    [Fact]
    public void CalcularMediaFinal_deve_falhar_sem_aulas_realizadas()
    {
        var m = new MatriculaCursoBuilder().BuildPago();

        m.Invoking(x => x.CalcularMediaFinalCurso())
         .Should().Throw<DomainException>()
         .WithMessage("*Não é possível concluir um curso sem aulas realizadas*");
    }

    [Fact]
    public void CalcularMediaFinal_deve_retornar_valor_quando_existem_aulas()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 5);

        var media = m.CalcularMediaFinalCurso();
        media.Should().Be(10.00m); // implementação atual
    }

    // ---------- ToString ----------
    [Fact]
    public void ToString_deve_refletir_ids_e_status()
    {
        var aluno = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var curso = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var m = new MatriculaCursoBuilder()
            .ComAlunoId(aluno)
            .ComCursoId(curso)
            .Build();

        var s = m.ToString();
        s.Should().Contain(curso.ToString());
        s.Should().Contain(aluno.ToString());
        s.Should().Contain("Concluído? Não");
    }

    [Fact]
    public void ToString_deve_mostrar_concluido_sim_quando_finalizado()
    {
        var m = new MatriculaCursoBuilder().BuildPago();
        m.RegistrarHistoricoAprendizado(Guid.NewGuid(), "Aula 1", 3, DateTime.Now);
        m.ConcluirCurso();

        m.ToString().Should().Contain("Concluído? Sim");
    }
}
