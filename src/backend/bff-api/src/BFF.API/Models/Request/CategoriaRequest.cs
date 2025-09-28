using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BFF.API.Models.Request;

[ExcludeFromCodeCoverage]
public class CategoriaRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cor é obrigatória")]
    [StringLength(100, ErrorMessage = "Cor deve ter no máximo 100 caracteres")]
    public string Cor { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "URL do ícone deve ter no máximo 500 caracteres")]
    public string IconeUrl { get; set; } = string.Empty;

    public int Ordem { get; set; } = 0;
}
