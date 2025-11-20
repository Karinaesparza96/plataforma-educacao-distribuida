namespace Pagamentos.UnitTests.Domains;
public class PagamentoBuilder
{
    private Guid _cobrancaCursoId = Guid.NewGuid();
    private Guid _alunoId = Guid.NewGuid();
    private string _status = "Criado";
    private decimal _valor = 100m;

    private string _nomeCartao = "Fulano de Tal";
    private string _numeroCartao = "4111111111111111";
    private string _expiracaoCartao = "12/2030";
    private string _cvvCartao = "123";

    public PagamentoBuilder ComCobrancaId(Guid v) { _cobrancaCursoId = v; return this; }
    public PagamentoBuilder ComAlunoId(Guid v) { _alunoId = v; return this; }
    public PagamentoBuilder ComStatus(string v) { _status = v; return this; }
    public PagamentoBuilder ComValor(decimal v) { _valor = v; return this; }

    public PagamentoBuilder ComNomeCartao(string v) { _nomeCartao = v; return this; }
    public PagamentoBuilder ComNumeroCartao(string v) { _numeroCartao = v; return this; }
    public PagamentoBuilder ComExpiracao(string v) { _expiracaoCartao = v; return this; }
    public PagamentoBuilder ComCvv(string v) { _cvvCartao = v; return this; }

    public Domain.Entities.Pagamento Build()
        => new Domain.Entities.Pagamento
        {
            CobrancaCursoId = _cobrancaCursoId,
            AlunoId = _alunoId,
            Status = _status,
            Valor = _valor,
            NomeCartao = _nomeCartao,
            NumeroCartao = _numeroCartao,
            ExpiracaoCartao = _expiracaoCartao,
            CvvCartao = _cvvCartao,
            // Transacao permanece null por padrão (tipo não fornecido aqui)
        };
}
