namespace Pagamentos.UnitTests.Domains;
public class PagamentoTests
{
    [Fact]
    public void Deve_criar_pagamento_com_propriedades_definidas()
    {
        var cobrancaId = Guid.NewGuid();
        var alunoId = Guid.NewGuid();

        var p = new PagamentoBuilder()
            .ComCobrancaId(cobrancaId)
            .ComAlunoId(alunoId)
            .ComStatus("Criado")
            .ComValor(250.75m)
            .ComNomeCartao("Fulano de Tal")
            .ComNumeroCartao("4111111111111111")
            .ComExpiracao("12/2030")
            .ComCvv("123")
            .Build();

        p.Id.Should().NotBeEmpty(); // vindo de Entidade base
        p.CobrancaCursoId.Should().Be(cobrancaId);
        p.AlunoId.Should().Be(alunoId);
        p.Status.Should().Be("Criado");
        p.Valor.Should().Be(250.75m);
        p.NomeCartao.Should().Be("Fulano de Tal");
        p.NumeroCartao.Should().Be("4111111111111111");
        p.ExpiracaoCartao.Should().Be("12/2030");
        p.CvvCartao.Should().Be("123");
        p.Transacao.Should().BeNull(); // padrão atual
    }

    [Fact]
    public void Deve_permitir_atualizar_status_e_valor()
    {
        var p = new PagamentoBuilder().Build();

        p.Status = "Aprovado";
        p.Valor = 1234.56m;

        p.Status.Should().Be("Aprovado");
        p.Valor.Should().Be(1234.56m);
    }

    [Fact]
    public void Deve_permitir_atualizar_dados_do_cartao()
    {
        var p = new PagamentoBuilder().Build();

        p.NomeCartao = "Ciclano";
        p.NumeroCartao = "5555444433331111";
        p.ExpiracaoCartao = "01/2031";
        p.CvvCartao = "999";

        p.NomeCartao.Should().Be("Ciclano");
        p.NumeroCartao.Should().Be("5555444433331111");
        p.ExpiracaoCartao.Should().Be("01/2031");
        p.CvvCartao.Should().Be("999");
    }
}
