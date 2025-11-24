using Conteudo.Domain.Entities;
using Core.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Conteudo.Infrastructure.Configurations;

[ExcludeFromCodeCoverage]
public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> entity)
    {
        entity.ToTable("Categoria");

        entity.HasKey(a => a.Id)
            .HasName("CategoriaPK");

        entity.Property(x => x.Id)
            .HasColumnName("CategoriaId")
            .HasColumnType(DatabaseTypeConstant.UniqueIdentifier)
            .IsRequired();

        entity.Property(c => c.Nome)
            .HasColumnName("Nome")
            .HasColumnType(DatabaseTypeConstant.Varchar)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(c => c.Descricao)
            .HasColumnName("Descricao")
            .HasColumnType(DatabaseTypeConstant.Varchar)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(c => c.Cor)
            .HasColumnName("Cor")
            .HasColumnType(DatabaseTypeConstant.Varchar)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(c => c.IconeUrl)
            .HasColumnName("IconeUrl")
            .HasColumnType(DatabaseTypeConstant.Varchar)
            .HasMaxLength(500);

        entity.Property(a => a.CreatedAt)
            .HasColumnName("DataCriacao")
            .HasColumnType(DatabaseTypeConstant.DateTime)
            .IsRequired();

        entity.Property(a => a.UpdatedAt)
            .HasColumnName("DataAlteracao")
            .HasColumnType(DatabaseTypeConstant.DateTime);

        entity.HasIndex(c => c.Nome)
            .HasDatabaseName("CategoriaNomeIDX")
            .IsUnique();

        entity.HasIndex(c => c.IsAtiva)
            .HasDatabaseName("CategoriaIsAtivaIDX");

        entity.HasIndex(c => c.Ordem)
            .HasDatabaseName("CategoriaOrdemIDX");

        entity.HasMany(c => c.Cursos)
            .WithOne(curso => curso.Categoria)
            .HasForeignKey(curso => curso.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
