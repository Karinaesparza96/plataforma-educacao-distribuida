using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pagamentos.Domain.Entities;
using Pagamentos.Domain.Enum;
using Pagamentos.Infrastructure.Context;
using Pagamentos.Infrastructure.Repositories;
using Pagamentos.UnitTests.Repositories.Infra;

namespace Pagamentos.UnitTests.Repositories;
public class PagamentoRepositoryTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly PagamentoContext _ctx;
    private readonly PagamentoRepository _repo;

    public PagamentoRepositoryTests()
    {
        var (options, conn) = EfSqliteInMemoryFactory.Create();
        _conn = conn;
        _ctx = new PagamentoContext(options);
        _repo = new PagamentoRepository(_ctx);
    }

    // ------------ Helpers ------------
    private static Domain.Entities.Pagamento NovoPagamento(
        Guid? cobrancaId = null,
        Guid? alunoId = null,
        string status = "Criado",
        decimal valor = 123.45m)
    => new()
    {
        CobrancaCursoId = cobrancaId ?? Guid.NewGuid(),
        AlunoId = alunoId ?? Guid.NewGuid(),
        Status = status,
        Valor = valor,
        NomeCartao = "Fulano",
        NumeroCartao = "4111111111111111",
        ExpiracaoCartao = "12/2030",
        CvvCartao = "123"
    };

    private static Transacao NovaTransacao(Guid pagamentoId, Guid? cobrancaId = null, decimal total = 123.45m, int status = 1)
    => new()
    {
        PagamentoId = pagamentoId,
        CobrancaCursoId = cobrancaId ?? Guid.NewGuid(),
        Total = total,
        StatusTransacao = (StatusTransacao)status
    };

    // ------------ Tests ------------
    [Fact]
    public async Task Adicionar_e_Commit_devem_persistir_pagamento()
    {
        var p = NovoPagamento();

        _repo.Adicionar(p);
        var ok = await _repo.UnitOfWork.Commit();

        ok.Should().BeTrue();
        (await _repo.ObterPorId(p.Id))!.Id.Should().Be(p.Id);
    }

    [Fact]
    public async Task AdicionarTransacao_e_incluir_na_consulta_por_Id()
    {
        var p = NovoPagamento();
        _repo.Adicionar(p);
        await _repo.UnitOfWork.Commit();

        var t = NovaTransacao(p.Id, p.CobrancaCursoId, total: p.Valor, status: 2);
        _repo.AdicionarTransacao(t);
        await _repo.UnitOfWork.Commit();

        var carregado = await _repo.ObterPorId(p.Id);
        carregado.Should().NotBeNull();
        carregado!.Transacao.Should().NotBeNull();
        carregado.Transacao!.PagamentoId.Should().Be(p.Id);
        ((int)carregado.Transacao.StatusTransacao).Should().Be(2);
    }

    [Fact]
    public async Task ObterTodos_deve_incluir_transacao_e_ser_no_tracking()
    {
        // Arrange: 2 pagamentos, 1 com transação
        var p1 = NovoPagamento();
        var p2 = NovoPagamento(status: "Aprovado", valor: 999m);
        _repo.Adicionar(p1);
        _repo.Adicionar(p2);
        await _repo.UnitOfWork.Commit();

        _repo.AdicionarTransacao(NovaTransacao(p2.Id, p2.CobrancaCursoId, p2.Valor, status: 3));
        await _repo.UnitOfWork.Commit();

        // Act
        var lista = (await _repo.ObterTodos()).ToList();

        // Assert
        lista.Should().HaveCount(2);
        var comTransacao = lista.Single(x => x.Id == p2.Id);
        comTransacao.Transacao.Should().NotBeNull();

        // sanity check de no-tracking: alterar um campo em memória não marca estado.
        comTransacao.Status = "AlteradoLocal"; // não deve afetar ChangeTracker do contexto
        _ctx.ChangeTracker.Entries().Should().NotContain(x => x.State != EntityState.Unchanged);
    }

    public void Dispose()
    {
        _conn.Dispose();
        GC.SuppressFinalize(this);
    }
}

