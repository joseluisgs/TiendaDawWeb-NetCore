#nullable disable
using FluentAssertions;
using TiendaDawWeb.RazorPages.Pages.Product;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Tests.RazorPages.Product;

public class ProductDetailsModelTests {
    [Test]
    public void ProductDetailsModel_CanBeInstantiated() {
        var model = new DetailsModel(null!, null!, null!, null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void ProductDetailsModel_HasProductProperty() {
        var model = new DetailsModel(null!, null!, null!, null!, null!);
        model.Product.Should().Be(default(ProductModel));
    }
}

public class ProductIndexModelTests {
    [Test]
    public void ProductIndexModel_CanBeInstantiated() {
        var model = new IndexModel(null!, null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void ProductIndexModel_ProductsDefaultToEmpty() {
        var model = new IndexModel(null!, null!, null!);
        model.Products.Should().NotBeNull();
    }
}
