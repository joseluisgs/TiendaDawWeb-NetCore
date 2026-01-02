namespace TiendaDawWeb.Errors;

/// <summary>
///     Errores relacionados con compras
/// </summary>
public static class PurchaseError {
    public static DomainError EmptyCarrito =>
        new BusinessError("EMPTY_CARRITO", "No se puede crear una compra con el carrito vacío");

    public static DomainError Unauthorized =>
        new ForbiddenError("UNAUTHORIZED", "No tienes permiso para ver esta compra");

    public static DomainError NotFound(long id) {
        return new NotFoundError("PURCHASE_NOT_FOUND", $"Compra con ID {id} no encontrada");
    }

    public static DomainError ProductNotAvailable(string productName) {
        return new BusinessError("PRODUCT_NOT_AVAILABLE", $"El producto '{productName}' ya no está disponible");
    }

    public static DomainError InsufficientStock(string productName) {
        return new BusinessError("INSUFFICIENT_STOCK", $"Stock insuficiente para '{productName}'");
    }

    public static DomainError PdfGenerationFailed(string message) {
        return new TechnicalError("PDF_GENERATION_FAILED", $"Error al generar PDF: {message}");
    }

    private record NotFoundError(string Code, string Message) : DomainError(Code, Message);

    private record BusinessError(string Code, string Message) : DomainError(Code, Message);

    private record ForbiddenError(string Code, string Message) : DomainError(Code, Message);

    private record TechnicalError(string Code, string Message) : DomainError(Code, Message);
}