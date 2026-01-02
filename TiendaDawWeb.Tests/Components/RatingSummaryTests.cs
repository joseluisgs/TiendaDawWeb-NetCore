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
/// Ejemplo de test unitario para un componente Blazor usando bUnit.
/// Este test muestra cómo mockear servicios y verificar el renderizado de la UI.
/// </summary>
public class RatingSummaryTests : BunitTestContext
{
    private Mock<IRatingService> _ratingServiceMock;
    private Mock<ILogger<RatingSummary>> _loggerMock;
    private RatingStateContainer _stateContainer;

    [SetUp]
    public void Setup()
    {
        _ratingServiceMock = new Mock<IRatingService>();
        _loggerMock = new Mock<ILogger<RatingSummary>>();
        _stateContainer = new RatingStateContainer();

        // Registrar servicios necesarios en el contenedor de bUnit
        Services.AddSingleton(_ratingServiceMock.Object);
        Services.AddSingleton(_loggerMock.Object);
        Services.AddSingleton(_stateContainer);
    }

    [Test]
    public void Should_Render_Average_Stars_Correctly()
    {
        // Arrange
        var productId = 1L;
        var ratings = new List<Rating>
        {
            new() { Puntuacion = 5 },
            new() { Puntuacion = 4 }
        };

        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, TiendaDawWeb.Errors.DomainError>(ratings));

        // Act - Renderizar el componente
        var cut = RenderComponent<RatingSummary>(parameters => parameters
            .Add(p => p.ProductId, productId));

        // Assert
        // Verificamos que se muestra la media (4.5 o 4,5 según cultura)
        var text = cut.Markup;
        Assert.That(text, Does.Match("4[.,]5"));
        Assert.That(text, Does.Contain("(2 valoraciones)"));
        
        // Verificamos que hay estrellas llenas (FontAwesome classes)
        var filledStars = cut.FindAll("i.bi-star-fill");
        Assert.That(filledStars.Count, Is.EqualTo(4)); // 4 estrellas llenas para un 4.5
        
        var halfStar = cut.FindAll("i.bi-star-half");
        Assert.That(halfStar.Count, Is.EqualTo(1)); // 1 estrella media para el .5
    }

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
