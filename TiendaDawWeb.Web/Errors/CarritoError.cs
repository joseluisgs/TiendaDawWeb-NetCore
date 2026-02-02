namespace TiendaDawWeb.Errors;

/// <summary>
/// Errores específicos del carrito de compras.
/// </summary>
public static class CarritoError
{
    public static NotFoundError ItemNotFound(long id) =>
        NotFoundError.FromId(id, "Item del carrito");

    public static BusinessRuleError ProductNotAvailable(long productId) =>
        new($"El producto con ID {productId} no está disponible");

    public static BusinessRuleError ProductNotAvailableWithName(string productName) =>
        new($"El producto '{productName}' no está disponible o está reservado por otro usuario");

    public static ConflictError ProductAlreadyInCart(long productId) =>
        new($"El producto con ID {productId} ya está en el carrito");

    public static ConflictError ProductAlreadyInCartWithName(string productName) =>
        new($"El producto '{productName}' ya está en el carrito");

    public static BusinessRuleError InsufficientStock(long productId) =>
        new($"Stock insuficiente para el producto con ID {productId}");

    public static ConflictError ConcurrencyConflict() =>
        new("El carrito fue modificado por otro proceso. Por favor, intenta de nuevo.");

    public static BusinessRuleError CarritoEmpty() =>
        new("El carrito está vacío");
}
