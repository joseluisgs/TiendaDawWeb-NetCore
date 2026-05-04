using FluentAssertions;
using TiendaDawWeb.Shared.Services.Rating;

namespace TiendaDawWeb.Tests.Shared.Services.Rating;

public class RatingStateTests
{
    [Test]
    public void RatingState_DefaultConstructor_HasEmptyRatings()
    {
        var state = new RatingState();

        state.Ratings.Should().BeEmpty();
        state.CurrentProductId.Should().Be(0);
    }

    [Test]
    public void RatingState_ConstructorWithRatings_HasCorrectValues()
    {
        var ratings = new List<TiendaDawWeb.Shared.Models.Rating>
        {
            new() { Id = 1, Puntuacion = 5 },
            new() { Id = 2, Puntuacion = 4 }
        };

        var state = new RatingState(ratings, 100);

        state.Ratings.Should().HaveCount(2);
        state.CurrentProductId.Should().Be(100);
    }

    [Test]
    public void RatingState_Average_ReturnsCorrectValue()
    {
        var ratings = new List<TiendaDawWeb.Shared.Models.Rating>
        {
            new() { Id = 1, Puntuacion = 5 },
            new() { Id = 2, Puntuacion = 3 }
        };

        var state = new RatingState(ratings, 1);

        state.Average.Should().Be(4);
    }

    [Test]
    public void RatingState_Average_ReturnsZero_WhenNoRatings()
    {
        var state = new RatingState();

        state.Average.Should().Be(0);
    }

    [Test]
    public void RatingState_Count_ReturnsCorrectValue()
    {
        var ratings = new List<TiendaDawWeb.Shared.Models.Rating>
        {
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 }
        };

        var state = new RatingState(ratings, 1);

        state.Count.Should().Be(3);
    }

    [Test]
    public void RatingState_Count_ReturnsZero_WhenNoRatings()
    {
        var state = new RatingState();

        state.Count.Should().Be(0);
    }

    [Test]
    public void RatingState_HasRatings_ReturnsTrue_WhenHasRatings()
    {
        var ratings = new List<TiendaDawWeb.Shared.Models.Rating> { new() { Id = 1 } };

        var state = new RatingState(ratings, 1);

        state.HasRatings.Should().BeTrue();
    }

    [Test]
    public void RatingState_HasRatings_ReturnsFalse_WhenEmpty()
    {
        var state = new RatingState();

        state.HasRatings.Should().BeFalse();
    }

    [Test]
    public void RatingState_IsImmutable()
    {
        var state1 = new RatingState(new List<TiendaDawWeb.Shared.Models.Rating>(), 1);
        var state2 = new RatingState(new List<TiendaDawWeb.Shared.Models.Rating>(), 2);

        state1.Should().NotBeSameAs(state2);
    }
}