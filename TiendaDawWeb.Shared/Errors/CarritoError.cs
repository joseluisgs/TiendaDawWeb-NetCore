namespace TiendaDawWeb.Shared.Errors;

/// <summary>
///     Errores específicos del carrito de compras.
/// </summary>
public static class CarritoError {
    public static NotFoundError ItemNotFound(long id) {
        return NotFoundError.FromId(id, "Item del carrito");
    }

    public static BusinessRuleError ProductNotAvailable(long productId) {
        return new BusinessRuleError($"El producto con ID {productId} no está disponible");
    }

    public static BusinessRuleError ProductNotAvailableWithName(string productName) {
        return new BusinessRuleError(
            $"El producto '{productName}' no está disponible o está reservado por otro usuario");
    }

    public static ConflictError ProductAlreadyInCart(long productId) {
        return new ConflictError($"El producto con ID {productId} ya está en el carrito");
    }

    public static ConflictError ProductAlreadyInCartWithName(string productName) {
        return new ConflictError($"El producto '{productName}' ya está en el carrito");
    }

    public static BusinessRuleError InsufficientStock(long productId) {
        return new BusinessRuleError($"Stock insuficiente para el producto con ID {productId}");
    }

    public static ConflictError ConcurrencyConflict() {
        return new ConflictError("El carrito fue modificado por otro proceso. Por favor, intenta de nuevo.");
    }

    public static BusinessRuleError CarritoEmpty() {
        return new BusinessRuleError("El carrito está vacío");
    }
}
