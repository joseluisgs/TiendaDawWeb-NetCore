namespace TiendaDawWeb.Errors;

/// <summary>
///     Errores relacionados con favoritos
/// </summary>
public static class FavoriteError {
    public static DomainError AlreadyExists =>
        new ConflictError("FAVORITE_EXISTS", "Este producto ya está en tus favoritos");

    public static DomainError NotFound =>
        new NotFoundError("FAVORITE_NOT_FOUND", "Este producto no está en tus favoritos");

    public static DomainError ProductNotFound(long productId) {
        return new NotFoundError("PRODUCT_NOT_FOUND", $"Producto con ID {productId} no encontrado");
    }

    public static DomainError UserNotFound(long userId) {
        return new NotFoundError("USER_NOT_FOUND", $"Usuario con ID {userId} no encontrado");
    }

    private record ConflictError(string Code, string Message) : DomainError(Code, Message);

    private record NotFoundError(string Code, string Message) : DomainError(Code, Message);
}