using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TiendaDawWeb.Shared.Data.Abstractions;

namespace TiendaDawWeb.Shared.Models;

/// <summary>
/// Entidad de usuario con soporte de ASP.NET Core Identity.
/// Hereda de IdentityUser para autenticación con email/password.
/// </summary>
public class User : IdentityUser<long>, ITimestamped
{
    /// <summary>Fecha de creación del registro en UTC.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Fecha de última modificación en UTC.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>ID del usuario que creó el registro (username o ID).</summary>
    public string? CreatedBy { get; set; }

    /// <summary>ID del usuario que realizó la última modificación.</summary>
    public string? UpdatedBy { get; set; }

    /// <summary>Nombre completo del usuario (obligatorio, 1-100 caracteres).</summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Apellidos del usuario (obligatorio, 1-200 caracteres).</summary>
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(200)]
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>URL o ruta del avatar del usuario (null = sin avatar).</summary>
    public string? Avatar { get; set; }

    /// <summary>Rol del usuario (USER, ADMIN, MODERATOR).</summary>
    [Required]
    public string Rol { get; set; } = "USER";

    /// <summary>Indica si el usuario está eliminado (soft-delete).</summary>
    public bool Deleted { get; set; } = false;

    /// <summary>Fecha de eliminación en UTC (null si no eliminado).</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>ID del usuario que realizó la eliminación.</summary>
    public string? DeletedBy { get; set; }

    /// <summary>Productos publicados por el usuario.</summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>Compras realizadas por el usuario.</summary>
    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    /// <summary>Productos favoritos del usuario.</summary>
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    /// <summary>Valoraciones realizadas por el usuario.</summary>
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    /// <summary>Items en el carrito del usuario.</summary>
    public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();
}
