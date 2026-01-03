using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;
using TiendaDawWeb.Web.Hubs;
using CSharpFunctionalExtensions;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TiendaDawWeb.Errors;
using TiendaDawWeb.ViewModels;
using TiendaDawWeb.Models.Enums;

using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador de productos.
/// LO QUE BUSCA: Asegurar que las acciones de visualización, creación, edición y borrado
/// funcionan correctamente y gestionan adecuadamente los servicios y la seguridad.
/// </summary>
[TestFixture]
public class ProductControllerTests
{
    private Mock<IProductService> _productServiceMock;
    private Mock<IStorageService> _storageServiceMock;
    private Mock<IFavoriteService> _favoriteServiceMock;
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<IHubContext<NotificationHub>> _hubContextMock;
    private Mock<ILogger<ProductController>> _loggerMock;
    private Mock<ITempDataDictionary> _tempDataMock;
    private ProductController _controller;

    [SetUp]
    public void Setup()
    {
        _productServiceMock = new Mock<IProductService>();
        _storageServiceMock = new Mock<IStorageService>();
        _favoriteServiceMock = new Mock<IFavoriteService>();
        
        // Mocking UserManager
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, 
            null!, null!, null!, null!, null!, null!, null!, null!);

        _hubContextMock = new Mock<IHubContext<NotificationHub>>();
        
        // Mock IHubClients and IClientProxy for SignalR
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
        _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        _loggerMock = new Mock<ILogger<ProductController>>();
        _tempDataMock = new Mock<ITempDataDictionary>();

        _controller = new ProductController(
            _productServiceMock.Object,
            _storageServiceMock.Object,
            _favoriteServiceMock.Object,
            _userManagerMock.Object,
            _hubContextMock.Object,
            _loggerMock.Object
        )
        {
            TempData = _tempDataMock.Object
        };
        
        // Setup default ControllerContext with User
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated by default
        _controller.ControllerContext = new ControllerContext { HttpContext = context };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: Listado de productos para usuario autenticado.
    /// OBJETIVO: Verificar que el Index carga los productos y marca los favoritos.
    /// </summary>
    [Test]
    public async Task Index_ShouldReturnViewWithProducts_WhenServiceReturnsSuccess()
    {
        // Arrange
        var products = new List<Product> { new Product { Id = 1, Nombre = "Test" } };
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Success<IEnumerable<Product>, DomainError>(products));

        // Mock User Identity for Favorites check (Authenticated user)
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        
        _favoriteServiceMock.Setup(s => s.GetUserFavoritesAsync(user.Id))
            .ReturnsAsync(Result.Success<IEnumerable<Product>, DomainError>(new List<Product>()));

        // Simulate Authenticated Context
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = context };

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        model.Should().HaveCount(1);
    }

    /// <summary>
    /// PRUEBA: Vista de detalles de producto.
    /// OBJETIVO: Validar que se muestra la información correcta del producto.
    /// </summary>
    [Test]
    public async Task Details_ShouldReturnView_WhenProductExists()
    {
        // Arrange
        var product = new Product { Id = 1, Nombre = "Test" };
        _productServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Product, DomainError>(product));

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<Product>().Subject;
        model.Id.Should().Be(1);
    }

    /// <summary>
    /// PRUEBA: Detalle de producto inexistente.
    /// OBJETIVO: Verificar que redirige a la página pública si el ID no existe.
    /// </summary>
    [Test]
    public async Task Details_ShouldRedirectToPublicIndex_WhenProductNotFound()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetByIdAsync(99))
            .ReturnsAsync(Result.Failure<Product, DomainError>(ProductError.NotFound(99)));

        // Act
        var result = await _controller.Details(99);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ControllerName.Should().Be("Public");
        redirectResult.ActionName.Should().Be("Index");
    }

    /// <summary>
    /// PRUEBA: Creación exitosa de producto.
    /// OBJETIVO: Validar el flujo completo de creación, incluyendo guardado de imagen y notificación SignalR.
    /// </summary>
    [Test]
    public async Task Create_Post_ShouldRedirect_WhenModelIsValid()
    {
        // Arrange
        var model = new ProductViewModel { Nombre = "Nuevo", Precio = 100, Categoria = ProductCategory.SMARTPHONES };
        var user = new User { Id = 1, UserName = "test" };
        
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _productServiceMock.Setup(s => s.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(Result.Success<Product, DomainError>(new Product { Id = 10, Nombre = "Nuevo" }));

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Details");
        redirectResult.RouteValues!["id"].Should().Be(10L);
    }

    /// <summary>
    /// PRUEBA: Borrado de producto.
    /// OBJETIVO: Confirmar que el controlador llama al servicio de borrado y gestiona el resultado.
    /// </summary>
    [Test]
    public async Task Delete_ShouldRedirectToIndex()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _productServiceMock.Setup(s => s.DeleteAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    /// <summary>
    /// PRUEBA: Error en listado de productos.
    /// OBJETIVO: Verificar que si el servicio falla, se muestra un mensaje de error y un modelo vacío.
    /// </summary>
    [Test]
    public async Task Index_ShouldReturnViewWithEmptyModel_WhenServiceReturnsFailure()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Failure<IEnumerable<Product>, DomainError>(ProductError.InvalidData("Error")));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        model.Should().BeEmpty();
        _tempDataMock.VerifySet(t => t["Error"] = "Error al cargar los productos");
    }

    /// <summary>
    /// PRUEBA: Formulario de creación.
    /// OBJETIVO: Confirmar que la acción GET de Create devuelve la vista correspondiente.
    /// </summary>
    [Test]
    public void Create_Get_ShouldReturnView()
    {
        // Act
        var result = _controller.Create();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    /// <summary>
    /// PRUEBA: Formulario de edición para el propietario.
    /// OBJETIVO: Verificar que el propietario puede acceder a la edición de su producto.
    /// </summary>
    [Test]
    public async Task Edit_Get_ShouldReturnView_WhenUserIsOwner()
    {
        // Arrange
        var productId = 1L;
        var user = new User { Id = 1, UserName = "owner" };
        var product = new Product { Id = productId, Nombre = "Test", PropietarioId = 1 };

        _productServiceMock.Setup(s => s.GetByIdAsync(productId))
            .ReturnsAsync(Result.Success<Product, DomainError>(product));
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Edit(productId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<ProductViewModel>();
    }

    /// <summary>
    /// PRUEBA: Protección de edición para no propietarios.
    /// OBJETIVO: Asegurar que un usuario no puede acceder al formulario de edición de un producto ajeno.
    /// </summary>
    [Test]
    public async Task Edit_Get_ShouldRedirect_WhenUserIsNotOwner()
    {
        // Arrange
        var productId = 1L;
        var user = new User { Id = 2, UserName = "hacker" };
        var product = new Product { Id = productId, Nombre = "Test", PropietarioId = 1 };

        _productServiceMock.Setup(s => s.GetByIdAsync(productId))
            .ReturnsAsync(Result.Success<Product, DomainError>(product));
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Edit(productId);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Error"] = "No tienes permiso para editar este producto");
    }

    /// <summary>
    /// PRUEBA: Procesar edición exitosa.
    /// OBJETIVO: Confirmar que tras una edición válida se actualiza el producto y se redirige al detalle.
    /// </summary>
    [Test]
    public async Task Edit_Post_ShouldRedirect_WhenUpdateSucceeds()
    {
        // Arrange
        var productId = 1L;
        var model = new ProductViewModel { Nombre = "Editado", Precio = 50 };
        var user = new User { Id = 1, UserName = "owner" };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _productServiceMock.Setup(s => s.UpdateAsync(productId, It.IsAny<Product>(), user.Id))
            .ReturnsAsync(Result.Success<Product, DomainError>(new Product { Id = productId }));

        // Act
        var result = await _controller.Edit(productId, model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Details");
        redirectResult.RouteValues!["id"].Should().Be(productId);
    }

    /// <summary>
    /// PRUEBA: Listado de "Mis Productos".
    /// OBJETIVO: Validar que el usuario puede ver la lista de sus propios productos publicados.
    /// </summary>
    [Test]
    public async Task MyProducts_ShouldReturnViewWithUserProducts()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var products = new List<Product> 
        { 
            new Product { Id = 1, PropietarioId = 1, Nombre = "Mío" },
            new Product { Id = 2, PropietarioId = 2, Nombre = "Otro" }
        };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _productServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result.Success<IEnumerable<Product>, DomainError>(products));

        // Act
        var result = await _controller.MyProducts();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        model.Should().HaveCount(1);
        model.First().Nombre.Should().Be("Mío");
    }
}
