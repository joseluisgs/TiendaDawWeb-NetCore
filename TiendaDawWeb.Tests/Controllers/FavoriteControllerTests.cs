using CSharpFunctionalExtensions;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Favorite;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador de favoritos.
/// LO QUE BUSCA: Asegurar que las operaciones de favoritos funcionan correctamente.
/// </summary>
[TestFixture]
public class FavoriteControllerTests
{
    private Mock<IFavoriteService> _favoriteServiceMock = null!;
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<ILogger<FavoriteController>> _loggerMock = null!;
    private FavoriteController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _favoriteServiceMock = new Mock<IFavoriteService>();

        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<FavoriteController>>();

        _controller = new FavoriteController(
            _favoriteServiceMock.Object,
            _userManagerMock.Object,
            _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = context };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: Index con favoritos retorna vista.
    /// </summary>
    [Test]
    public async Task Index_ShouldReturnViewWithFavorites_WhenUserHasFavorites()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var favorites = new List<Product>
        {
            new() { Id = 1, Nombre = "Producto 1" },
            new() { Id = 2, Nombre = "Producto 2" }
        };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _favoriteServiceMock.Setup(s => s.GetUserFavoritesAsync(user.Id))
            .ReturnsAsync(Result.Success<IEnumerable<Product>, DomainError>(favorites));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeAssignableTo<IEnumerable<Product>>();
        ((IEnumerable<Product>)viewResult.Model!).Should().HaveCount(2);
    }

    /// <summary>
    /// PRUEBA: Index sin usuario redirige a login.
    /// </summary>
    [Test]
    public async Task Index_ShouldRedirectToLogin_WhenUserNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Login");
        redirectResult.ControllerName.Should().Be("Auth");
    }

    /// <summary>
    /// PRUEBA: Add añade producto a favoritos.
    /// </summary>
    [Test]
    public async Task Add_ShouldReturnSuccess_WhenAddingFavorite()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _favoriteServiceMock.Setup(s => s.AddFavoriteAsync(user.Id, 1))
            .ReturnsAsync(Result.Success<Favorite, DomainError>(new Favorite { UsuarioId = 1, ProductoId = 1 }));

        // Act
        var result = await _controller.Add(1);

        // Assert
        var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        dict["success"].GetBoolean().Should().BeTrue();
        dict["message"].GetString().Should().Be("Añadido a favoritos");
    }

    /// <summary>
    /// PRUEBA: Add sin usuario retorna error.
    /// </summary>
    [Test]
    public async Task Add_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Add(1);

        // Assert
        var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        dict["success"].GetBoolean().Should().BeFalse();
        dict["message"].GetString().Should().Be("No autorizado");
    }

    /// <summary>
    /// PRUEBA: Add con error de duplicado retorna mensaje.
    /// </summary>
    [Test]
    public async Task Add_ShouldReturnError_WhenAlreadyFavorite()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _favoriteServiceMock.Setup(s => s.AddFavoriteAsync(user.Id, 1))
            .ReturnsAsync(Result.Failure<Favorite, DomainError>(FavoriteError.AlreadyExists()));

        // Act
        var result = await _controller.Add(1);

        // Assert
        var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        dict["success"].GetBoolean().Should().BeFalse();
        dict["message"].GetString().Should().Contain("ya está en tus favoritos");
    }

    /// <summary>
    /// PRUEBA: Remove elimina producto de favoritos.
    /// </summary>
    [Test]
    public async Task Remove_ShouldReturnSuccess_WhenRemovingFavorite()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _favoriteServiceMock.Setup(s => s.RemoveFavoriteAsync(user.Id, 1))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.Remove(1);

        // Assert
        var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        dict["success"].GetBoolean().Should().BeTrue();
        dict["message"].GetString().Should().Be("Eliminado de favoritos");
    }

    /// <summary>
    /// PRUEBA: Remove sin usuario retorna error.
    /// </summary>
    [Test]
    public async Task Remove_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Remove(1);

        // Assert
        var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        dict["success"].GetBoolean().Should().BeFalse();
        dict["message"].GetString().Should().Be("No autorizado");
    }

    /// <summary>
    /// PRUEBA: Remove con error de no encontrado retorna mensaje.
    /// </summary>
    [Test]
    public async Task Remove_ShouldReturnError_WhenNotFavorite()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _favoriteServiceMock.Setup(s => s.RemoveFavoriteAsync(user.Id, 1))
            .ReturnsAsync(Result.Failure<bool, DomainError>(FavoriteError.NotFound()));

        // Act
        var result = await _controller.Remove(1);

        // Assert
        var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        dict["success"].GetBoolean().Should().BeFalse();
        dict["message"].GetString().Should().Contain("no está en tus favoritos");
    }
}
