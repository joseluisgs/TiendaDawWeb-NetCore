using Microsoft.EntityFrameworkCore;
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

[TestFixture]
public class PurchaseServiceTests
{
    private ApplicationDbContext _context;
    private SqliteConnection _connection;
    private Mock<ICarritoService> _carritoServiceMock;
    private Mock<IPdfService> _pdfServiceMock;
    private Mock<ILogger<PurchaseService>> _loggerMock;
    private PurchaseService _purchaseService;

    [SetUp]
    public void Setup()
    {
        // SQLite in-memory requiere que la conexión esté abierta durante todo el test
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated(); // Crear esquema

        _carritoServiceMock = new Mock<ICarritoService>();
        _pdfServiceMock = new Mock<IPdfService>();
        _loggerMock = new Mock<ILogger<PurchaseService>>();

        _purchaseService = new PurchaseService(
            _context,
            _carritoServiceMock.Object,
            _pdfServiceMock.Object,
            _loggerMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Test]
    public async Task CreatePurchaseFromCarritoAsync_Should_Succeed_When_Carrito_Is_Valid()
    {
        // Arrange
        var usuarioId = 1L;
        
        // Crear usuario comprador
        var comprador = new User { Id = usuarioId, Nombre = "Comprador", Email = "comp@test.com" };
        _context.Users.Add(comprador);
        
        // Crear vendedor y producto
        var vendedor = new User { Id = 2L, Nombre = "Vendedor", Email = "vend@test.com" };
        _context.Users.Add(vendedor);
        
        var product = new Product 
        { 
            Id = 10L, 
            Nombre = "Producto Test", 
            Precio = 100m,
            PropietarioId = vendedor.Id 
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var carritoItems = new List<CarritoItem>
        {
            new() { ProductoId = product.Id, Precio = product.Precio, UsuarioId = usuarioId }
        };

        _carritoServiceMock.Setup(s => s.GetCarritoByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(carritoItems));
        
        _carritoServiceMock.Setup(s => s.ClearCarritoAsync(usuarioId))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _purchaseService.CreatePurchaseFromCarritoAsync(usuarioId);

        // Assert
        Assert.That(result.IsSuccess, Is.True, result.IsFailure ? result.Error.Message : "");
        Assert.That(result.Value.Total, Is.EqualTo(100m));
        Assert.That(result.Value.Products.Count(), Is.EqualTo(1));
        
        // Verificar que el producto está marcado como vendido
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.That(updatedProduct!.CompraId, Is.EqualTo(result.Value.Id));
        
        _carritoServiceMock.Verify(s => s.ClearCarritoAsync(usuarioId), Times.Once);
    }

    [Test]
    public async Task CreatePurchaseFromCarritoAsync_Should_Fail_When_Carrito_Is_Empty()
    {
        // Arrange
        var usuarioId = 1L;
        _carritoServiceMock.Setup(s => s.GetCarritoByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(new List<CarritoItem>()));

        // Act
        var result = await _purchaseService.CreatePurchaseFromCarritoAsync(usuarioId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(PurchaseError.EmptyCarrito));
    }

    [Test]
    public async Task CreatePurchaseFromCarritoAsync_Should_Fail_When_Product_Is_Already_Sold()
    {
        // Arrange
        var usuarioId = 1L;
        
        var seller = new User { Id = 5L, Nombre = "Seller", Email = "seller@test.com" };
        _context.Users.Add(seller);
        
        var buyer = new User { Id = 3L, Nombre = "Buyer", Email = "buyer@test.com" };
        _context.Users.Add(buyer);

        var existingPurchase = new Purchase { Id = 99L, CompradorId = buyer.Id, Total = 50m, FechaCompra = DateTime.UtcNow };
        _context.Purchases.Add(existingPurchase);

        var soldProduct = new Product 
        { 
            Id = 20L, 
            Nombre = "Vendido", 
            Precio = 50m, 
            PropietarioId = 5L,
            CompraId = existingPurchase.Id // Ya vendido
        };
        _context.Products.Add(soldProduct);
        await _context.SaveChangesAsync();

        var carritoItems = new List<CarritoItem>
        {
            new() { ProductoId = soldProduct.Id, Precio = soldProduct.Precio, UsuarioId = usuarioId }
        };

        _carritoServiceMock.Setup(s => s.GetCarritoByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(carritoItems));

        // Act
        var result = await _purchaseService.CreatePurchaseFromCarritoAsync(usuarioId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("no está disponible"));
    }

    [Test]
    public async Task CreatePurchaseFromCarritoAsync_Should_Fail_When_Product_Is_Reserved_By_Other()
    {
        // Arrange
        var usuarioId = 1L;
        
        var seller = new User { Id = 6L, Nombre = "Seller", Email = "seller6@test.com" };
        _context.Users.Add(seller);

        var otherUser = new User { Id = 99L, Nombre = "Other", Email = "other@test.com" };
        _context.Users.Add(otherUser);

        var reservedProduct = new Product 
        { 
            Id = 30L, 
            Nombre = "Reservado", 
            Precio = 50m, 
            PropietarioId = 6L,
            Reservado = true,
            ReservadoPor = otherUser.Id, // Reservado por otro usuario
            ReservadoHasta = DateTime.UtcNow.AddHours(1)
        };
        _context.Products.Add(reservedProduct);
        await _context.SaveChangesAsync();

        var carritoItems = new List<CarritoItem>
        {
            new() { ProductoId = reservedProduct.Id, Precio = reservedProduct.Precio, UsuarioId = usuarioId }
        };

        _carritoServiceMock.Setup(s => s.GetCarritoByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(Result.Success<IEnumerable<CarritoItem>, DomainError>(carritoItems));

        // Act
        var result = await _purchaseService.CreatePurchaseFromCarritoAsync(usuarioId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("no está disponible"));
    }
}