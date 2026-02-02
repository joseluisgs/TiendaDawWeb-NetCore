namespace TiendaDawWeb.Errors;

/// <summary>
/// Errores relacionados con favoritos.
/// </summary>
public static class FavoriteError
{
    public static ConflictError AlreadyExists() =>
        new("Este producto ya está en tus favoritos");

    public static BusinessRuleError NotFound() =>
        new("Este producto no está en tus favoritos");

    public static NotFoundError ProductNotFound(long productId) =>
        NotFoundError.FromId(productId, "Producto");

    public static NotFoundError UserNotFound(long userId) =>
        NotFoundError.FromId(userId, "Usuario");
}
