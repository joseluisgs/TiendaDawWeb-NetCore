using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Web.Hubs;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
///     Tests unitarios para ProductService con EF Core InMemory.
/// </summary>
public class ProductServiceTests {
    private static ProductService CreateService(ApplicationDbContext context) {
        var cache = new MemoryCache(new MemoryCacheOptions());
        ILogger<ProductService> logger = Mock.Of<ILogger<ProductService>>();
        IHubContext<NotificationHub> hubContext = Mock.Of<IHubContext<NotificationHub>>();
        return new ProductService(context, cache, hubContext, logger);
    }

    private static ApplicationDbContext CreateContext() {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Test]
    public async Task GetByIdAsync_NonExistentProduct_ReturnsFailure() {
        await using ApplicationDbContext context = CreateContext();
        ProductService service = CreateService(context);
        Result<Product, DomainError> result = await service.GetByIdAsync(999);

        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProduct() {
        await using ApplicationDbContext context = CreateContext();
        var user = new User { UserName = "owner", Email = "owner@test.com" };
        context.Users.Add(user);
        var product = new Product {
            Nombre = "Laptop",
            Precio = 999.99m,
            Descripcion = "Test",
            Deleted = false,
            PropietarioId = user.Id
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        ProductService service = CreateService(context);
        Result<Product, DomainError> result = await service.GetByIdAsync(product.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("Laptop");
    }
}
