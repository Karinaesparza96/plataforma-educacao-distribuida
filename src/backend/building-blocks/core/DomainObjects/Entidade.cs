using System.ComponentModel.DataAnnotations;

namespace Core.DomainObjects;

public abstract class Entidade
{
    [Key]
    public Guid Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    protected Entidade()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }

    public void DefinirId(Guid id)
    {
        Id = id;
    }

    public void AtualizarDataModificacao()
    {
        UpdatedAt = DateTime.Now;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var entidade = (Entidade)obj;
        return Id == entidade.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}