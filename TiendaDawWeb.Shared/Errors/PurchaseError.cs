namespace TiendaDawWeb.Shared.Errors;

/// <summary>
///     Errores relacionados con compras.
/// </summary>
public static class PurchaseError {
    public static BusinessRuleError EmptyCarrito() {
        return new BusinessRuleError("No se puede crear una compra con el carrito vacío");
    }

    public static ForbiddenError Unauthorized() {
        return new ForbiddenError("No tienes permiso para ver esta compra");
    }

    public static NotFoundError NotFound(long id) {
        return NotFoundError.FromId(id, "Compra");
    }

    public static BusinessRuleError ProductNotAvailable(string productName) {
        return new BusinessRuleError($"El producto '{productName}' ya no está disponible");
    }

    public static BusinessRuleError InsufficientStock(string productName) {
        return new BusinessRuleError($"Stock insuficiente para '{productName}'");
    }

    public static InternalError PdfGenerationFailed(string message) {
        return new InternalError($"Error al generar PDF: {message}");
    }
}
