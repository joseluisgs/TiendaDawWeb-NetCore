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
/// OBJETIVO: Validar el API de valoraciones para AJAX.
/// LO QUE BUSCA: Verificar la creación, borrado y consulta de valoraciones
/// a través de los endpoints REST.
/// </summary>
[TestFixture]
public class RatingApiControllerTests
{
    private Mock<IRatingService> _ratingServiceMock;
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<ILogger<RatingApiController>> _loggerMock;
    private RatingApiController _controller;

    [SetUp]
    public void Setup()
    {
        _ratingServiceMock = new Mock<IRatingService>();
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _loggerMock = new Mock<ILogger<RatingApiController>>();
        
        _controller = new RatingApiController(
            _ratingServiceMock.Object,
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
    /// PRUEBA: Añadir valoración vía API.
    /// OBJETIVO: Confirmar que el endpoint POST devuelve OK cuando los datos son válidos.
    /// </summary>
    [Test]
    public async Task AddRating_ShouldReturnOk_WhenValid()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _ratingServiceMock.Setup(rs => rs.AddRatingAsync(user.Id, 100L, 5, "Good"))
            .ReturnsAsync(Result.Success<Rating, DomainError>(new Rating { Id = 1, Puntuacion = 5, Comentario = "Good" }));

        // Act
        var result = await _controller.AddRating(new AddRatingRequest(100L, 5, "Good"));

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// PRUEBA: Obtener valoraciones de un producto.
    /// OBJETIVO: Validar que el endpoint GET devuelve el listado de notas.
    /// </summary>
    [Test]
    public async Task GetProductRatings_ShouldReturnOk()
    {
        // Arrange
        _ratingServiceMock.Setup(rs => rs.GetByProductoIdAsync(100L))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(new List<Rating>()));

        // Act
        var result = await _controller.GetProductRatings(100L);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// PRUEBA: Obtener valoración propia.
    /// OBJETIVO: Confirmar que el endpoint devuelve la nota del usuario actual para un producto.
    /// </summary>
    [Test]
    public async Task GetUserRating_ShouldReturnOk()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _ratingServiceMock.Setup(rs => rs.GetByProductoIdAsync(100L))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(new List<Rating>()));

        // Act
        var result = await _controller.GetUserRating(100L);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}