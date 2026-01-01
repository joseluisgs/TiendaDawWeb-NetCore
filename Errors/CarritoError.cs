using TiendaDawWeb.Errors;

namespace TiendaDawWeb.Errors;

/// <summary>
/// Errores específicos del carrito de compras
/// </summary>
public record CarritoError : DomainError
{
    private CarritoError(string code, string message) : base(code, message) { }

    public static CarritoError ItemNotFound(long id) =>
        new("CARRITO_ITEM_NOT_FOUND", $"Item del carrito con ID {id} no encontrado");

    public static CarritoError ProductNotAvailable(long productId) =>
        new("PRODUCT_NOT_AVAILABLE", $"El producto con ID {productId} no está disponible");

    public static CarritoError InsufficientStock(long productId) =>
        new("INSUFFICIENT_STOCK", $"Stock insuficiente para el producto con ID {productId}");

    public static CarritoError ConcurrencyConflict() =>
        new("CARRITO_CONCURRENCY_CONFLICT", "El carrito fue modificado por otro proceso. Por favor, intenta de nuevo.");

    public static CarritoError InvalidQuantity(int quantity) =>
        new("INVALID_QUANTITY", $"La cantidad {quantity} no es válida");

    public static CarritoError CarritoEmpty() =>
        new("CARRITO_EMPTY", "El carrito está vacío");
}
