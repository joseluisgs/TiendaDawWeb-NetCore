namespace TiendaDawWeb.Shared.Errors;

/// <summary>
/// Errores relacionados con valoraciones.
/// </summary>
public static class RatingError
{
    public static BusinessRuleError ProductNotPurchased() =>
        new("Solo puedes valorar productos que hayas comprado");

    public static ValidationError InvalidRating() =>
        ValidationError.Create("La puntuación debe estar entre 1 y 5");

    public static BusinessRuleError AlreadyRated() =>
        new("Ya has valorado este producto");

    public static ForbiddenError Unauthorized() =>
        new("No tienes permiso para modificar esta valoración");

    public static NotFoundError NotFound(long id) =>
        NotFoundError.FromId(id, "Valoración");

    public static NotFoundError ProductNotFound(long productoId) =>
        NotFoundError.FromId(productoId, "Producto");
}
