using FluentAssertions;
using Moq;
using System.Reflection;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Rating;
using RatingModel = TiendaDawWeb.Shared.Models.Rating;

namespace TiendaDawWeb.Tests.Shared.Services.Rating;

public class RatingStateContainerTests
{
    private Mock<IRatingService> _ratingServiceMock = null!;
    private RatingStateContainer _container = null!;

    [SetUp]
    public void Setup()
    {
        _ratingServiceMock = new Mock<IRatingService>();
        _container = new RatingStateContainer(_ratingServiceMock.Object);
    }

    private void SetRatings(List<RatingModel>? ratings)
    {
        var property = typeof(RatingStateContainer).GetProperty("Ratings", 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        property?.SetValue(_container, ratings);
    }

    private void SetCurrentProductId(long productId)
    {
        var property = typeof(RatingStateContainer).GetProperty("CurrentProductId",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        property?.SetValue(_container, productId);
    }

    [Test]
    public void Constructor_SetsRatingService()
    {
        _container.Should().NotBeNull();
    }

    [Test]
    public void Ratings_IsNullInitially()
    {
        var ratings = _container.Ratings;
        ratings.Should().BeNull();
    }

    [Test]
    public void CurrentProductId_IsZeroInitially()
    {
        var productId = _container.CurrentProductId;
        productId.Should().Be(0);
    }

    [Test]
    public void Average_IsZero_WhenRatingsIsNull()
    {
        SetRatings(null);
        var average = _container.Average;
        average.Should().Be(0);
    }

    [Test]
    public void Average_IsZero_WhenRatingsIsEmpty()
    {
        SetRatings(new List<RatingModel>());
        var average = _container.Average;
        average.Should().Be(0);
    }

    [Test]
    public void Average_CalculatesCorrectly_WhenHasRatings()
    {
        SetRatings(new List<RatingModel>
        {
            new RatingModel { Puntuacion = 4 },
            new RatingModel { Puntuacion = 5 },
            new RatingModel { Puntuacion = 3 }
        });
        var average = _container.Average;
        average.Should().BeApproximately(4.0, 0.01);
    }

    [Test]
    public void Count_IsZero_WhenRatingsIsNull()
    {
        SetRatings(null);
        var count = _container.Count;
        count.Should().Be(0);
    }

    [Test]
    public void Count_ReturnsCorrectCount()
    {
        SetRatings(new List<RatingModel>
        {
            new RatingModel(),
            new RatingModel(),
            new RatingModel()
        });
        var count = _container.Count;
        count.Should().Be(3);
    }

    [Test]
    public void NotifyRatingChanged_InvokesOnChange()
    {
        var invoked = false;
        _container.OnChange += () => invoked = true;
        _container.NotifyRatingChanged();
        invoked.Should().BeTrue();
    }

    [Test]
    public void NotifyRatingChanged_CanHandleMultipleSubscribers()
    {
        var count = 0;
        _container.OnChange += () => count++;
        _container.OnChange += () => count++;
        _container.NotifyRatingChanged();
        count.Should().Be(2);
    }

    [Test]
    public void Average_SingleRating_ReturnsRatingValue()
    {
        SetRatings(new List<RatingModel>
        {
            new RatingModel { Puntuacion = 5 }
        });
        var average = _container.Average;
        average.Should().Be(5);
    }

    [Test]
    public void Count_WithNullRatings_ReturnsZero()
    {
        SetRatings(null);
        _container.Ratings.Should().BeNull();
        _container.Count.Should().Be(0);
    }
}
