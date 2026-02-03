namespace TiendaDawWeb.Shared.Errors;

/// <summary>
/// Errores del dominio de productos (HTTP 404, 409, 400).
/// </summary>
public static class ProductError
{
    /// <summary>Crea error para producto no encontrado.</summary>
    /// <param name="id">ID del producto.</param>
    /// <returns>NotFoundError (HTTP 404).</returns>
    public static NotFoundError NotFound(long id) =>
        NotFoundError.FromId(id, "Producto");

    /// <summary>Crea error para producto ya vendido.</summary>
    /// <returns>ConflictError (HTTP 409).</returns>
    public static ConflictError AlreadySold() =>
        new("Este producto ya ha sido vendido");

    /// <summary>Crea error cuando no se puede eliminar un producto vendido.</summary>
    /// <returns>BusinessRuleError (HTTP 400).</returns>
    public static BusinessRuleError CannotDeleteSold() =>
        new("No se puede eliminar un producto que ya ha sido vendido");

    /// <summary>Crea error cuando el usuario no es propietario.</summary>
    /// <param name="productId">ID del producto.</param>
    /// <returns>ForbiddenError (HTTP 403).</returns>
    public static ForbiddenError NotOwner(long productId) =>
        ForbiddenError.NotOwner("producto", productId.ToString());

    /// <summary>Crea error para precio inválido.</summary>
    /// <returns>ValidationError (HTTP 400).</returns>
    public static ValidationError InvalidPrice() =>
        ValidationError.Create("El precio debe ser mayor que cero");

    /// <summary>Crea error para datos inválidos.</summary>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>ValidationError (HTTP 400).</returns>
    public static ValidationError InvalidData(string message) =>
        ValidationError.Create(message);

    /// <summary>Crea error para datos inválidos con detalles por campo.</summary>
    /// <param name="errores">Diccionario de errores por campo.</param>
    /// <returns>ValidationError (HTTP 400).</returns>
    public static ValidationError InvalidDataWithFields(Dictionary<string, string[]> errores) =>
        ValidationError.WithFieldErrors(errores);
}
