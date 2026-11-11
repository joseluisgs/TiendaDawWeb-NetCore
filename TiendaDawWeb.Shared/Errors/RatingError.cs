namespace TiendaDawWeb.Shared.Errors;

/// <summary>
///     Errores relacionados con valoraciones.
/// </summary>
public static class RatingError {
    public static BusinessRuleError ProductNotPurchased() {
        return new BusinessRuleError("Solo puedes valorar productos que hayas comprado");
    }

    public static ValidationError InvalidRating() {
        return ValidationError.Create("La puntuación debe estar entre 1 y 5");
    }

    public static BusinessRuleError AlreadyRated() {
        return new BusinessRuleError("Ya has valorado este producto");
    }

    public static ForbiddenError Unauthorized() {
        return new ForbiddenError("No tienes permiso para modificar esta valoración");
    }

    public static NotFoundError NotFound(long id) {
        return NotFoundError.FromId(id, "Valoración");
    }

    public static NotFoundError ProductNotFound(long productoId) {
        return NotFoundError.FromId(productoId, "Producto");
    }
}
