using FluentAssertions;
using TiendaDawWeb.Shared.Mappers;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Tests.Shared.Mappers;

public class ProductMapperTests
{
    [Test]
    public void ToViewModel_MapsAllProperties_Success()
    {
        var product = new Product
        {
            Id = 1,
            Nombre = "iPhone 15",
            Descripcion = "Latest iPhone model",
            Precio = 999.99m,
            Categoria = ProductCategory.SMARTPHONES,
            Imagen = "/uploads/iphone15.jpg"
        };

        var result = product.ToViewModel();

        result.Id.Should().Be(1);
        result.Nombre.Should().Be("iPhone 15");
        result.Descripcion.Should().Be("Latest iPhone model");
        result.Precio.Should().Be(999.99m);
        result.Categoria.Should().Be(ProductCategory.SMARTPHONES);
        result.ImagenUrl.Should().Be("/uploads/iphone15.jpg");
    }

    [Test]
    public void ToViewModel_HandlesNullImagen_Success()
    {
        var product = new Product
        {
            Id = 1,
            Nombre = "Test",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.LAPTOPS,
            Imagen = null
        };

        var result = product.ToViewModel();

        result.ImagenUrl.Should().BeNull();
    }

    [Test]
    public void ToEntity_CreatesEntity_Success()
    {
        var model = new ProductViewModel
        {
            Id = 0,
            Nombre = "MacBook Pro",
            Descripcion = "Professional laptop",
            Precio = 1999.99m,
            Categoria = ProductCategory.LAPTOPS,
            ImagenUrl = null
        };

        var result = model.ToEntity(propietarioId: 5, imagenUrl: "/uploads/macbook.jpg");

        result.Nombre.Should().Be("MacBook Pro");
        result.Descripcion.Should().Be("Professional laptop");
        result.Precio.Should().Be(1999.99m);
        result.Categoria.Should().Be(ProductCategory.LAPTOPS);
        result.PropietarioId.Should().Be(5);
        result.Imagen.Should().Be("/uploads/macbook.jpg");
    }

    [Test]
    public void ToEntity_UsesImagenUrl_WhenImagenUrlProvided()
    {
        var model = new ProductViewModel
        {
            Id = 0,
            Nombre = "Test",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.AUDIO,
            ImagenUrl = "/existing/image.jpg"
        };

        var result = model.ToEntity(propietarioId: 1, imagenUrl: null);

        result.Imagen.Should().Be("/existing/image.jpg");
    }

    [Test]
    public void ToEntity_WithIncludeId_SetsId_Success()
    {
        var model = new ProductViewModel
        {
            Id = 42,
            Nombre = "Test",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.GAMING
        };

        var result = model.ToEntity(propietarioId: 1, imagenUrl: null, includeId: true);

        result.Id.Should().Be(42);
    }

    [Test]
    public void ToEntity_WithoutIncludeId_DoesNotSetId_Success()
    {
        var model = new ProductViewModel
        {
            Id = 42,
            Nombre = "Test",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.ACCESSORIES
        };

        var result = model.ToEntity(propietarioId: 1, imagenUrl: null, includeId: false);

        result.Id.Should().Be(0);
    }

    [Test]
    public void UpdateEntity_UpdatesAllProperties_Success()
    {
        var product = new Product
        {
            Id = 1,
            Nombre = "Old Name",
            Descripcion = "Old Desc",
            Precio = 50,
            Categoria = ProductCategory.SMARTPHONES,
            Imagen = "/old/image.jpg"
        };

        var model = new ProductViewModel
        {
            Id = 1,
            Nombre = "New Name",
            Descripcion = "New Desc",
            Precio = 150,
            Categoria = ProductCategory.LAPTOPS,
            ImagenUrl = null
        };

        model.UpdateEntity(product, imagenUrl: "/new/image.jpg");

        product.Nombre.Should().Be("New Name");
        product.Descripcion.Should().Be("New Desc");
        product.Precio.Should().Be(150);
        product.Categoria.Should().Be(ProductCategory.LAPTOPS);
        product.Imagen.Should().Be("/new/image.jpg");
    }

    [Test]
    public void UpdateEntity_DoesNotUpdateImagen_WhenNull_Success()
    {
        var product = new Product
        {
            Id = 1,
            Nombre = "Name",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.GAMING,
            Imagen = "/existing/image.jpg"
        };

        var model = new ProductViewModel
        {
            Id = 1,
            Nombre = "Updated Name",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.GAMING,
            ImagenUrl = null
        };

        model.UpdateEntity(product, imagenUrl: null);

        product.Imagen.Should().Be("/existing/image.jpg");
        product.Nombre.Should().Be("Updated Name");
    }
}
