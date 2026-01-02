using Bunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Components.Ratings;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;
using TiendaDawWeb.Services.Implementations;
using TiendaDawWeb.Errors;
using CSharpFunctionalExtensions;
using System.Security.Claims;

namespace TiendaDawWeb.Tests.Components;

/// <summary>
/// Mock simple para el estado de autenticación en Blazor.
/// </summary>
public class MockAuthStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _user;

    public MockAuthStateProvider(ClaimsPrincipal user) => _user = user;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_user));
}

public class RatingSectionTests : BunitTestContext
{
    private Mock<IRatingService> _ratingServiceMock;
    private Mock<ILogger<RatingSection>> _loggerMock;
    private RatingStateContainer _stateContainer;
    private Mock<IUserStore<User>> _userStoreMock;
    private Mock<UserManager<User>> _userManagerMock;

    [SetUp]
    public void Setup()
    {
        _ratingServiceMock = new Mock<IRatingService>();
        _loggerMock = new Mock<ILogger<RatingSection>>();
        _stateContainer = new RatingStateContainer();
        
        _userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(_userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Configurar un usuario autenticado
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "prueba@prueba.com"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "TestAuth"));

        // Registrar servicios
        Services.AddSingleton(_ratingServiceMock.Object);
        Services.AddSingleton(_loggerMock.Object);
        Services.AddSingleton(_stateContainer);
        Services.AddSingleton(_userManagerMock.Object);
        Services.AddSingleton<AuthenticationStateProvider>(new MockAuthStateProvider(user));
    }

    [Test]
    public void Should_Submit_Rating_Successfully()
    {
        // Arrange
        var productId = 1L;
        var userId = 1L;
        var expectedScore = 4;
        var expectedComment = "Muy buen producto";

        bool notificationReceived = false;
        _stateContainer.OnChange += () => notificationReceived = true;

        // Mock de datos iniciales dinámico
        var ratingsList = new List<Rating>();
        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(() => Result.Success<IEnumerable<Rating>, TiendaDawWeb.Errors.DomainError>(ratingsList));

        // Mock del usuario actual
        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Id = userId, Nombre = "Prueba" });

        // Mock del resultado de añadir valoración
        _ratingServiceMock.Setup(s => s.AddRatingAsync(userId, productId, expectedScore, expectedComment))
            .ReturnsAsync((long u, long p, int s, string? c) => {
                var newRating = new Rating { UsuarioId = u, ProductoId = p, Puntuacion = s, Comentario = c, CreatedAt = DateTime.UtcNow };
                ratingsList.Add(newRating); // Añadir a la lista para el siguiente "Get"
                return Result.Success<Rating, DomainError>(newRating);
            });

        // Act
        var cut = RenderComponent<RatingSection>(parameters => parameters
            .Add(p => p.ProductId, productId)
            .Add(p => p.CurrentUserId, userId)
            .Add(p => p.IsOwner, false));

        // 1. Simular clic en la 4ª estrella
        var stars = cut.FindAll(".star-item");
        stars[3].Click();

        // 2. Simular escritura en el textarea
        var textarea = cut.Find("textarea");
        textarea.Change(expectedComment);

        // 3. Simular envío del formulario
        var form = cut.Find("form");
        form.Submit();

        // Assert
        _ratingServiceMock.Verify(s => s.AddRatingAsync(userId, productId, expectedScore, expectedComment), Times.Once);
        Assert.That(notificationReceived, Is.True, "El StateContainer debería haber notificado el cambio.");
        Assert.That(cut.Markup, Does.Contain("Gracias por tu valoración"));
    }

    [Test]
    public void Should_Show_Error_When_No_Stars_Selected()
    {
        // Arrange
        var productId = 1L;
        var userId = 1L;
        
        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, TiendaDawWeb.Errors.DomainError>(new List<Rating>()));

        var cut = RenderComponent<RatingSection>(parameters => parameters
            .Add(p => p.ProductId, productId)
            .Add(p => p.CurrentUserId, userId));

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.That(cut.Markup, Does.Contain("Debes seleccionar una puntuación"));
        _ratingServiceMock.Verify(s => s.AddRatingAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }
}