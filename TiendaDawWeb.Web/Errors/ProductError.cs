namespace TiendaDawWeb.Errors;

/// <summary>
///     Errores relacionados con productos
/// </summary>
public static class ProductError {
    public static DomainError AlreadySold =>
        new BusinessError("PRODUCT_SOLD", "Este producto ya ha sido vendido");

    public static DomainError CannotDeleteSold =>
        new BusinessError("CANNOT_DELETE_SOLD", "No se puede eliminar un producto que ya ha sido vendido");

    public static DomainError NotOwner =>
        new ForbiddenError("NOT_OWNER", "No eres el propietario de este producto");

    public static DomainError InvalidPrice =>
        new ValidationError("INVALID_PRICE", "El precio debe ser mayor que cero");

    public static DomainError NotFound(long id) {
        return new NotFoundError("PRODUCT_NOT_FOUND", $"Producto con ID {id} no encontrado");
    }

    public static DomainError InvalidData(string message) {
        return new ValidationError("INVALID_DATA", message);
    }

    private record NotFoundError(string Code, string Message) : DomainError(Code, Message);

    private record BusinessError(string Code, string Message) : DomainError(Code, Message);

    private record ForbiddenError(string Code, string Message) : DomainError(Code, Message);

    private record ValidationError(string Code, string Message) : DomainError(Code, Message);
}