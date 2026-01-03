using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Implementations;
using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using System.Collections.Generic;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// OBJETIVO: Test unitarios/integración para ProductService, cubriendo operaciones CRUD,
/// cache y soft-delete sobre productos. Valida la compatibilidad con EF Core InMemory y caché real.
/// </summary>
[TestFixture]
public class ProductServiceTests
{
    // Campos necesarios para la infraestructura de los tests
    private ApplicationDbContext _context = null!;
    private IMemoryCache _memoryCache = null!;
    private Mock<ILogger<ProductService>> _loggerMock = null!;
    private ProductService _productService = null!;

    /// <summary>
    /// Inicialización de cada test. Aquí se crea una BD en memoria distinta para cada ejecución,
    /// se añade el usuario propietario y los productos referenciados, y luego se inicializan la caché y el servicio.
    /// Es fundamental guardar los cambios antes de crear el servicio para asegurar que la caché lea el estado correcto.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // 1. Crea el contexto de BD en memoria para una suite aislada.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // 2. Crea y persiste un usuario propietario (necesario para todas las relaciones y navegaciones).
        var propietario = new User
        {
            Id = 1,
            Nombre = "Propietario",
            Apellidos = "Propietario Test",
            Email = "propietario@test.com",
            UserName = "propietario@test.com",
            Rol = "USER",
            EmailConfirmed = true,
            Avatar = "test.png"
        };
        _context.Users.Add(propietario);
        _context.SaveChanges();

        // 3. Crea dos productos de prueba referenciando al propietario por Id y navegación.
        _context.Products.AddRange(
            new Product
            {
                Id = 1,
                Nombre = "Producto Test 1",
                Descripcion = "Desc 1",
                Precio = 10.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = propietario.Id,
                Propietario = propietario, // navegación EF
                CreatedAt = DateTime.UtcNow,
                Deleted = false,
                Imagen = "img1.png",
                CompraId = null,
                Ratings = new List<Rating>()
            },
            new Product
            {
                Id = 2,
                Nombre = "Producto Test 2",
                Descripcion = "Desc 2",
                Precio = 20.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = propietario.Id,
                Propietario = propietario,
                CreatedAt = DateTime.UtcNow,
                Deleted = false,
                Imagen = "img2.png",
                CompraId = null,
                Ratings = new List<Rating>()
            }
        );
        _context.SaveChanges();

        // 4. Inicializa la caché y el servicio SÓLO después de guardar los datos.
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_context, _memoryCache, _loggerMock.Object);
    }

    /// <summary>
    /// Limpieza tras cada test para evitar contaminación de datos entre pruebas.
    /// Elimina la BD y libera recursos de caché y contexto.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _memoryCache.Dispose();
    }

    /// <summary>
    /// PRUEBA: Consulta de producto existente.
    /// OBJETIVO: Validar que el servicio recupera correctamente un producto por su Id,
    /// incluido el almacenamiento en caché.
    /// </summary>
    [Test]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        var productId = 1L;
        var result = await _productService.GetByIdAsync(productId);

        // El producto debe ser encontrado (IsSuccess), no nulo y con los datos esperados.
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(productId);
        result.Value!.Nombre.Should().Be("Producto Test 1");

        // Comprobamos que el resultado se haya cacheado en memoria.
        object? cachedProduct;
        _memoryCache.TryGetValue($"product_details_{productId}", out cachedProduct).Should().BeTrue();
    }

    /// <summary>
    /// PRUEBA: Consulta de producto inexistente.
    /// OBJETIVO: Validar el manejo de errores al buscar un Id fuera del catálogo.
    /// </summary>
    [Test]
    public async Task GetByIdAsync_ShouldReturnError_WhenNotExists()
    {
        var productId = 99L;
        var result = await _productService.GetByIdAsync(productId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductError.NotFound(productId));
    }
    
    /// <summary>
    /// PRUEBA: Recuperación de todos los productos.
    /// OBJETIVO: Confirmar que el servicio devuelve ambos productos persistidos y el resultado se cachea.
    /// </summary>
    [Test]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        var result = await _productService.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        object? cachedProducts;
        _memoryCache.TryGetValue("all_products", out cachedProducts).Should().BeTrue();
    }
    
    /// <summary>
    /// PRUEBA: Creación de nuevo producto e invalidación de caché.
    /// OBJETIVO: Verificar que al crear un producto, este aparece en la BD y la caché previa se invalida.
    /// </summary>
    [Test]
    public async Task CreateAsync_ShouldAddProductAndInvalidateCache()
    {
        var propietario = await _context.Users.FirstAsync(u => u.Id == 1);
        var newProduct = new Product
        {
            Id = 3,
            Nombre = "Nuevo Producto",
            Descripcion = "Desc 3",
            Precio = 30.0m,
            Categoria = ProductCategory.GAMING,
            PropietarioId = propietario.Id,
            Propietario = propietario,
            Deleted = false,
            CreatedAt = DateTime.UtcNow,
            Imagen = "img3.png",
            CompraId = null,
            Ratings = new List<Rating>()
        };
        
        await _productService.GetAllAsync(); // Inicializa caché

        var result = await _productService.CreateAsync(newProduct);

        // Comprobamos éxito y existencia en BD
        result.IsSuccess.Should().BeTrue();
        var productInDb = await _context.Products.FirstOrDefaultAsync(p => p.Id == 3);
        productInDb.Should().NotBeNull();
        productInDb!.Nombre.Should().Be("Nuevo Producto");

        // La caché general debe invalidarse tras la creación.
        object? cachedProducts;
        _memoryCache.TryGetValue("all_products", out cachedProducts).Should().BeFalse();
    }

    /// <summary>
    /// PRUEBA: Actualización de un producto e invalidación de caché.
    /// OBJETIVO: Validar que los cambios en producto se guardan y la caché se actualiza.
    /// </summary>
    [Test]
    public async Task UpdateAsync_ShouldUpdateProductAndInvalidateCache()
    {
        var propietario = await _context.Users.FirstAsync(u => u.Id == 1);
        var productId = 1L;
        var updatedProduct = new Product
        {
            Id = productId,
            Nombre = "Producto Actualizado",
            Descripcion = "Nueva Desc",
            Precio = 15.0m,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = propietario.Id,
            Propietario = propietario,
            Deleted = false,
            CreatedAt = DateTime.UtcNow,
            Imagen = "img1.png",
            CompraId = null,
            Ratings = new List<Rating>()
        };

        await _productService.GetByIdAsync(productId);
        await _productService.GetAllAsync();

        var result = await _productService.UpdateAsync(productId, updatedProduct, propietario.Id);

        result.IsSuccess.Should().BeTrue(); // No accedemos a .Error si Success
        var dbProduct = await _context.Products.FindAsync(productId);
        dbProduct.Should().NotBeNull();
        dbProduct!.Nombre.Should().Be("Producto Actualizado");

        // Verificamos que tanto el cache individual como el general hayan sido invalidados.
        object? cachedItem;
        _memoryCache.TryGetValue($"product_details_{productId}", out cachedItem).Should().BeFalse();
        _memoryCache.TryGetValue("all_products", out cachedItem).Should().BeFalse();
    }
    
    /// <summary>
    /// PRUEBA: Borrado lógico (soft-delete) del producto e invalidación de caché.
    /// OBJETIVO: Validar que tras borrar un producto, este aparece como Deleted en BD y la caché se borra.
    /// </summary>
    [Test]
    public async Task DeleteAsync_ShouldSoftDeleteProductAndInvalidateCache()
    {
        var propietario = await _context.Users.FirstAsync(u => u.Id == 1);
        var productId = 1L;
        await _productService.GetByIdAsync(productId); 
        await _productService.GetAllAsync(); 

        var result = await _productService.DeleteAsync(productId, propietario.Id);

        result.IsSuccess.Should().BeTrue();
        var deletedProduct = await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == productId);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.Deleted.Should().BeTrue();

        // Verificamos invalidación de caché individual y general.
        object? cachedItem;
        _memoryCache.TryGetValue($"product_details_{productId}", out cachedItem).Should().BeFalse();
        _memoryCache.TryGetValue("all_products", out cachedItem).Should().BeFalse();
    }

    [Test]
    public async Task SearchAsync_ShouldFilterByNombre()
    {
        // Act
        var result = await _productService.SearchAsync("Test 1", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Nombre.Should().Contain("Test 1");
    }

    [Test]
    public async Task SearchAsync_ShouldFilterByCategoria()
    {
        // Act
        var result = await _productService.SearchAsync(null, ProductCategory.LAPTOPS.ToString());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Categoria.Should().Be(ProductCategory.LAPTOPS);
    }

    [Test]
    public async Task CreateAsync_ShouldFail_WhenPriceIsInvalid()
    {
        // Arrange
        var product = new Product { Nombre = "Invalid", Precio = 0, Categoria = ProductCategory.AUDIO };

        // Act
        var result = await _productService.CreateAsync(product);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductError.InvalidPrice);
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenNotOwner()
    {
        // Arrange
        var productId = 1L;
        var updatedProduct = new Product { Nombre = "Hack" };

        // Act
        var result = await _productService.UpdateAsync(productId, updatedProduct, userId: 999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductError.NotOwner);
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenProductIsSold()
    {
        // Arrange
        var productId = 1L;
        var product = await _context.Products.FindAsync(productId);
        product!.CompraId = 55L; // Mark as sold
        await _context.SaveChangesAsync();

        // Act
        var result = await _productService.DeleteAsync(productId, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductError.CannotDeleteSold);
    }

    [Test]
    public async Task DeleteAsync_ShouldSucceed_WhenIsAdminAndNotOwner()
    {
        // Act
        var result = await _productService.DeleteAsync(1, 999, isAdmin: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}