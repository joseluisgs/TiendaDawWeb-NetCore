namespace TiendaDawWeb.Shared.Errors;

/// <summary>
///     Errores relacionados con favoritos.
/// </summary>
public static class FavoriteError {
    public static ConflictError AlreadyExists() {
        return new ConflictError("Este producto ya está en tus favoritos");
    }

    public static BusinessRuleError NotFound() {
        return new BusinessRuleError("Este producto no está en tus favoritos");
    }

    public static NotFoundError ProductNotFound(long productId) {
        return NotFoundError.FromId(productId, "Producto");
    }

    public static NotFoundError UserNotFound(long userId) {
        return NotFoundError.FromId(userId, "Usuario");
    }
}
