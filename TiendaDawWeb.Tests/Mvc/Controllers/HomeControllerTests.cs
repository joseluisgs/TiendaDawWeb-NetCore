#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Controllers;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class HomeControllerTests {
    private readonly HomeController _controller;

    public HomeControllerTests() {
        _controller = new HomeController();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() {
        _controller.Dispose();
    }

    [Test]
    public void Index_RedirectsToPublic_WithSearchQuery() {
        IActionResult result = _controller.Index("test", null, null, null, null);

        result.Should().BeOfType<RedirectToActionResult>();
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("Public");
        redirect.ActionName.Should().Be("Index");
        redirect.RouteValues.Should().ContainKey("q");
        redirect.RouteValues["q"].Should().Be("test");
    }

    [Test]
    public void Index_RedirectsToPublic_WithQParameter() {
        IActionResult result = _controller.Index(null, "laptop", null, null, null);

        result.Should().BeOfType<RedirectToActionResult>();
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("Public");
        redirect.RouteValues["q"].Should().Be("laptop");
    }

    [Test]
    public void Index_RedirectsToPublic_WithCategory() {
        IActionResult result = _controller.Index(null, null, "electronics", null, null);

        result.Should().BeOfType<RedirectToActionResult>();
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["categoria"].Should().Be("electronics");
    }

    [Test]
    public void Index_RedirectsToPublic_WithPriceRange() {
        IActionResult result = _controller.Index(null, null, null, 100, 500);

        result.Should().BeOfType<RedirectToActionResult>();
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["minPrecio"].Should().Be(100);
        redirect.RouteValues["maxPrecio"].Should().Be(500);
    }

    [Test]
    public void Index_RedirectsToPublic_WithPagination() {
        IActionResult result = _controller.Index(null, null, null, null, null, 3, 24);

        result.Should().BeOfType<RedirectToActionResult>();
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["page"].Should().Be(3);
        redirect.RouteValues["size"].Should().Be(24);
    }

    [Test]
    public void Index_PrefersSearchOverQ() {
        IActionResult result = _controller.Index("priority", "secondary", null, null, null);

        result.Should().BeOfType<RedirectToActionResult>();
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["q"].Should().Be("priority");
    }

    [Test]
    public void Index_RedirectsWithAllParameters() {
        IActionResult result = _controller.Index(
            "phone",
            null,
            "electronics",
            200,
            1000,
            2);

        result.Should().BeOfType<RedirectToActionResult>();
        RedirectToActionResult redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["q"].Should().Be("phone");
        redirect.RouteValues["categoria"].Should().Be("electronics");
        redirect.RouteValues["minPrecio"].Should().Be(200);
        redirect.RouteValues["maxPrecio"].Should().Be(1000);
        redirect.RouteValues["page"].Should().Be(2);
        redirect.RouteValues["size"].Should().Be(12);
    }
}
