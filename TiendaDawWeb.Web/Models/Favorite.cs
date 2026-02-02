namespace TiendaDawWeb.Models;

/// <summary>
/// Entidad de relación Many-to-Many entre Usuario y Producto para Favoritos.
/// Representa un producto marcado como favorito por un usuario.
/// </summary>
public class Favorite : AuditableEntity
{
    /// <summary>ID único del favorito (PK en SQLite).</summary>
    public long Id { get; set; }

    /// <summary>ID del usuario que marcó el producto como favorito (FK).</summary>
    public long UsuarioId { get; set; }

    /// <summary>Usuario que marcó el producto como favorito.</summary>
    public virtual User Usuario { get; set; } = null!;

    /// <summary>ID del producto marcado como favorito (FK).</summary>
    public long ProductoId { get; set; }

    /// <summary>Producto marcado como favorito.</summary>
    public virtual Product Producto { get; set; } = null!;
}
