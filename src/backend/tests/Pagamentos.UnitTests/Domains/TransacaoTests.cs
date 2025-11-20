using Pagamentos.Domain.Enum;

namespace Pagamentos.UnitTests.Domains;
public class TransacaoTests
{
    [Fact]
    public void Deve_criar_transacao_com_propriedades_definidas()
    {
        var cobrancaId = Guid.NewGuid();
        var pagamentoId = Guid.NewGuid();

        var t = new TransacaoBuilder()
            .ComCobrancaId(cobrancaId)
            .ComPagamentoId(pagamentoId)
            .ComTotal(250.75m)
            // usamos um cast para não depender de nomes específicos do enum
            .ComStatus((StatusTransacao)1)
            .Build();

        t.Id.Should().NotBeEmpty(); // herdado de Entidade
        t.CobrancaCursoId.Should().Be(cobrancaId);
        t.PagamentoId.Should().Be(pagamentoId);
        t.Total.Should().Be(250.75m);
        ((int)t.StatusTransacao).Should().Be(1);
        t.Pagamento.Should().BeNull(); // não setado
    }

    [Fact]
    public void Deve_permitir_atualizar_campos_escalares()
    {
        var t = new TransacaoBuilder().Build();

        var novoCobranca = Guid.NewGuid();
        var novoPagamento = Guid.NewGuid();

        t.CobrancaCursoId = novoCobranca;
        t.PagamentoId = novoPagamento;
        t.Total = 999.99m;
        t.StatusTransacao = (StatusTransacao)2;

        t.CobrancaCursoId.Should().Be(novoCobranca);
        t.PagamentoId.Should().Be(novoPagamento);
        t.Total.Should().Be(999.99m);
        ((int)t.StatusTransacao).Should().Be(2);
    }

    [Fact]
    public void Deve_associar_pagamento_navegacao_e_sincronizar_pagamentoId()
    {
        // Pagamento é anêmico também; usamos valores simples
        var pagamento = new Domain.Entities.Pagamento
        {
            CobrancaCursoId = Guid.NewGuid(),
            AlunoId = Guid.NewGuid(),
            Status = "Aprovado",
            Valor = 123.45m,
            NomeCartao = "Fulano",
            NumeroCartao = "4111111111111111",
            ExpiracaoCartao = "12/2030",
            CvvCartao = "123"
        };

        var t = new TransacaoBuilder()
            .ComPagamento(pagamento) // também sincroniza PagamentoId = pagamento.Id
            .ComTotal(123.45m)
            .Build();

        t.Pagamento.Should().NotBeNull();
        t.Pagamento!.Id.Should().Be(pagamento.Id);
        t.PagamentoId.Should().Be(pagamento.Id);
        t.Total.Should().Be(123.45m);
    }
}
