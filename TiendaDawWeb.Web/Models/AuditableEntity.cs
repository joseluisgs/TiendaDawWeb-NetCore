namespace TiendaDawWeb.Models;

/// <summary>
/// OBJETIVO: Definir los campos comunes para el rastreo y auditoría de registros.
/// UBICACIÓN: /Models
/// RAZÓN: Proporciona una base estandarizada para que EF Core pueda identificar qué 
/// entidades deben ser auditadas automáticamente al guardar cambios.
/// </summary>
public abstract class AuditableEntity
{
    // Fecha en la que se creó el registro
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Fecha de la última modificación (opcional al inicio)
    public DateTime? UpdatedAt { get; set; }

    // ID del usuario que creó el registro (opcional, para sistemas multiusuario)
    public string? CreatedBy { get; set; }

    // ID del usuario que realizó la última modificación
    public string? UpdatedBy { get; set; }
}
