namespace TiendaDawWeb.Errors;

/// <summary>
/// Error genérico para errores de infraestructura o base de datos
/// </summary>
public record GenericError : DomainError
{
    public GenericError(string code, string message) : base(code, message) { }

    public static GenericError DatabaseError(string message) =>
        new("DATABASE_ERROR", message);

    public static GenericError UnexpectedError(string message) =>
        new("UNEXPECTED_ERROR", message);

    public static GenericError ConcurrencyError(string message) =>
        new("CONCURRENCY_ERROR", message);
}
