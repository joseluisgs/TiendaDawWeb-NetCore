using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Components.Ratings;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;
using TiendaDawWeb.Services.Implementations;
using CSharpFunctionalExtensions;

namespace TiendaDawWeb.Tests.Components;

/// <summary>
/// OBJETIVO: Verificar el renderizado dinámico del resumen de estrellas en la cabecera.
/// LO QUE BUSCA: Validar que el componente Blazor RatingSummary transforma correctamente 
/// una lista de valoraciones en una representación visual de estrellas y promedios.
/// </summary>
public class RatingSummaryTests : BunitTestContext
{
    private Mock<IRatingService> _ratingServiceMock;
    private Mock<ILogger<RatingSummary>> _loggerMock;
    private RatingStateContainer _stateContainer;

    [SetUp]
    public void Setup()
    {
        // Inicialización de dependencias mockeadas
        _ratingServiceMock = new Mock<IRatingService>();
        _loggerMock = new Mock<ILogger<RatingSummary>>();
        _stateContainer = new RatingStateContainer();

        // Inyección de servicios en el contexto de bUnit
        Services.AddSingleton(_ratingServiceMock.Object);
        Services.AddSingleton(_loggerMock.Object);
        Services.AddSingleton(_stateContainer);
    }

    /// <summary>
    /// PRUEBA: Renderizado de media con estrellas.
    /// OBJETIVO: Verificar que si un producto tiene valoraciones (ej. 5 y 4), 
    /// el componente muestra "4.5" y el número correcto de iconos de estrellas.
    /// </summary>
    [Test]
    public void Should_Render_Average_Stars_Correctly()
    {
        // Arrange (Preparación)
        var productId = 1L;
        var ratings = new List<Rating>
        {
            new() { Puntuacion = 5 },
            new() { Puntuacion = 4 }
        };

        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, TiendaDawWeb.Errors.DomainError>(ratings));

        // Act (Acción: Renderizado del componente)
        var cut = RenderComponent<RatingSummary>(parameters => parameters
            .Add(p => p.ProductId, productId));

        // Assert (Verificación)
        var text = cut.Markup;
        Assert.That(text, Does.Match("4[.,]5")); // Comprueba que aparece la media (formato internacional)
        Assert.That(text, Does.Contain("(2 valoraciones)"));
        
        // Verificamos la lógica visual: 4 estrellas llenas + 1 media estrella = 4.5
        var filledStars = cut.FindAll("i.bi-star-fill");
        Assert.That(filledStars.Count, Is.EqualTo(4));
        
        var halfStar = cut.FindAll("i.bi-star-half");
        Assert.That(halfStar.Count, Is.EqualTo(1));
    }

    /// <summary>
    /// PRUEBA: Comportamiento sin valoraciones.
    /// OBJETIVO: Asegurar que si el servicio devuelve una lista vacía, 
    /// el componente muestra un mensaje informativo amigable en lugar de errores o 0.
    /// </summary>
    [Test]
    public void Should_Show_No_Ratings_Message_When_Empty()
    {
        // Arrange
        var productId = 1L;
        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, TiendaDawWeb.Errors.DomainError>(new List<Rating>()));

        // Act
        var cut = RenderComponent<RatingSummary>(parameters => parameters
            .Add(p => p.ProductId, productId));

        // Assert
        Assert.That(cut.Markup, Does.Contain("Sin valoraciones aún"));
    }
}
