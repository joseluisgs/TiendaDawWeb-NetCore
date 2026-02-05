using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Email;
using TiendaDawWeb.Shared.Services.Pdf;
using TiendaDawWeb.Shared.Services.Purchase;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.Tests.Shared.Services.Purchase;

public class PurchaseServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<ICarritoService> _carritoServiceMock = null!;
    private Mock<IPdfService> _pdfServiceMock = null!;
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<IMemoryCache> _cacheMock = null!;
    private Mock<ILogger<PurchaseService>> _loggerMock = null!;
    private PurchaseService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _carritoServiceMock = new Mock<ICarritoService>();
        _pdfServiceMock = new Mock<IPdfService>();
        _emailServiceMock = new Mock<IEmailService>();
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<PurchaseService>>();
        _service = new PurchaseService(
            _context,
            _carritoServiceMock.Object,
            _pdfServiceMock.Object,
            _emailServiceMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    #region GetByIdAsync Tests

    [Test]
    public async Task GetByIdAsync_ReturnsPurchase_WhenExists()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var purchase = new PurchaseModel { Id = 1, CompradorId = 1, Total = 100, FechaCompra = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsFailure_WhenNotExists()
    {
        var result = await _service.GetByIdAsync(999);

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region GetByUserAsync Tests

    [Test]
    public async Task GetByUserAsync_ReturnsPurchases()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var purchase1 = new PurchaseModel { Id = 1, CompradorId = 1, Total = 100, FechaCompra = DateTime.UtcNow };
        var purchase2 = new PurchaseModel { Id = 2, CompradorId = 1, Total = 200, FechaCompra = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.Purchases.AddRange(purchase1, purchase2);
        await _context.SaveChangesAsync();

        var result = await _service.GetByUserAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task GetByUserAsync_ReturnsEmpty_WhenNoPurchases()
    {
        var result = await _service.GetByUserAsync(999);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetByUserAsync_PaginatesResults()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Users.Add(user);
        for (int i = 1; i <= 15; i++)
        {
            _context.Purchases.Add(new PurchaseModel { Id = i, CompradorId = 1, Total = i * 100, FechaCompra = DateTime.UtcNow });
        }
        await _context.SaveChangesAsync();

        var result = await _service.GetByUserAsync(1, page: 1, pageSize: 5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    #endregion

    #region GetAllAsync Tests

    [Test]
    public async Task GetAllAsync_ReturnsAllPurchases()
    {
        var user1 = new User { Id = 1, Email = "test1@test.com", UserName = "test1" };
        var user2 = new User { Id = 2, Email = "test2@test.com", UserName = "test2" };
        _context.Users.AddRange(user1, user2);
        _context.Purchases.AddRange(
            new PurchaseModel { Id = 1, CompradorId = 1, Total = 100, FechaCompra = DateTime.UtcNow },
            new PurchaseModel { Id = 2, CompradorId = 2, Total = 200, FechaCompra = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task GetAllAsync_PaginatesResults()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Users.Add(user);
        for (int i = 1; i <= 15; i++)
        {
            _context.Purchases.Add(new PurchaseModel { Id = i, CompradorId = 1, Total = i * 100, FechaCompra = DateTime.UtcNow });
        }
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(page: 2, pageSize: 5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    #endregion

    #region GetByDateRangeAsync Tests

    [Test]
    public async Task GetByDateRangeAsync_ReturnsPurchasesInRange()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var now = DateTime.UtcNow;
        _context.Users.Add(user);
        _context.Purchases.AddRange(
            new PurchaseModel { Id = 1, CompradorId = 1, Total = 100, FechaCompra = now.AddDays(-5) },
            new PurchaseModel { Id = 2, CompradorId = 1, Total = 200, FechaCompra = now.AddDays(-3) },
            new PurchaseModel { Id = 3, CompradorId = 1, Total = 300, FechaCompra = now.AddDays(5) }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetByDateRangeAsync(now.AddDays(-7), now.AddDays(-1));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    #endregion
}
