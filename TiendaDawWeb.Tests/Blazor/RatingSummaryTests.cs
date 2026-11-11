using System.Reactive.Linq;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Blazor.Ratings;
using TiendaDawWeb.Shared.Services.Rating;
using TestContext = Bunit.BunitContext;

namespace TiendaDawWeb.Tests.Blazor;

public class RatingSummaryTests {
    private readonly Mock<ILogger<RatingSummary>> _mockLogger;

    public RatingSummaryTests() {
        _mockLogger = new Mock<ILogger<RatingSummary>>();
    }

    private IRatingStore CreateMockRatingStore() {
        var mock = new Mock<IRatingStore>();
        IObservable<RatingState> emptyObservable = Observable.Empty<RatingState>();
        mock.Setup(x => x.State).Returns(emptyObservable);
        return mock.Object;
    }

    [Test]
    public void RatingSummary_Component_CanBeCreated() {
#pragma warning disable CS0618
        using var ctx = new TestContext();
#pragma warning restore CS0618
        ctx.Services.AddSingleton(CreateMockRatingStore());
        ctx.Services.AddSingleton(_mockLogger.Object);

        IRenderedComponent<RatingSummary> cut = ctx.Render<RatingSummary>();

        cut.Should().NotBeNull();
    }

    [Test]
    public void RatingSummary_ImplementsIDisposable() {
        typeof(RatingSummary).Should().Implement<IDisposable>();
    }
}
