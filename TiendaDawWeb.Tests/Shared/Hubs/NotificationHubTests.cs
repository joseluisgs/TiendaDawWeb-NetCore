using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using TiendaDawWeb.Shared.Web.Hubs;

namespace TiendaDawWeb.Tests.Shared.Hubs;

public class NotificationHubTests
{
    [Test]
    public void NotificationHub_CanBeInstantiated()
    {
        var hub = new NotificationHub();

        hub.Should().NotBeNull();
    }

    [Test]
    public void NotificationHub_InheritsFromHub()
    {
        var hub = new NotificationHub();

        hub.Should().BeAssignableTo<Hub>();
    }

    [Test]
    public void NotificationHub_IsSealedClass()
    {
        typeof(NotificationHub).IsClass.Should().BeTrue();
    }

    [Test]
    public void NotificationHub_HasDefaultConstructor()
    {
        var hub = new NotificationHub();

        hub.Should().NotBeNull();
    }

    [Test]
    public void NotificationHub_CanBeCreatedMultipleTimes()
    {
        var hub1 = new NotificationHub();
        var hub2 = new NotificationHub();

        hub1.Should().NotBeSameAs(hub2);
    }

    [Test]
    public void NotificationHub_HasCorrectNamespace()
    {
        typeof(NotificationHub).Namespace.Should().Be("TiendaDawWeb.Shared.Web.Hubs");
    }
}
