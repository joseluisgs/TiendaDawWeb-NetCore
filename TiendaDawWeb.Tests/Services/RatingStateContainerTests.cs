using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Implementations;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// OBJETIVO: Validar la gestión de estado y bus de eventos de valoraciones.
/// LO QUE BUSCA: Confirmar que los datos se cachean correctamente en memoria
/// y que los componentes reciben notificaciones de cambio.
/// </summary>
[TestFixture]
public class RatingStateContainerTests
{
    private Mock<IRatingService> _ratingServiceMock;
    private RatingStateContainer _container;

    [SetUp]
    public void Setup()
    {
        _ratingServiceMock = new Mock<IRatingService>();
        _container = new RatingStateContainer(_ratingServiceMock.Object);
    }

    /// <summary>
    /// PRUEBA: Carga inicial de valoraciones.
    /// OBJETIVO: Verificar que los datos se cargan desde el servicio cuando el producto cambia.
    /// </summary>
    [Test]
    public async Task EnsureLoadedAsync_ShouldLoadRatings_WhenProductIdChanges()
    {
        // Arrange
        var productId = 1L;
        var ratings = new List<Rating>
        {
            new Rating { Id = 1, Puntuacion = 5, ProductoId = productId },
            new Rating { Id = 2, Puntuacion = 4, ProductoId = productId }
        };

        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(ratings));

        // Act
        await _container.EnsureLoadedAsync(productId);

        // Assert
        _container.Ratings.Should().HaveCount(2);
        _container.CurrentProductId.Should().Be(productId);
        _ratingServiceMock.Verify(s => s.GetByProductoIdAsync(productId), Times.Once);
    }

    /// <summary>
    /// PRUEBA: Optimización de carga (Caché).
    /// OBJETIVO: Confirmar que no se vuelve a llamar al servicio si los datos ya están en memoria.
    /// </summary>
    [Test]
    public async Task EnsureLoadedAsync_ShouldNotLoadRatings_WhenProductIdIsSameAndRatingsAreLoaded()
    {
        // Arrange
        var productId = 1L;
        var ratings = new List<Rating>
        {
            new Rating { Id = 1, Puntuacion = 5, ProductoId = productId }
        };

        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(ratings));

        // Load initially
        await _container.EnsureLoadedAsync(productId);
        _ratingServiceMock.Invocations.Clear(); // Clear previous calls

        // Act
        await _container.EnsureLoadedAsync(productId);

        // Assert
        _ratingServiceMock.Verify(s => s.GetByProductoIdAsync(It.IsAny<long>()), Times.Never);
    }

    /// <summary>
    /// PRUEBA: Recarga forzada y notificación.
    /// OBJETIVO: Validar que RefreshAsync actualiza datos y dispara el evento OnChange.
    /// </summary>
    [Test]
    public async Task RefreshAsync_ShouldReloadRatings_AndNotifyChange()
    {
        // Arrange
        var productId = 1L;
        var ratings = new List<Rating>
        {
            new Rating { Id = 1, Puntuacion = 5, ProductoId = productId }
        };
        var eventTriggered = false;
        _container.OnChange += () => eventTriggered = true;

        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(ratings));

        // Act
        await _container.RefreshAsync(productId);

        // Assert
        _container.Ratings.Should().HaveCount(1);
        _container.CurrentProductId.Should().Be(productId);
        eventTriggered.Should().BeTrue();
        _ratingServiceMock.Verify(s => s.GetByProductoIdAsync(productId), Times.Once);
    }

    /// <summary>
    /// PRUEBA: Cálculo de promedio en el contenedor.
    /// OBJETIVO: Asegurar que el promedio expuesto coincide con los datos cargados.
    /// </summary>
    [Test]
    public void Average_ShouldReturnCorrectAverage_WhenRatingsExist()
    {
        // Arrange
        var productId = 1L;
        var ratings = new List<Rating>
        {
            new Rating { Id = 1, Puntuacion = 5, ProductoId = productId },
            new Rating { Id = 2, Puntuacion = 3, ProductoId = productId }
        }; // Average = 4

        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(ratings));

        // Act
        _container.RefreshAsync(productId).Wait();

        // Assert
        _container.Average.Should().Be(4);
    }

    /// <summary>
    /// PRUEBA: Promedio por defecto.
    /// OBJETIVO: Verificar que el promedio es 0 cuando no hay datos.
    /// </summary>
    [Test]
    public void Average_ShouldReturnZero_WhenRatingsAreNullOrEmpty()
    {
        // Assert
        _container.Average.Should().Be(0);
    }

    /// <summary>
    /// PRUEBA: Conteo de valoraciones.
    /// OBJETIVO: Confirmar que el contador refleja el número de elementos en memoria.
    /// </summary>
    [Test]
    public void Count_ShouldReturnCorrectCount_WhenRatingsExist()
    {
        // Arrange
        var productId = 1L;
        var ratings = new List<Rating>
        {
            new Rating { Id = 1, Puntuacion = 5, ProductoId = productId },
            new Rating { Id = 2, Puntuacion = 3, ProductoId = productId }
        };

        _ratingServiceMock.Setup(s => s.GetByProductoIdAsync(productId))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(ratings));

        // Act
        _container.RefreshAsync(productId).Wait();

        // Assert
        _container.Count.Should().Be(2);
    }

    /// <summary>
    /// PRUEBA: Conteo inicial.
    /// OBJETIVO: Verificar que el contador es 0 inicialmente.
    /// </summary>
    [Test]
    public void Count_ShouldReturnZero_WhenRatingsAreNull()
    {
        // Assert
        _container.Count.Should().Be(0);
    }
}
