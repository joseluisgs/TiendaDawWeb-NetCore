using FluentAssertions;
using TiendaDawWeb.Shared.Errors;

namespace TiendaDawWeb.Tests.Shared.Errors;

public class ErrorTests
{
    #region ProductError Tests

    [Test]
    public void ProductError_NotFound_ReturnsCorrectMessage()
    {
        var error = ProductError.NotFound(123);

        error.Should().NotBeNull();
        error.Message.Should().Contain("123");
        error.Message.Should().Contain("no encontrado");
    }

    [Test]
    public void ProductError_AlreadySold_ReturnsCorrectMessage()
    {
        var error = ProductError.AlreadySold();

        error.Message.Should().Contain("vendido");
    }

    [Test]
    public void ProductError_CannotDeleteSold_ReturnsCorrectMessage()
    {
        var error = ProductError.CannotDeleteSold();

        error.Message.Should().Contain("vendido");
    }

    [Test]
    public void ProductError_NotOwner_ReturnsCorrectMessage()
    {
        var error = ProductError.NotOwner(456);

        error.Message.Should().Contain("456");
        error.Message.Should().Contain("permisos");
    }

    [Test]
    public void ProductError_InvalidPrice_ReturnsCorrectMessage()
    {
        var error = ProductError.InvalidPrice();

        error.Message.Should().Contain("precio");
        error.Message.Should().Contain("cero");
    }

    [Test]
    public void ProductError_InvalidData_ReturnsMessage()
    {
        var error = ProductError.InvalidData("Test error message");

        error.Message.Should().Be("Test error message");
    }

    [Test]
    public void ProductError_InvalidDataWithFields_ReturnsValidationError()
    {
        var fieldErrors = new Dictionary<string, string[]>
        {
            { "Nombre", new[] { "El nombre es obligatorio" } },
            { "Precio", new[] { "El precio debe ser mayor que cero" } }
        };

        var error = ProductError.InvalidDataWithFields(fieldErrors);

        error.Should().NotBeNull();
        error.Message.Should().Be("Errores de validación");
        error.ValidationErrors.Should().ContainKey("Nombre");
        error.ValidationErrors.Should().ContainKey("Precio");
    }

    #endregion

    #region CarritoError Tests

    [Test]
    public void CarritoError_ItemNotFound_ReturnsCorrectMessage()
    {
        var error = CarritoError.ItemNotFound(1);

        error.Message.Should().Contain("1");
        error.Message.Should().Contain("no encontrado");
    }

    [Test]
    public void CarritoError_ProductNotAvailable_ReturnsCorrectMessage()
    {
        var error = CarritoError.ProductNotAvailable(100);

        error.Message.Should().Contain("100");
        error.Message.Should().Contain("disponible");
    }

    [Test]
    public void CarritoError_ProductNotAvailableWithName_ReturnsCorrectMessage()
    {
        var error = CarritoError.ProductNotAvailableWithName("iPhone");

        error.Message.Should().Contain("iPhone");
        error.Message.Should().Contain("reservado");
    }

    [Test]
    public void CarritoError_ProductAlreadyInCart_ReturnsCorrectMessage()
    {
        var error = CarritoError.ProductAlreadyInCart(50);

        error.Message.Should().Contain("50");
        error.Message.Should().Contain("carrito");
    }

    [Test]
    public void CarritoError_InsufficientStock_ReturnsCorrectMessage()
    {
        var error = CarritoError.InsufficientStock(100);

        error.Message.Should().Contain("100");
        error.Message.Should().Contain("Stock insuficiente");
    }

    [Test]
    public void CarritoError_ConcurrencyConflict_ReturnsCorrectMessage()
    {
        var error = CarritoError.ConcurrencyConflict();

        error.Message.Should().Contain("modificado");
        error.Message.Should().Contain("proceso");
    }

    [Test]
    public void CarritoError_CarritoEmpty_ReturnsCorrectMessage()
    {
        var error = CarritoError.CarritoEmpty();

        error.Message.Should().Contain("vacío");
    }

    #endregion

    #region FavoriteError Tests

    [Test]
    public void FavoriteError_AlreadyExists_ReturnsCorrectMessage()
    {
        var error = FavoriteError.AlreadyExists();

        error.Message.Should().Contain("favoritos");
    }

    [Test]
    public void FavoriteError_NotFound_ReturnsCorrectMessage()
    {
        var error = FavoriteError.NotFound();

        error.Message.Should().Contain("favoritos");
    }

    [Test]
    public void FavoriteError_ProductNotFound_ReturnsCorrectMessage()
    {
        var error = FavoriteError.ProductNotFound(100);

        error.Message.Should().Contain("100");
        error.Message.Should().Contain("no encontrado");
    }

    [Test]
    public void FavoriteError_UserNotFound_ReturnsCorrectMessage()
    {
        var error = FavoriteError.UserNotFound(1);

        error.Message.Should().Contain("1");
        error.Message.Should().Contain("no encontrado");
    }

    #endregion

    #region PurchaseError Tests

    [Test]
    public void PurchaseError_EmptyCarrito_ReturnsCorrectMessage()
    {
        var error = PurchaseError.EmptyCarrito();

        error.Message.Should().Contain("carrito vacío");
    }

    [Test]
    public void PurchaseError_Unauthorized_ReturnsCorrectMessage()
    {
        var error = PurchaseError.Unauthorized();

        error.Message.Should().Contain("permiso");
    }

    [Test]
    public void PurchaseError_NotFound_ReturnsCorrectMessage()
    {
        var error = PurchaseError.NotFound(123);

        error.Message.Should().Contain("123");
        error.Message.Should().Contain("no encontrado");
    }

    [Test]
    public void PurchaseError_ProductNotAvailable_ReturnsCorrectMessage()
    {
        var error = PurchaseError.ProductNotAvailable("iPhone");

        error.Message.Should().Contain("iPhone");
        error.Message.Should().Contain("disponible");
    }

    [Test]
    public void PurchaseError_InsufficientStock_ReturnsCorrectMessage()
    {
        var error = PurchaseError.InsufficientStock("MacBook");

        error.Message.Should().Contain("MacBook");
        error.Message.Should().Contain("Stock insuficiente");
    }

    [Test]
    public void PurchaseError_PdfGenerationFailed_ReturnsCorrectMessage()
    {
        var error = PurchaseError.PdfGenerationFailed("PDF generation error");

        error.Message.Should().Contain("PDF");
        error.Message.Should().Contain("PDF generation error");
    }

    #endregion

    #region UserError Tests

    [Test]
    public void UserError_InvalidCredentials_ReturnsCorrectMessage()
    {
        var error = UserError.InvalidCredentials();

        error.Message.Should().Contain("Credenciales");
    }

    [Test]
    public void UserError_Unauthorized_ReturnsCorrectMessage()
    {
        var error = UserError.Unauthorized();

        error.Message.Should().Be("No autorizado");
    }

    [Test]
    public void UserError_HasSoldProducts_ReturnsCorrectMessage()
    {
        var error = UserError.HasSoldProducts();

        error.Message.Should().Contain("vendido productos");
    }

    [Test]
    public void UserError_HasPurchases_ReturnsCorrectMessage()
    {
        var error = UserError.HasPurchases();

        error.Message.Should().Contain("compras");
    }

    [Test]
    public void UserError_HasActiveProducts_ReturnsCorrectMessage()
    {
        var error = UserError.HasActiveProducts();

        error.Message.Should().Contain("a la venta");
    }

    [Test]
    public void UserError_NotFound_ReturnsCorrectMessage()
    {
        var error = UserError.NotFound(456);

        error.Message.Should().Contain("456");
        error.Message.Should().Contain("no encontrado");
    }

    [Test]
    public void UserError_NotFoundByEmail_ReturnsCorrectMessage()
    {
        var error = UserError.NotFoundByEmail("test@example.com");

        error.Message.Should().Contain("test@example.com");
        error.Message.Should().Contain("no encontrado");
    }

    [Test]
    public void UserError_AlreadyExists_ReturnsCorrectMessage()
    {
        var error = UserError.AlreadyExists("test@example.com");

        error.Message.Should().Contain("test@example.com");
        error.Message.Should().Contain("Ya existe");
    }

    #endregion

    #region RatingError Tests

    [Test]
    public void RatingError_ProductNotPurchased_ReturnsCorrectMessage()
    {
        var error = RatingError.ProductNotPurchased();

        error.Message.Should().Contain("comprado");
    }

    [Test]
    public void RatingError_InvalidRating_ReturnsCorrectMessage()
    {
        var error = RatingError.InvalidRating();

        error.Message.Should().Contain("puntuación");
        error.Message.Should().Contain("1 y 5");
    }

    [Test]
    public void RatingError_AlreadyRated_ReturnsCorrectMessage()
    {
        var error = RatingError.AlreadyRated();

        error.Message.Should().Contain("valorado");
    }

    [Test]
    public void RatingError_Unauthorized_ReturnsCorrectMessage()
    {
        var error = RatingError.Unauthorized();

        error.Message.Should().Contain("permiso");
    }

    [Test]
    public void RatingError_NotFound_ReturnsCorrectMessage()
    {
        var error = RatingError.NotFound(789);

        error.Message.Should().Contain("789");
        error.Message.Should().Contain("no encontrado");
    }

    [Test]
    public void RatingError_ProductNotFound_ReturnsCorrectMessage()
    {
        var error = RatingError.ProductNotFound(100);

        error.Message.Should().Contain("100");
        error.Message.Should().Contain("no encontrado");
    }

    #endregion

    #region GenericError Tests

    [Test]
    public void GenericError_DatabaseError_ReturnsMessage()
    {
        var error = GenericError.DatabaseError("Connection failed");

        error.Message.Should().Be("Connection failed");
    }

    [Test]
    public void GenericError_UnexpectedError_ReturnsMessage()
    {
        var error = GenericError.UnexpectedError("Unexpected");

        error.Message.Should().Be("Unexpected");
    }

    [Test]
    public void GenericError_ConcurrencyError_ReturnsMessage()
    {
        var error = GenericError.ConcurrencyError("Conflict");

        error.Message.Should().Be("Conflict");
    }

    #endregion

    #region Domain Error Base Tests

    [Test]
    public void NotFoundError_FromId_FormatsCorrectly()
    {
        var error = NotFoundError.FromId(42, "Producto");

        error.Message.Should().Be("Recurso con ID 42 no encontrado");
    }

    [Test]
    public void ValidationError_Create_SimpleMessage()
    {
        var error = ValidationError.Create("Error simple");

        error.Message.Should().Be("Error simple");
        error.ValidationErrors.Should().BeNull();
    }

    [Test]
    public void ValidationError_WithFieldErrors_HasValidationErrors()
    {
        var fieldErrors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email inválido" } }
        };

        var error = ValidationError.WithFieldErrors(fieldErrors);

        error.Message.Should().Be("Errores de validación");
        error.ValidationErrors.Should().ContainKey("Email");
    }

    [Test]
    public void UnauthorizedError_InvalidCredentials_FormatsCorrectly()
    {
        var error = UnauthorizedError.InvalidCredentials();

        error.Message.Should().Be("Credenciales inválidas");
    }

    [Test]
    public void UnauthorizedError_TokenExpired_FormatsCorrectly()
    {
        var error = UnauthorizedError.TokenExpired();

        error.Message.Should().Be("Token expirado o inválido");
    }

    [Test]
    public void ForbiddenError_NotOwner_FormatsCorrectly()
    {
        var error = ForbiddenError.NotOwner("producto", "123");

        error.Message.Should().Contain("123");
        error.Message.Should().Contain("producto");
    }

    [Test]
    public void ConflictError_Duplicate_FormatsCorrectly()
    {
        var error = ConflictError.Duplicate("email", "test@test.com");

        error.Message.Should().Contain("test@test.com");
        error.Message.Should().Contain("Ya existe");
    }

    #endregion
}
