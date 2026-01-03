using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TiendaDawWeb.Models;

/// <summary>
/// Entidad de usuario con soporte de ASP.NET Core Identity
/// </summary>
public class User : IdentityUser<long>
{
    // OBJETIVO: Integrar propiedades de auditoría sin romper la herencia de Identity.
    // Usamos composición o simplemente añadimos las propiedades. 
    // Como C# no soporta herencia múltiple, añadiremos las propiedades de AuditableEntity manualmente
    // o haremos que implemente una interfaz. Por simplicidad didáctica, las añadiremos aquí.

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(200)]
    public string Apellidos { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    [Required]
    public string Rol { get; set; } = "USER"; // ADMIN, USER, MODERATOR

    public bool Deleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Relaciones
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();
}
