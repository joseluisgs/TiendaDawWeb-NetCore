using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;
using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del API de favoritos.
/// LO QUE BUSCA: Asegurar que los endpoints responden correctamente a las peticiones AJAX,
/// gestionan la autenticación y devuelven los códigos de estado HTTP apropiados.
/// </summary>
[TestFixture]
public class FavoriteApiControllerTests
{
    private Mock<IFavoriteService> _favoriteServiceMock;
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<ILogger<FavoriteApiController>> _loggerMock;
    private FavoriteApiController _controller;

    [SetUp]
    public void Setup()
    {
        _favoriteServiceMock = new Mock<IFavoriteService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _loggerMock = new Mock<ILogger<FavoriteApiController>>();
        
        _controller = new FavoriteApiController(
            _favoriteServiceMock.Object,
            _userManagerMock.Object,
            _loggerMock.Object
        );
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));
        
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    /// <summary>
    /// PRUEBA: Añadir a favoritos con éxito.
    /// OBJETIVO: Verificar que el endpoint devuelve OK(200) cuando el servicio procesa la solicitud correctamente.
    /// </summary>
    [Test]
    public async Task AddFavorite_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _favoriteServiceMock.Setup(fs => fs.AddFavoriteAsync(user.Id, 100L))
            .ReturnsAsync(Result.Success<Favorite, DomainError>(new Favorite()));

        // Act
        var result = await _controller.AddFavorite(new AddFavoriteRequest(100L));

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// PRUEBA: Eliminar de favoritos con éxito.
    /// OBJETIVO: Confirmar que el borrado de favoritos vía API funciona y devuelve éxito.
    /// </summary>
    [Test]
    public async Task RemoveFavorite_ShouldReturnOk_WhenSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _favoriteServiceMock.Setup(fs => fs.RemoveFavoriteAsync(user.Id, 100L))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.RemoveFavorite(100L);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// PRUEBA: Comprobar estado de favorito.
    /// OBJETIVO: Validar que el endpoint devuelve si un producto es favorito del usuario actual.
    /// </summary>
    [Test]
    public async Task CheckFavorite_ShouldReturnIsFavorite()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _favoriteServiceMock.Setup(fs => fs.IsFavoriteAsync(user.Id, 100L))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.CheckFavorite(100L);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        // Using dynamic check or anonymous type match
        okResult.Value.Should().NotBeNull();
    }

    /// <summary>
    /// PRUEBA: Conmutar (Toggle) favorito (Añadir).
    /// OBJETIVO: Verificar que Toggle añade a favoritos si el producto no lo era antes.
    /// </summary>
    [Test]
    public async Task ToggleFavorite_ShouldAdd_WhenNotFavorite()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _favoriteServiceMock.Setup(fs => fs.IsFavoriteAsync(user.Id, 100L))
            .ReturnsAsync(Result.Success<bool, DomainError>(false));
        _favoriteServiceMock.Setup(fs => fs.AddFavoriteAsync(user.Id, 100L))
            .ReturnsAsync(Result.Success<Favorite, DomainError>(new Favorite()));

        // Act
        var result = await _controller.ToggleFavorite(new AddFavoriteRequest(100L));

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }
}