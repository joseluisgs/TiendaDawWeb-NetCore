using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TiendaDawWeb.Data.Abstractions;

namespace TiendaDawWeb.Models;

/// <summary>
/// Entidad de usuario con soporte de ASP.NET Core Identity
/// </summary>
public class User : IdentityUser<long>, ITimestamped
{
    /// <summary>Fecha de creación.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Fecha de última modificación.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>ID del usuario que creó el registro.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>ID del usuario que realizó la última modificación.</summary>
    public string? UpdatedBy { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(200)]
    public string Apellidos { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    [Required]
    public string Rol { get; set; } = "USER";

    public bool Deleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();
}
