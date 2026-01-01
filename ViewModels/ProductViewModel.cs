using System.ComponentModel.DataAnnotations;
using TiendaDawWeb.Models.Enums;

namespace TiendaDawWeb.ViewModels;

/// <summary>
/// ViewModel para formularios de producto
/// </summary>
public class ProductViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede tener más de 200 caracteres")]
    [Display(Name = "Nombre del Producto")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(1000, ErrorMessage = "La descripción no puede tener más de 1000 caracteres")]
    [Display(Name = "Descripción")]
    [DataType(DataType.MultilineText)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
    [Display(Name = "Precio (€)")]
    [DataType(DataType.Currency)]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [Display(Name = "Categoría")]
    public ProductCategory Categoria { get; set; }

    [Display(Name = "Imagen del Producto")]
    public IFormFile? ImagenFile { get; set; }

    public string? ImagenUrl { get; set; }
}
