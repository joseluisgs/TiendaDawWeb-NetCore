using Bunit;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TiendaDawWeb.Shared.Blazor.Ratings;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Rating;

namespace TiendaDawWeb.Tests.Blazor;

public class RatingSummaryTests
{
    private readonly Mock<IRatingService> _mockRatingService;
    private readonly RatingStateContainer _stateContainer;

    public RatingSummaryTests()
    {
        _mockRatingService = new Mock<IRatingService>();
        var ratings = new List<Rating>
        {
            new() { Id = 1, ProductoId = 1, Puntuacion = 4, UsuarioId = 1 },
            new() { Id = 2, ProductoId = 1, Puntuacion = 5, UsuarioId = 2 }
        };
        var result = Result.Success<IEnumerable<Rating>, DomainError>(ratings);
        _mockRatingService.Setup(s => s.GetByProductoIdAsync(It.IsAny<long>()))
            .Returns(Task.FromResult(result));
        _stateContainer = new RatingStateContainer(_mockRatingService.Object);
    }

    [Test]
    public void RatingSummary_Component_CanBeCreated()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSummary>(parameters => parameters
            .Add(p => p.ProductId, 1));

        cut.Should().NotBeNull();
    }

    [Test]
    public void RatingSummary_AcceptsProductId_Parameter()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSummary>(parameters => parameters
            .Add(p => p.ProductId, 123));

        cut.Instance.ProductId.Should().Be(123);
    }

    [Test]
    public void RatingSummary_DefaultProductId_IsZero()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSummary>();

        cut.Instance.ProductId.Should().Be(0);
    }

    [Test]
    public void RatingSummary_ImplementsIDisposable()
    {
        typeof(RatingSummary).Should().Implement<IDisposable>();
    }

    [Test]
    public void RatingSummary_HasProductIdProperty()
    {
        var propertyInfo = typeof(RatingSummary).GetProperty("ProductId");
        propertyInfo.Should().NotBeNull();
    }
}
