using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Shared.Dto.Stats;

/// <summary>
/// DTO para ventas por categoría.
/// </summary>
public class CategorySalesDto
{
    /// <summary>Categoría del producto.</summary>
    public ProductCategory Categoria { get; set; }
    /// <summary>Cantidad de ventas en la categoría.</summary>
    public int Cantidad { get; set; }
    /// <summary>Porcentaje de ventas sobre el total.</summary>
    public double Porcentaje { get; set; }
}