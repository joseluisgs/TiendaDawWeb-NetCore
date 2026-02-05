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

public class RatingSectionTests
{
    private readonly Mock<IRatingService> _mockRatingService;
    private readonly RatingStateContainer _stateContainer;

    public RatingSectionTests()
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
        _mockRatingService.Setup(s => s.CanUserRateProductAsync(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.FromResult(Result.Success<bool, DomainError>(true)));
        _stateContainer = new RatingStateContainer(_mockRatingService.Object);
    }

    [Test]
    public void RatingSection_Component_CanBeCreated()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSection>(parameters => parameters
            .Add(p => p.ProductId, 1)
            .Add(p => p.CurrentUserId, 1)
            .Add(p => p.IsAuthenticated, true)
            .Add(p => p.IsOwner, false));

        cut.Should().NotBeNull();
    }

    [Test]
    public void RatingSection_AcceptsProductId_Parameter()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSection>(parameters => parameters
            .Add(p => p.ProductId, 123)
            .Add(p => p.CurrentUserId, 1)
            .Add(p => p.IsAuthenticated, true)
            .Add(p => p.IsOwner, false));

        cut.Instance.ProductId.Should().Be(123);
    }

    [Test]
    public void RatingSection_AcceptsCurrentUserId_Parameter()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSection>(parameters => parameters
            .Add(p => p.ProductId, 1)
            .Add(p => p.CurrentUserId, 456)
            .Add(p => p.IsAuthenticated, true)
            .Add(p => p.IsOwner, false));

        cut.Instance.CurrentUserId.Should().Be(456);
    }

    [Test]
    public void RatingSection_AcceptsIsAuthenticated_Parameter()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSection>(parameters => parameters
            .Add(p => p.ProductId, 1)
            .Add(p => p.CurrentUserId, 1)
            .Add(p => p.IsAuthenticated, true)
            .Add(p => p.IsOwner, false));

        cut.Instance.IsAuthenticated.Should().BeTrue();
    }

    [Test]
    public void RatingSection_AcceptsIsOwner_Parameter()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSection>(parameters => parameters
            .Add(p => p.ProductId, 1)
            .Add(p => p.CurrentUserId, 1)
            .Add(p => p.IsAuthenticated, true)
            .Add(p => p.IsOwner, true));

        cut.Instance.IsOwner.Should().BeTrue();
    }

    [Test]
    public void RatingSection_AcceptsProductUrl_Parameter()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSection>(parameters => parameters
            .Add(p => p.ProductId, 1)
            .Add(p => p.CurrentUserId, 1)
            .Add(p => p.IsAuthenticated, true)
            .Add(p => p.IsOwner, false)
            .Add(p => p.ProductUrl, "/products/1"));

        cut.Instance.ProductUrl.Should().Be("/products/1");
    }

    [Test]
    public void RatingSection_DefaultProductId_IsZero()
    {
#pragma warning disable CS0618
        using var ctx = new Bunit.TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(_mockRatingService.Object);
        ctx.Services.AddSingleton(_stateContainer);

        var cut = ctx.Render<RatingSection>(parameters => parameters
            .Add(p => p.CurrentUserId, 1)
            .Add(p => p.IsAuthenticated, true)
            .Add(p => p.IsOwner, false));

        cut.Instance.ProductId.Should().Be(0);
    }
}
