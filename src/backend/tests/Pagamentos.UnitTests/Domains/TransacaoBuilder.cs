using Pagamentos.Domain.Entities;
using Pagamentos.Domain.Enum;

namespace Pagamentos.UnitTests.Domains;
public class TransacaoBuilder
{
    private Guid _cobrancaCursoId = Guid.NewGuid();
    private Guid _pagamentoId = Guid.NewGuid();
    private decimal _total = 100m;
    private StatusTransacao _status = default; // geralmente 0
    private Domain.Entities.Pagamento? _pagamento = null;

    public TransacaoBuilder ComCobrancaId(Guid v) { _cobrancaCursoId = v; return this; }
    public TransacaoBuilder ComPagamentoId(Guid v) { _pagamentoId = v; return this; }
    public TransacaoBuilder ComTotal(decimal v) { _total = v; return this; }
    public TransacaoBuilder ComStatus(StatusTransacao v) { _status = v; return this; }

    /// <summary>
    /// Define a navegação e sincroniza PagamentoId com o Id do pagamento.
    /// </summary>
    public TransacaoBuilder ComPagamento(Domain.Entities.Pagamento p)
    {
        _pagamento = p;
        _pagamentoId = p.Id;
        return this;
    }

    public Transacao Build()
        => new Transacao
        {
            CobrancaCursoId = _cobrancaCursoId,
            PagamentoId = _pagamentoId,
            Total = _total,
            StatusTransacao = _status,
            Pagamento = _pagamento!
        };
}
