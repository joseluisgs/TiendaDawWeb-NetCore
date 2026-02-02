namespace TiendaDawWeb.Errors;

/// <summary>
/// Errores relacionados con compras.
/// </summary>
public static class PurchaseError
{
    public static BusinessRuleError EmptyCarrito() =>
        new("No se puede crear una compra con el carrito vacío");

    public static ForbiddenError Unauthorized() =>
        new("No tienes permiso para ver esta compra");

    public static NotFoundError NotFound(long id) =>
        NotFoundError.FromId(id, "Compra");

    public static BusinessRuleError ProductNotAvailable(string productName) =>
        new($"El producto '{productName}' ya no está disponible");

    public static BusinessRuleError InsufficientStock(string productName) =>
        new($"Stock insuficiente para '{productName}'");

    public static InternalError PdfGenerationFailed(string message) =>
        new($"Error al generar PDF: {message}");
}
