namespace TiendaDawWeb.Errors;

/// <summary>
/// Errores relacionados con usuarios
/// </summary>
public static class UserError
{
    public static DomainError NotFound(long id) =>
        new NotFoundError("USER_NOT_FOUND", $"Usuario con ID {id} no encontrado");

    public static DomainError NotFoundByEmail(string email) =>
        new NotFoundError("USER_NOT_FOUND", $"Usuario con email {email} no encontrado");

    public static DomainError AlreadyExists(string email) =>
        new ConflictError("USER_EXISTS", $"Ya existe un usuario con email {email}");

    public static DomainError InvalidCredentials =>
        new UnauthorizedError("INVALID_CREDENTIALS", "Email o contraseña incorrectos");

    public static DomainError Unauthorized =>
        new UnauthorizedError("UNAUTHORIZED", "No autorizado");

    public static DomainError HasSoldProducts =>
        new BusinessError("USER_HAS_SOLD_PRODUCTS", "No se puede eliminar un usuario que ha vendido productos");

    public static DomainError HasPurchases =>
        new BusinessError("USER_HAS_PURCHASES", "No se puede eliminar un usuario que ha realizado compras");

    private record NotFoundError(string Code, string Message) : DomainError(Code, Message);
    private record ConflictError(string Code, string Message) : DomainError(Code, Message);
    private record UnauthorizedError(string Code, string Message) : DomainError(Code, Message);
    private record BusinessError(string Code, string Message) : DomainError(Code, Message);
}
