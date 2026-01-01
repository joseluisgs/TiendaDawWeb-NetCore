namespace TiendaDawWeb.Models;

/// <summary>
/// Relación Many-to-Many entre Usuario y Producto para Favoritos
/// </summary>
public class Favorite
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long UsuarioId { get; set; }
    public virtual User Usuario { get; set; } = null!;

    public long ProductoId { get; set; }
    public virtual Product Producto { get; set; } = null!;
}
