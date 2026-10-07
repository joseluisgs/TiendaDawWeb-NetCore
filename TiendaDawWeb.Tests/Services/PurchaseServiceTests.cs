using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Email;
using TiendaDawWeb.Shared.Services.Pdf;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// Tests unitarios para PurchaseService con EF Core InMemory.
/// </summary>
public class PurchaseServiceTests
{
    private static PurchaseService CreateService(
        ApplicationDbContext context,
        ICarritoService? carritoService = null,
        IPdfService? pdfService = null,
        IEmailService? emailService = null)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<PurchaseService>>();
        return new PurchaseService(
            context,
            carritoService ?? Mock.Of<ICarritoService>(),
            pdfService ?? Mock.Of<IPdfService>(),
            emailService ?? Mock.Of<IEmailService>(),
            cache,
            logger);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Test]
    public async Task GetByUserAsync_NoPurchases_ReturnsEmpty()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var result = await service.GetByUserAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetByUserAsync_WithPurchases_ReturnsPurchases()
    {
        await using var context = CreateContext();
        var user = new User { UserName = "test", Email = "test@test.com" };
        context.Users.Add(user);
        context.Purchases.Add(new Purchase { CompradorId = user.Id, Total = 100m, FechaCompra = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetByUserAsync(user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}
