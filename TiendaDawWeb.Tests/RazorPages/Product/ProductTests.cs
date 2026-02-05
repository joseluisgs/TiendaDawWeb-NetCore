#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.RazorPages.Pages.Product;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Favorite;
using TiendaDawWeb.Shared.Services.Product;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Tests.RazorPages.Product;

public class ProductDetailsModelTests
{
    [Test]
    public void ProductDetailsModel_CanBeInstantiated()
    {
        var model = new DetailsModel(null!, null!, null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void ProductDetailsModel_HasProductProperty()
    {
        var model = new DetailsModel(null!, null!, null!, null!);
        model.Product.Should().Be(default(ProductModel));
    }
}

public class ProductIndexModelTests
{
    [Test]
    public void ProductIndexModel_CanBeInstantiated()
    {
        var model = new IndexModel(null!, null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void ProductIndexModel_ProductsDefaultToEmpty()
    {
        var model = new IndexModel(null!, null!, null!);
        model.Products.Should().NotBeNull();
    }
}
