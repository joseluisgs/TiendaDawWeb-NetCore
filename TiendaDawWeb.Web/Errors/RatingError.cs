namespace TiendaDawWeb.Errors;

/// <summary>
///     Errores relacionados con valoraciones
/// </summary>
public static class RatingError {
    public static DomainError ProductNotPurchased =>
        new BusinessError("PRODUCT_NOT_PURCHASED", "Solo puedes valorar productos que hayas comprado");

    public static DomainError InvalidRating =>
        new ValidationError("INVALID_RATING", "La puntuación debe estar entre 1 y 5");

    public static DomainError AlreadyRated =>
        new BusinessError("ALREADY_RATED", "Ya has valorado este producto");

    public static DomainError Unauthorized =>
        new ForbiddenError("UNAUTHORIZED", "No tienes permiso para modificar esta valoración");

    public static DomainError NotFound(long id) {
        return new NotFoundError("RATING_NOT_FOUND", $"Valoración con ID {id} no encontrada");
    }

    public static DomainError ProductNotFound(long productoId) {
        return new NotFoundError("PRODUCT_NOT_FOUND", $"Producto con ID {productoId} no encontrado");
    }

    private record NotFoundError(string Code, string Message) : DomainError(Code, Message);

    private record BusinessError(string Code, string Message) : DomainError(Code, Message);

    private record ValidationError(string Code, string Message) : DomainError(Code, Message);

    private record ForbiddenError(string Code, string Message) : DomainError(Code, Message);
}