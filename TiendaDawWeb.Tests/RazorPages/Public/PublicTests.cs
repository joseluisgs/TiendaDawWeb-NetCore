#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.RazorPages.Pages.Public;

namespace TiendaDawWeb.Tests.RazorPages.Public;

public class PublicIndexModelTests
{
    [Test]
    public void PublicIndexModel_CanBeInstantiated()
    {
        var model = new IndexModel(null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void PublicIndexModel_HasProductsProperty()
    {
        var model = new IndexModel(null!, null!);
        model.Products.Should().NotBeNull();
    }

    [Test]
    public void PublicIndexModel_ProductsDefaultIsEmpty()
    {
        var model = new IndexModel(null!, null!);
        model.Products.Should().BeEmpty();
    }

    [Test]
    public void PublicIndexModel_HasCorrectAttributes()
    {
        typeof(IndexModel).Should().BeDerivedFrom<PageModel>();
    }
}
