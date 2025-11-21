using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Auth.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public DateTime DataNascimento { get; set; }

    [Required]
    [StringLength(14, MinimumLength = 11)]
    public string CPF { get; set; } = string.Empty;

    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [StringLength(20)]
    public string Genero { get; set; } = string.Empty;

    [StringLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [StringLength(50)]
    public string Estado { get; set; } = string.Empty;

    [StringLength(10)]
    public string CEP { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Foto { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public bool Ativo { get; set; } = true;

    public Guid? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}