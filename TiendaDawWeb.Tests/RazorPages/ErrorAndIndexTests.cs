#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using TiendaDawWeb.RazorPages.Pages;

namespace TiendaDawWeb.Tests.RazorPages;

public class ErrorModelTests
{
    [Test]
    public void OnGet_SetsRequestId()
    {
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext { HttpContext = httpContext };

        var model = new ErrorModel { PageContext = pageContext };
        model.OnGet();

        model.RequestId.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void OnGet_UsesTraceIdentifier()
    {
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext { HttpContext = httpContext };

        var model = new ErrorModel { PageContext = pageContext };
        model.OnGet();

        model.RequestId.Should().Be(httpContext.TraceIdentifier);
    }

    [Test]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdIsNotEmpty()
    {
        var model = new ErrorModel { RequestId = "test-id" };
        model.ShowRequestId.Should().BeTrue();
    }

    [Test]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNull()
    {
        var model = new ErrorModel { RequestId = null };
        model.ShowRequestId.Should().BeFalse();
    }

    [Test]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsEmpty()
    {
        var model = new ErrorModel { RequestId = string.Empty };
        model.ShowRequestId.Should().BeFalse();
    }

    [Test]
    public void ErrorModel_CanBeInstantiated()
    {
        var model = new ErrorModel();
        model.Should().NotBeNull();
    }

    [Test]
    public void ErrorModel_HasCorrectAttributes()
    {
        typeof(ErrorModel).Should().BeDerivedFrom<PageModel>();
    }
}

public class IndexModelTests
{
    [Test]
    public void OnGet_RedirectsToPublicIndex()
    {
        var model = new IndexModel();
        var result = model.OnGet(search: null, q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = result as RedirectToPageResult;
        redirect!.PageName.Should().Be("/Public/Index");
    }

    [Test]
    public void OnGet_WithSearchQuery_PassesQueryToRedirect()
    {
        var model = new IndexModel();
        var result = model.OnGet(search: "test", q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = result as RedirectToPageResult;
        redirect!.RouteValues.Should().ContainKey("q");
        redirect!.RouteValues["q"].Should().Be("test");
    }

    [Test]
    public void OnGet_WithQParameter_PassesQueryToRedirect()
    {
        var model = new IndexModel();
        var result = model.OnGet(search: null, q: "laptop", categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = result as RedirectToPageResult;
        redirect!.RouteValues.Should().ContainKey("q");
        redirect!.RouteValues["q"].Should().Be("laptop");
    }

    [Test]
    public void OnGet_PrefersSearchOverQ()
    {
        var model = new IndexModel();
        var result = model.OnGet(search: "priority", q: "secondary", categoria: null, minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = result as RedirectToPageResult;
        redirect!.RouteValues["q"].Should().Be("priority");
    }

    [Test]
    public void OnGet_WithCategory_PassesCategoryToRedirect()
    {
        var model = new IndexModel();
        var result = model.OnGet(search: null, q: null, categoria: "electronics", minPrecio: null, maxPrecio: null, page: 1, size: 12);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = result as RedirectToPageResult;
        redirect!.RouteValues.Should().ContainKey("categoria");
        redirect!.RouteValues["categoria"].Should().Be("electronics");
    }

    [Test]
    public void OnGet_WithPriceRange_PassesPricesToRedirect()
    {
        var model = new IndexModel();
        var result = model.OnGet(search: null, q: null, categoria: null, minPrecio: 100, maxPrecio: 500, page: 1, size: 12);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = result as RedirectToPageResult;
        redirect!.RouteValues["minPrecio"].Should().Be(100);
        redirect!.RouteValues["maxPrecio"].Should().Be(500);
    }

    [Test]
    public void OnGet_WithPagination_PassesPageToRedirect()
    {
        var model = new IndexModel();
        var result = model.OnGet(search: null, q: null, categoria: null, minPrecio: null, maxPrecio: null, page: 3, size: 24);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = result as RedirectToPageResult;
        redirect!.RouteValues["page"].Should().Be(3);
        redirect!.RouteValues["size"].Should().Be(24);
    }

    [Test]
    public void OnGet_WithAllParameters_RedirectsWithAll()
    {
        var model = new IndexModel();
        var result = model.OnGet(
            search: "phone",
            q: null,
            categoria: "electronics",
            minPrecio: 200,
            maxPrecio: 1000,
            page: 2,
            size: 12);

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = result as RedirectToPageResult;
        redirect!.RouteValues["q"].Should().Be("phone");
        redirect!.RouteValues["categoria"].Should().Be("electronics");
        redirect!.RouteValues["minPrecio"].Should().Be(200);
        redirect!.RouteValues["maxPrecio"].Should().Be(1000);
        redirect!.RouteValues["page"].Should().Be(2);
        redirect!.RouteValues["size"].Should().Be(12);
    }

    [Test]
    public void IndexModel_CanBeInstantiated()
    {
        var model = new IndexModel();
        model.Should().NotBeNull();
    }
}
