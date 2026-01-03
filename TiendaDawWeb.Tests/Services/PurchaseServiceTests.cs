using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Implementations;
using TiendaDawWeb.Services.Interfaces;
using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using Microsoft.Data.Sqlite;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// OBJETIVO: Verificar la lógica de negocio del proceso de compra y transaccionalidad.
/// UBICACIÓN: TiendaDawWeb.Tests/Services/
/// RAZÓN: Se utiliza SQLite In-Memory en lugar de InMemoryDatabase para soportar transacciones
/// reales y restricciones de clave foránea (FK), proporcionando un entorno de prueba más fiel a producción.
/// </summary>
[TestFixture]
public class PurchaseServiceTests
{
    private ApplicationDbContext? _context;
    private SqliteConnection? _connection;
    private Mock<ICarritoService>? _carritoServiceMock;
    private Mock<IPdfService>? _pdfServiceMock;
    private Mock<IMemoryCache>? _cacheMock;
    private Mock<ILogger<PurchaseService>>? _loggerMock;
    private PurchaseService? _purchaseService;

    private ApplicationDbContext Context => _context!;
    private PurchaseService PurchaseService => _purchaseService!;

    [SetUp]
    public void Setup()
    {
        // Configuración de SQLite In-Memory (mantiene la base de datos viva mientras la conexión esté abierta)
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated(); // Asegura que el esquema de tablas se cree al inicio

        _carritoServiceMock = new Mock<ICarritoService>();
        _pdfServiceMock = new Mock<IPdfService>();
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<PurchaseService>>();

        _purchaseService = new PurchaseService(
            Context,
            _carritoServiceMock.Object,
            _pdfServiceMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _connection?.Close();
    }

    /// <summary>
    /// PRUEBA: Proceso de compra exitoso desde el carrito.
    /// OBJETIVO: Validar que cuando el carrito tiene productos disponibles, se genera una compra,
    /// se calcula el total exacto y los productos quedan bloqueados (vendidos).
    /// </summary>
    [Test]
    public async Task CreatePurchaseFromCarritoAsync_Should_Succeed_When_Carrito_Is_Valid()
    {
        // Arrange (Preparación de datos con integridad referencial)
        var usuarioId = 1L;
        var comprador = new User { Id = usuarioId, Nombre = "Comprador", Email = "comp@test.com" };
        Context.Users.Add(comprador);

        var vendedor = new User { Id = 2L, Nombre = "Vendedor", Email = "vend@test.com" };
        Context.Users.Add(vendedor);

        var product = new Product
        {
            Id = 10L,
            Nombre = "Producto Test",
            Precio = 100m,
            PropietarioId = vendedor.Id
        };
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var carritoItems = new List<CarritoItem>
        {
            new() { ProductoId = product.Id, Precio = product.Precio, UsuarioId = usuarioId }
        };

        _carritoServiceMock!.Setup(s => s.GetCarritoByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(carritoItems));

        _carritoServiceMock!.Setup(s => s.ClearCarritoAsync(usuarioId))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act (Ejecución de la lógica de compra)
        var result = await PurchaseService.CreatePurchaseFromCarritoAsync(usuarioId);

        // Assert (Verificación de resultados y estado de BD)
        Assert.That(result.IsSuccess, Is.True, result.IsFailure ? result.Error.Message : "");
        Assert.That(result.Value.Total, Is.EqualTo(100m));

        var updatedProduct = await Context.Products.FindAsync(product.Id);
        Assert.That(updatedProduct!.CompraId, Is.EqualTo(result.Value.Id), "El producto debe quedar vinculado a la compra.");

        _carritoServiceMock!.Verify(s => s.ClearCarritoAsync(usuarioId), Times.Once);
    }

    /// <summary>
    /// PRUEBA: Intento de compra con carrito vacío.
    /// OBJETIVO: Asegurar que el sistema no permite procesar compras si el usuario no tiene productos.
    /// </summary>
    [Test]
    public async Task CreatePurchaseFromCarritoAsync_Should_Fail_When_Carrito_Is_Empty()
    {
        // Arrange
        var usuarioId = 1L;
        _carritoServiceMock!.Setup(s => s.GetCarritoByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(new List<CarritoItem>()));

        // Act
        var result = await PurchaseService.CreatePurchaseFromCarritoAsync(usuarioId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(PurchaseError.EmptyCarrito));
    }

    /// <summary>
    /// PRUEBA: Conflicto de disponibilidad (Producto ya vendido).
    /// OBJETIVO: Validar que si un producto ya tiene un CompraId (vendido), el sistema
    /// aborta la nueva compra para evitar duplicidad de venta.
    /// </summary>
    [Test]
    public async Task CreatePurchaseFromCarritoAsync_Should_Fail_When_Product_Is_Already_Sold()
    {
        // Arrange
        var usuarioId = 1L;
        var seller = new User { Id = 5L, Nombre = "Seller", Email = "seller@test.com" };
        Context.Users.Add(seller);

        var buyer = new User { Id = 3L, Nombre = "Buyer", Email = "buyer@test.com" };
        Context.Users.Add(buyer);

        var existingPurchase = new Purchase { Id = 99L, CompradorId = buyer.Id, Total = 50m, FechaCompra = DateTime.UtcNow };
        Context.Purchases.Add(existingPurchase);

        var soldProduct = new Product
        {
            Id = 20L,
            Nombre = "Vendido",
            Precio = 50m,
            PropietarioId = 5L,
            CompraId = existingPurchase.Id
        };
        Context.Products.Add(soldProduct);
        await Context.SaveChangesAsync();

        var carritoItems = new List<CarritoItem>
        {
            new() { ProductoId = soldProduct.Id, Precio = soldProduct.Precio, UsuarioId = usuarioId }
        };

        _carritoServiceMock!.Setup(s => s.GetCarritoByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(carritoItems));

        // Act
        var result = await PurchaseService.CreatePurchaseFromCarritoAsync(usuarioId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("no está disponible"));
    }

    /// <summary>
    /// PRUEBA: Conflicto de disponibilidad (Producto reservado por otro).
    /// OBJETIVO: Verificar que las reservas temporales de otros usuarios son respetadas
    /// y bloquean la compra por parte de terceros.
    /// </summary>
    [Test]
    public async Task CreatePurchaseFromCarritoAsync_Should_Fail_When_Product_Is_Reserved_By_Other()
    {
        // Arrange
        var usuarioId = 1L;
        var seller = new User { Id = 6L, Nombre = "Seller", Email = "seller6@test.com" };
        Context.Users.Add(seller);

        var otherUser = new User { Id = 99L, Nombre = "Other", Email = "other@test.com" };
        Context.Users.Add(otherUser);

        var reservedProduct = new Product
        {
            Id = 30L,
            Nombre = "Reservado",
            Precio = 50m,
            PropietarioId = 6L,
            Reservado = true,
            ReservadoPor = otherUser.Id,
            ReservadoHasta = DateTime.UtcNow.AddHours(1)
        };
        Context.Products.Add(reservedProduct);
        await Context.SaveChangesAsync();

        var carritoItems = new List<CarritoItem>
        {
            new() { ProductoId = reservedProduct.Id, Precio = reservedProduct.Precio, UsuarioId = usuarioId }
        };

        _carritoServiceMock!.Setup(s => s.GetCarritoByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(carritoItems));

        // Act
        var result = await PurchaseService.CreatePurchaseFromCarritoAsync(usuarioId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("no está disponible"));
    }

    /// <summary>
    /// PRUEBA: Obtención de compra por ID.
    /// OBJETIVO: Confirmar que el servicio recupera los detalles de una compra existente.
    /// </summary>
    [Test]
    public async Task GetByIdAsync_ShouldReturnPurchase_WhenExists()
    {
        // Arrange
        var user = new User { Id = 1L, Nombre = "User", Email = "u@t.com" };
        Context.Users.Add(user);
        var purchase = new Purchase { Id = 100L, CompradorId = 1L, Total = 50m };
        Context.Purchases.Add(purchase);
        await Context.SaveChangesAsync();

        // Act
        var result = await PurchaseService.GetByIdAsync(100L);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(100L));
    }

    /// <summary>
    /// PRUEBA: Listado de compras por usuario.
    /// OBJETIVO: Verificar que se recuperan todas las compras asociadas a un cliente.
    /// </summary>
    [Test]
    public async Task GetByUserAsync_ShouldReturnPurchases()
    {
        // Arrange
        var user = new User { Id = 10L, Nombre = "User", Email = "u@t.com" };
        Context.Users.Add(user);
        Context.Purchases.Add(new Purchase { Id = 200L, CompradorId = 10L, Total = 10m });
        await Context.SaveChangesAsync();

        // Act
        var result = await PurchaseService.GetByUserAsync(10L);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count(), Is.EqualTo(1));
    }

    /// <summary>
    /// PRUEBA: Filtro por rango de fechas.
    /// OBJETIVO: Validar que el servicio filtra correctamente las compras según su fecha.
    /// </summary>
    [Test]
    public async Task GetByDateRangeAsync_ShouldFilterPurchases()
    {
        // Arrange
        var user = new User { Id = 1L, Nombre = "User", Email = "u@t.com" };
        Context.Users.Add(user);
        var now = DateTime.UtcNow;
        Context.Purchases.Add(new Purchase { Id = 300L, CompradorId = 1L, Total = 10m, FechaCompra = now.AddDays(-5) });
        Context.Purchases.Add(new Purchase { Id = 301L, CompradorId = 1L, Total = 20m, FechaCompra = now });
        await Context.SaveChangesAsync();

        // Act
        var result = await PurchaseService.GetByDateRangeAsync(now.AddDays(-1), now.AddDays(1));

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count(), Is.EqualTo(1));
        Assert.That(result.Value.First().Id, Is.EqualTo(301L));
    }

    /// <summary>
    /// PRUEBA: Integración con PdfService.
    /// OBJETIVO: Asegurar que se llama al servicio de PDF con los datos de compra correctos.
    /// </summary>
    [Test]
    public async Task GeneratePdfAsync_ShouldCallPdfService()
    {
        // Arrange
        var user = new User { Id = 1L, Nombre = "User", Email = "u@t.com" };
        Context.Users.Add(user);
        var purchase = new Purchase { Id = 400L, CompradorId = 1L, Total = 50m };
        Context.Purchases.Add(purchase);
        await Context.SaveChangesAsync();

        _pdfServiceMock!.Setup(s => s.GenerateInvoicePdfAsync(It.IsAny<Purchase>()))
            .ReturnsAsync(Result.Success<byte[], DomainError>(new byte[] { 1, 2, 3 }));

        // Act
        var result = await PurchaseService.GeneratePdfAsync(400L);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _pdfServiceMock.Verify(s => s.GenerateInvoicePdfAsync(It.IsAny<Purchase>()), Times.Once);
    }
}
