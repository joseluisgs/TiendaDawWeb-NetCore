namespace TiendaDawWeb.Errors;

/// <summary>
/// Clase base abstracta para errores de dominio
/// </summary>
public abstract record DomainError(string Code, string Message);
