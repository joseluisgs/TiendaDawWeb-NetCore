using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Implementations;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Tests.Services;

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

    [Test]
    public void Average_ShouldReturnCorrectAverage_WhenRatingsExist()
    {
        // Arrange
        // Manually loading state via reflection or by calling public methods if setter was public (it's private set).
        // Since we can't set property directly, we use RefreshAsync to populate it.
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

    [Test]
    public void Average_ShouldReturnZero_WhenRatingsAreNullOrEmpty()
    {
        // Assert
        _container.Average.Should().Be(0);
    }

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

    [Test]
    public void Count_ShouldReturnZero_WhenRatingsAreNull()
    {
        // Assert
        _container.Count.Should().Be(0);
    }
}
