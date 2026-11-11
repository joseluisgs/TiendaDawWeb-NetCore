namespace TiendaDawWeb.Shared.Errors;

/// <summary>
///     Error genérico para errores de infraestructura o base de datos.
/// </summary>
public static class GenericError {
    public static InternalError DatabaseError(string message) {
        return new InternalError(message);
    }

    public static InternalError UnexpectedError(string message) {
        return new InternalError(message);
    }

    public static InternalError ConcurrencyError(string message) {
        return new InternalError(message);
    }
}
