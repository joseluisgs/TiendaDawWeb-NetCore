using TiendaDawWeb.Data.Abstractions;

namespace TiendaDawWeb.Models;

/// <summary>
/// Clase base abstracta para entidades con auditoría de timestamps.
/// Proporciona campos comunes para seguimiento de creación y modificación.
/// </summary>
public abstract class AuditableEntity : ITimestamped
{
    /// <summary>Fecha de creación del registro (se asigna en INSERT).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Fecha de última modificación (se asigna en INSERT/UPDATE).</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>ID del usuario que creó el registro.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>ID del usuario que realizó la última modificación.</summary>
    public string? UpdatedBy { get; set; }
}
