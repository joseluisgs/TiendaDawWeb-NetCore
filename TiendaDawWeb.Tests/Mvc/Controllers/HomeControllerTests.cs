#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Controllers;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class HomeControllerTests
{
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _controller = new HomeController();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public void Index_RedirectsToPublic_WithSearchQuery()
    {
        var result = _controller.Index(search: "test", q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("Public");
        redirect.ActionName.Should().Be("Index");
        redirect.RouteValues.Should().ContainKey("q");
        redirect.RouteValues["q"].Should().Be("test");
    }

    [Test]
    public void Index_RedirectsToPublic_WithQParameter()
    {
        var result = _controller.Index(search: null, q: "laptop", categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("Public");
        redirect.RouteValues["q"].Should().Be("laptop");
    }

    [Test]
    public void Index_RedirectsToPublic_WithCategory()
    {
        var result = _controller.Index(search: null, q: null, categoria: "electronics", minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["categoria"].Should().Be("electronics");
    }

    [Test]
    public void Index_RedirectsToPublic_WithPriceRange()
    {
        var result = _controller.Index(search: null, q: null, categoria: null, minPrecio: 100, maxPrecio: 500, page: 1, size: 12);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["minPrecio"].Should().Be(100);
        redirect.RouteValues["maxPrecio"].Should().Be(500);
    }

    [Test]
    public void Index_RedirectsToPublic_WithPagination()
    {
        var result = _controller.Index(search: null, q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 3, size: 24);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["page"].Should().Be(3);
        redirect.RouteValues["size"].Should().Be(24);
    }

    [Test]
    public void Index_PrefersSearchOverQ()
    {
        var result = _controller.Index(search: "priority", q: "secondary", categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["q"].Should().Be("priority");
    }

    [Test]
    public void Index_RedirectsWithAllParameters()
    {
        var result = _controller.Index(
            search: "phone",
            q: null,
            categoria: "electronics",
            minPrecio: 200,
            maxPrecio: 1000,
            page: 2,
            size: 12);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.RouteValues["q"].Should().Be("phone");
        redirect.RouteValues["categoria"].Should().Be("electronics");
        redirect.RouteValues["minPrecio"].Should().Be(200);
        redirect.RouteValues["maxPrecio"].Should().Be(1000);
        redirect.RouteValues["page"].Should().Be(2);
        redirect.RouteValues["size"].Should().Be(12);
    }
}
