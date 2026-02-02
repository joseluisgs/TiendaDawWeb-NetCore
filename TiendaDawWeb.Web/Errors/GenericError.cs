namespace TiendaDawWeb.Errors;

/// <summary>
/// Error genérico para errores de infraestructura o base de datos.
/// </summary>
public static class GenericError
{
    public static InternalError DatabaseError(string message) =>
        new(message);

    public static InternalError UnexpectedError(string message) =>
        new(message);

    public static InternalError ConcurrencyError(string message) =>
        new(message);
}
