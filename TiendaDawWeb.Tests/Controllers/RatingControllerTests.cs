using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Rating;
using TiendaDawWeb.ViewModels;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador de valoraciones.
/// LO QUE BUSCA: Asegurar que las operaciones de valoración funcionan correctamente.
/// </summary>
[TestFixture]
public class RatingControllerTests
{
    private Mock<IRatingService> _ratingServiceMock = null!;
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<ILogger<RatingController>> _loggerMock = null!;
    private Mock<ITempDataDictionary> _tempDataMock = null!;
    private RatingController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _ratingServiceMock = new Mock<IRatingService>();

        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<RatingController>>();
        _tempDataMock = new Mock<ITempDataDictionary>();

        _controller = new RatingController(
            _ratingServiceMock.Object,
            _userManagerMock.Object,
            _loggerMock.Object)
        {
            TempData = _tempDataMock.Object
        };

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
    /// PRUEBA: AddRating con datos válidos.
    /// </summary>
    [Test]
    public async Task AddRating_ShouldAddRating_WhenDataIsValid()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var model = new RatingViewModel { ProductoId = 1, Puntuacion = 5, Comentario = "Great!" };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _ratingServiceMock.Setup(s => s.AddRatingAsync(user.Id, 1, 5, "Great!"))
            .ReturnsAsync(Result.Success<Rating, DomainError>(new Rating { Id = 1, UsuarioId = 1, ProductoId = 1 }));

        // Act
        var result = await _controller.AddRating(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Details");
        redirectResult.ControllerName.Should().Be("Product");
        _tempDataMock.VerifySet(t => t["Success"] = "¡Valoración añadida correctamente!");
    }

    /// <summary>
    /// PRUEBA: AddRating con modelo inválido.
    /// </summary>
    [Test]
    public async Task AddRating_ShouldShowError_WhenModelIsInvalid()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var model = new RatingViewModel { ProductoId = 1, Puntuacion = 0 }; // Invalid

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _controller.ModelState.AddModelError("Puntuacion", "Required");

        // Act
        var result = await _controller.AddRating(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = "Datos de valoración inválidos");
    }

    /// <summary>
    /// PRUEBA: AddRating sin usuario redirige a login.
    /// </summary>
    [Test]
    public async Task AddRating_ShouldRedirectToLogin_WhenUserNotFound()
    {
        // Arrange
        var model = new RatingViewModel { ProductoId = 1, Puntuacion = 5 };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.AddRating(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Login");
        redirectResult.ControllerName.Should().Be("Auth");
    }

    /// <summary>
    /// PRUEBA: AddRating con error.
    /// </summary>
    [Test]
    public async Task AddRating_ShouldShowError_WhenServiceFails()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var model = new RatingViewModel { ProductoId = 1, Puntuacion = 5 };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _ratingServiceMock.Setup(s => s.AddRatingAsync(user.Id, 1, 5, null))
            .ReturnsAsync(Result.Failure<Rating, DomainError>(RatingError.AlreadyRated()));

        // Act
        var result = await _controller.AddRating(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = "Ya has valorado este producto");
    }

    /// <summary>
    /// PRUEBA: Details muestra la valoración.
    /// </summary>
    [Test]
    public async Task Details_ShouldReturnView_WhenRatingExists()
    {
        // Arrange
        var rating = new Rating { Id = 1, ProductoId = 1, UsuarioId = 1, Puntuacion = 5 };
        _ratingServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Rating, DomainError>(rating));

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<Rating>();
    }

    /// <summary>
    /// PRUEBA: Details con valoración no encontrada.
    /// </summary>
    [Test]
    public async Task Details_ShouldRedirectWithError_WhenNotFound()
    {
        // Arrange
        _ratingServiceMock.Setup(s => s.GetByIdAsync(99))
            .ReturnsAsync(Result.Failure<Rating, DomainError>(RatingError.NotFound(99)));

        // Act
        var result = await _controller.Details(99);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = "Recurso con ID 99 no encontrado");
    }

    /// <summary>
    /// PRUEBA: Edit GET muestra formulario.
    /// </summary>
    [Test]
    public async Task Edit_Get_ShouldReturnView_WhenUserIsOwner()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var rating = new Rating { Id = 1, ProductoId = 1, UsuarioId = 1, Puntuacion = 4, Comentario = "Good" };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _ratingServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Rating, DomainError>(rating));

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<RatingViewModel>();
    }

    /// <summary>
    /// PRUEBA: Edit GET deniega acceso si no es propietario.
    /// </summary>
    [Test]
    public async Task Edit_Get_ShouldRedirectWithError_WhenNotOwner()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        var rating = new Rating { Id = 1, ProductoId = 1, UsuarioId = 2, Puntuacion = 4 }; // Different owner

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _ratingServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<Rating, DomainError>(rating));

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = "No tienes permiso para editar esta valoración");
    }

    /// <summary>
    /// PRUEBA: Delete elimina valoración.
    /// </summary>
    [Test]
    public async Task Delete_ShouldDeleteRating_WhenUserIsOwner()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _ratingServiceMock.Setup(s => s.DeleteRatingAsync(1, user.Id, false))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.Delete(1, productoId: 1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Success"] = "Valoración eliminada correctamente");
    }

    /// <summary>
    /// PRUEBA: Delete con error.
    /// </summary>
    [Test]
    public async Task Delete_ShouldShowError_WhenServiceFails()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _ratingServiceMock.Setup(s => s.DeleteRatingAsync(1, user.Id, false))
            .ReturnsAsync(Result.Failure<bool, DomainError>(RatingError.NotFound(1)));

        // Act
        var result = await _controller.Delete(1, productoId: 1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _tempDataMock.VerifySet(t => t["Error"] = "Recurso con ID 1 no encontrado");
    }

    /// <summary>
    /// PRUEBA: Delete como admin puede eliminar cualquier valoración.
    /// </summary>
    [Test]
    public async Task Delete_ShouldDeleteAsAdmin_WhenUserIsAdmin()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "admin" };
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = context };

        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _ratingServiceMock.Setup(s => s.DeleteRatingAsync(1, user.Id, true))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.Delete(1, productoId: 1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        _ratingServiceMock.Verify(s => s.DeleteRatingAsync(1, user.Id, true), Times.Once);
    }
}
