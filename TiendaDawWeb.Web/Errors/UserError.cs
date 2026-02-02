namespace TiendaDawWeb.Errors;

/// <summary>
/// Errores relacionados con usuarios.
/// </summary>
public static class UserError
{
    public static UnauthorizedError InvalidCredentials() =>
        UnauthorizedError.InvalidCredentials();

    public static UnauthorizedError Unauthorized() =>
        new("No autorizado");

    public static BusinessRuleError HasSoldProducts() =>
        new("No se puede eliminar un usuario que ha vendido productos");

    public static BusinessRuleError HasPurchases() =>
        new("No se puede eliminar un usuario que ha realizado compras");

    public static BusinessRuleError HasActiveProducts() =>
        new("No se puede eliminar un usuario con productos a la venta");

    public static NotFoundError NotFound(long id) =>
        NotFoundError.FromId(id, "Usuario");

    public static NotFoundError NotFoundByEmail(string email) =>
        new($"Usuario con email {email} no encontrado");

    public static ConflictError AlreadyExists(string email) =>
        ConflictError.Duplicate("usuario", email);
}
