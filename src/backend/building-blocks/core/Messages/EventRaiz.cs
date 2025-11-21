using FluentValidation.Results;
using MediatR;

namespace Core.Messages;

public class EventRaiz : INotification
{
    public Guid RaizAgregacao { get; internal set; }

    public DateTime DataHora { get; private set; }
    public ValidationResult Validacao { get; internal set; }

    protected EventRaiz()
    {
        DataHora = DateTime.Now;
    }

    public void DefinirRaizAgregacao(Guid raizAgregacao)
    {
        RaizAgregacao = raizAgregacao;
    }

    public void DefinirValidacao(ValidationResult validacao)
    {
        Validacao = validacao;
    }

    public ICollection<string> Erros => Validacao?.Errors?.Select(e => e.ErrorMessage).ToList() ?? [];

    public virtual bool EstaValido() => Validacao == null || Validacao.IsValid;
}
