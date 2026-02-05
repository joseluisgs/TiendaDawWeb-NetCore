using FluentAssertions;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Tests.Shared.Models;

public class ProductTests
{
    [Test]
    public void Product_CanSetAllProperties()
    {
        var product = new Product
        {
            Id = 1,
            Nombre = "iPhone 15",
            Descripcion = "Latest iPhone",
            Precio = 999.99m,
            Categoria = ProductCategory.SMARTPHONES,
            Imagen = "https://example.com/iphone.jpg",
            Reservado = true,
            ReservadoHasta = DateTime.UtcNow.AddDays(7),
            ReservadoPor = 100,
            Deleted = false,
            DeletedAt = null,
            DeletedBy = null,
            PropietarioId = 1,
            CompraId = null,
            Compra = null
        };

        product.Id.Should().Be(1);
        product.Nombre.Should().Be("iPhone 15");
        product.Descripcion.Should().Be("Latest iPhone");
        product.Precio.Should().Be(999.99m);
        product.Categoria.Should().Be(ProductCategory.SMARTPHONES);
        product.Imagen.Should().Be("https://example.com/iphone.jpg");
        product.Reservado.Should().BeTrue();
        product.ReservadoHasta.Should().NotBeNull();
        product.ReservadoPor.Should().Be(100);
        product.Deleted.Should().BeFalse();
        product.DeletedAt.Should().BeNull();
        product.DeletedBy.Should().BeNull();
        product.PropietarioId.Should().Be(1);
        product.CompraId.Should().BeNull();
    }

    [Test]
    public void Product_SupportsCollections()
    {
        var product = new Product
        {
            Favorites = new List<Favorite>(),
            Ratings = new List<Rating>()
        };

        product.Favorites.Should().NotBeNull();
        product.Ratings.Should().NotBeNull();
    }

    [Test]
    public void Product_DefaultReservado_IsFalse()
    {
        var product = new Product();
        product.Reservado.Should().BeFalse();
    }

    [Test]
    public void Product_CanSetDeletedProperties()
    {
        var now = DateTime.UtcNow;
        var product = new Product
        {
            Deleted = true,
            DeletedAt = now,
            DeletedBy = "admin"
        };

        product.Deleted.Should().BeTrue();
        product.DeletedAt.Should().Be(now);
        product.DeletedBy.Should().Be("admin");
    }

    [Test]
    public void ImagenOrDefault_ReturnsDefault_WhenNull()
    {
        var product = new Product { Imagen = null };

        product.ImagenOrDefault.Should().Be("/images/default-product.svg");
    }

    [Test]
    public void ImagenOrDefault_ReturnsDefault_WhenEmpty()
    {
        var product = new Product { Imagen = string.Empty };

        product.ImagenOrDefault.Should().Be("/images/default-product.svg");
    }

    [Test]
    public void ImagenOrDefault_ReturnsHttpUrl_WhenHttp()
    {
        var product = new Product { Imagen = "https://example.com/image.jpg" };

        product.ImagenOrDefault.Should().Be("https://example.com/image.jpg");
    }

    [Test]
    public void ImagenOrDefault_ReturnsSlashUrl_WhenSlash()
    {
        var product = new Product { Imagen = "/uploads/image.jpg" };

        product.ImagenOrDefault.Should().Be("/uploads/image.jpg");
    }

    [Test]
    public void ImagenOrDefault_ReturnsRelativePath_WithPrefix()
    {
        var product = new Product { Imagen = "uploads/image.jpg" };

        product.ImagenOrDefault.Should().Be("/uploads/uploads/image.jpg");
    }

    [Test]
    public void RatingPromedio_ReturnsZero_WhenNoRatings()
    {
        var product = new Product { Ratings = new List<Rating>() };

        product.RatingPromedio.Should().Be(0);
    }

    [Test]
    public void RatingPromedio_ReturnsAverage_WhenHasRatings()
    {
        var product = new Product
        {
            Ratings = new List<Rating>
            {
                new Rating { Puntuacion = 4 },
                new Rating { Puntuacion = 5 },
                new Rating { Puntuacion = 3 }
            }
        };

        product.RatingPromedio.Should().BeApproximately(4.0, 0.01);
    }

    [Test]
    public void SoftDelete_SetsDeletedProperties()
    {
        var product = new Product();

        product.SoftDelete("User-123");

        product.Deleted.Should().BeTrue();
        product.DeletedBy.Should().Be("User-123");
        product.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public void SoftDelete_SetsDeletedAtToNow()
    {
        var product = new Product();
        var beforeCall = DateTime.UtcNow.AddSeconds(-1);

        product.SoftDelete("User-123");

        product.DeletedAt.Should().BeAfter(beforeCall);
    }
}

public class RatingTests
{
    [Test]
    public void Rating_CalculatesAverage_SingleRating()
    {
        var product = new Product
        {
            Ratings = new List<Rating>
            {
                new Rating { Puntuacion = 5 }
            }
        };

        product.RatingPromedio.Should().Be(5);
    }

    [Test]
    public void Rating_CalculatesAverage_AllFiveRatings()
    {
        var product = new Product
        {
            Ratings = new List<Rating>
            {
                new Rating { Puntuacion = 1 },
                new Rating { Puntuacion = 2 },
                new Rating { Puntuacion = 3 },
                new Rating { Puntuacion = 4 },
                new Rating { Puntuacion = 5 }
            }
        };

        product.RatingPromedio.Should().Be(3.0);
    }
}

public class FavoriteTests
{
    [Test]
    public void Favorite_CreatesRelationship()
    {
        var favorite = new Favorite
        {
            UsuarioId = 1,
            ProductoId = 100
        };

        favorite.UsuarioId.Should().Be(1);
        favorite.ProductoId.Should().Be(100);
    }
}

public class CarritoItemTests
{
    [Test]
    public void CarritoItem_HasPrice()
    {
        var item = new CarritoItem
        {
            Precio = 99.99m
        };

        item.Precio.Should().Be(99.99m);
    }
}

public class PurchaseTests
{
    [Test]
    public void Purchase_CalculatesTotal()
    {
        var purchase = new Purchase
        {
            Total = 299.99m
        };

        purchase.Total.Should().Be(299.99m);
    }

    [Test]
    public void Purchase_DefaultDate_IsNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var purchase = new Purchase();
        var after = DateTime.UtcNow.AddSeconds(1);

        purchase.FechaCompra.Should().BeAfter(before);
        purchase.FechaCompra.Should().BeBefore(after);
    }
}

public class UserTests
{
    [Test]
    public void User_HasDefaultValues()
    {
        var user = new User();

        user.Rol.Should().Be("USER");
        user.Deleted.Should().BeFalse();
    }
}
