using TiendaDawWeb.Data.Abstractions;

namespace TiendaDawWeb.Models;

/// <summary>
/// Clase base para entidades con auditoría de timestamps.
/// </summary>
public abstract class AuditableEntity : ITimestamped
{
    /// <summary>Fecha de creación (se asigna en INSERT).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Fecha de última modificación (se asigna en INSERT/UPDATE).</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>ID del usuario que creó el registro.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>ID del usuario que realizó la última modificación.</summary>
    public string? UpdatedBy { get; set; }
}
