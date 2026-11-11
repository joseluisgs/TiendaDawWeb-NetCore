namespace TiendaDawWeb.Shared.Errors;

/// <summary>
///     Errores relacionados con usuarios.
/// </summary>
public static class UserError {
    public static UnauthorizedError InvalidCredentials() {
        return UnauthorizedError.InvalidCredentials();
    }

    public static UnauthorizedError Unauthorized() {
        return new UnauthorizedError("No autorizado");
    }

    public static BusinessRuleError HasSoldProducts() {
        return new BusinessRuleError("No se puede eliminar un usuario que ha vendido productos");
    }

    public static BusinessRuleError HasPurchases() {
        return new BusinessRuleError("No se puede eliminar un usuario que ha realizado compras");
    }

    public static BusinessRuleError HasActiveProducts() {
        return new BusinessRuleError("No se puede eliminar un usuario con productos a la venta");
    }

    public static NotFoundError NotFound(long id) {
        return NotFoundError.FromId(id, "Usuario");
    }

    public static NotFoundError NotFoundByEmail(string email) {
        return new NotFoundError($"Usuario con email {email} no encontrado");
    }

    public static ConflictError AlreadyExists(string email) {
        return ConflictError.Duplicate("usuario", email);
    }
}
