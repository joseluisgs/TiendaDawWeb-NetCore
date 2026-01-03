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

/// <summary>

/// OBJETIVO: Probar la interactividad del componente de valoraciones (formulario + listado).

/// LO QUE BUSCA: Validar que el componente maneja correctamente la autenticación, los eventos

/// de usuario (clics), la comunicación con servicios y la sincronización mediante StateContainer.

/// </summary>

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



            _stateContainer = new RatingStateContainer(_ratingServiceMock.Object);

        

        // Mock de infraestructura de Identity (necesario porque el componente inyecta UserManager)

        _userStoreMock = new Mock<IUserStore<User>>();

        _userManagerMock = new Mock<UserManager<User>>(_userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);



        // Configuración de un contexto de autenticación simulado para Blazor

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]

        {

            new Claim(ClaimTypes.Name, "prueba@prueba.com"),

            new Claim(ClaimTypes.NameIdentifier, "1")

        }, "TestAuth"));



        // Registro de servicios en el contenedor de bUnit

        Services.AddSingleton(_ratingServiceMock.Object);

        Services.AddSingleton(_loggerMock.Object);

        Services.AddSingleton(_stateContainer);

        Services.AddSingleton(_userManagerMock.Object);

        Services.AddSingleton<AuthenticationStateProvider>(new MockAuthStateProvider(user));

    }



    /// <summary>

    /// PRUEBA: Flujo completo de valoración exitosa.

    /// OBJETIVO: Simular que un usuario logueado selecciona una estrella, escribe un texto y envía.

    /// LO QUE BUSCA: Verificar que se llama al servicio AddRating, que el StateContainer avisa 

    /// a otros componentes y que la UI cambia a la tarjeta de "gracias".

    /// </summary>

    [Test]

    public void Should_Submit_Rating_Successfully()

    {

        // Arrange

        var productId = 1L;

        var userId = 1L;

        var expectedScore = 4;

        var expectedComment = "Muy buen producto";



        // Suscribirse al evento para verificar que se dispara la notificación reactiva

        bool notificationReceived = false;

        _stateContainer.OnChange += () => notificationReceived = true;



        // Mock dinámico: La primera vez está vacío, tras votar devuelve el voto (para simular recarga de datos)

        var ratingsList = new List<Rating>();

        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))

            .ReturnsAsync(() => Result.Success<IEnumerable<Rating>, TiendaDawWeb.Errors.DomainError>(ratingsList));



        _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))

            .ReturnsAsync(new User { Id = userId, Nombre = "Prueba" });



        _ratingServiceMock.Setup(s => s.AddRatingAsync(userId, productId, expectedScore, expectedComment))

            .ReturnsAsync((long u, long p, int s, string? c) => {

                var newRating = new Rating { UsuarioId = u, ProductoId = p, Puntuacion = s, Comentario = c, CreatedAt = DateTime.UtcNow };

                ratingsList.Add(newRating); 

                return Result.Success<Rating, DomainError>(newRating);

            });



        // Act

        var cut = RenderComponent<RatingSection>(parameters => parameters

            .Add(p => p.ProductId, productId)

            .Add(p => p.CurrentUserId, userId)

            .Add(p => p.IsOwner, false));



        // 1. Simulación de interacción con el DOM (Selección de estrellas)

        var stars = cut.FindAll(".star-item");

        stars[3].Click(); // Clic en la cuarta estrella (índice 3)



        // 2. Simulación de entrada de texto

        var textarea = cut.Find("textarea");

        textarea.Change(expectedComment);



        // 3. Simulación de envío de formulario

        var form = cut.Find("form");

        form.Submit();



        // Assert

        // Verificamos el contrato con la lógica de negocio

        _ratingServiceMock.Verify(s => s.AddRatingAsync(userId, productId, expectedScore, expectedComment), Times.Once);

        

        // Verificamos que se rompió el aislamiento y se notificó el cambio de estado global

        Assert.That(notificationReceived, Is.True, "El StateContainer debería haber notificado el cambio.");

        

        // Verificamos la reacción de la UI

        Assert.That(cut.Markup, Does.Contain("Gracias por tu valoración"));

    }



    /// <summary>

    /// PRUEBA: Validación de puntuación obligatoria.

    /// OBJETIVO: Verificar que el componente no permite enviar el formulario si no se ha seleccionado ninguna estrella.

    /// LO QUE BUSCA: Prevenir llamadas innecesarias al servidor y mostrar un error visual al usuario.

    /// </summary>

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



        // Act - Envío directo sin interacción previa con estrellas

        cut.Find("form").Submit();



        // Assert

        Assert.That(cut.Markup, Does.Contain("Debes seleccionar una puntuación"));

        _ratingServiceMock.Verify(s => s.AddRatingAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);

    }

}
