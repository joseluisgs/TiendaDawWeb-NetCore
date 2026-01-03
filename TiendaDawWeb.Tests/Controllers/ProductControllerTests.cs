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

using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TiendaDawWeb.Tests.Controllers;

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
            new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<User>>().Object,
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<User>>>().Object);

        _hubContextMock = new Mock<IHubContext<NotificationHub>>();
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
            new Claim(ClaimTypes.Name, "test")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = context };

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        model.Should().HaveCount(1);
    }

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
}
