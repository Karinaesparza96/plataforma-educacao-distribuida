using Core.Communication;
using FluentValidation.Results;
using MediatR;

namespace Core.Messages;

public abstract class RaizCommand : IRequest<CommandResult>
{
    private readonly CommandResult _resultado;

    public Guid RaizAgregacao { get; private set; }
    public DateTime DataHora { get; } = DateTime.Now;
    public ValidationResult Validacao { get; private set; } = new();

    public CommandResult Resultado => _resultado;

    protected RaizCommand()
    {
        _resultado = new CommandResult(Validacao);
    }

    public void DefinirRaizAgregacao(Guid raizAgregacao) => RaizAgregacao = raizAgregacao;

    public void DefinirValidacao(ValidationResult validacao)
    {
        Validacao = validacao;
        _resultado.AtualizarValidationResult(validacao);
    }

    public IEnumerable<string> Erros => Validacao?.Errors?.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>();

    public bool EstaValido() => Validacao?.IsValid != false;
}
