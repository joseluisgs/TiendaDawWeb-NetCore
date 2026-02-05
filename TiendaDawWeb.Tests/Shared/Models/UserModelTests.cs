using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using TiendaDawWeb.Shared.Data.Abstractions;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Tests.Shared.Models;

public class AuditableEntityTests
{
    [Test]
    public void AuditableEntity_CanSetAuditFields()
    {
        var entity = new TestAuditableEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "admin",
            UpdatedBy = "user1"
        };

        entity.CreatedAt.Should().NotBe(default);
        entity.UpdatedAt.Should().NotBe(default);
        entity.CreatedBy.Should().Be("admin");
        entity.UpdatedBy.Should().Be("user1");
    }

    private class TestAuditableEntity : AuditableEntity { }
}

public class UserModelTests
{
    [Test]
    public void User_CanSetProperties()
    {
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com",
            Nombre = "Test",
            Apellidos = "User",
            Rol = "ADMIN",
            Avatar = "https://example.com/avatar.jpg",
            Deleted = false,
            CreatedBy = "system",
            UpdatedBy = "admin",
            DeletedBy = null
        };

        user.Id.Should().Be(1);
        user.UserName.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.Nombre.Should().Be("Test");
        user.Apellidos.Should().Be("User");
        user.Rol.Should().Be("ADMIN");
        user.Avatar.Should().Be("https://example.com/avatar.jpg");
        user.Deleted.Should().BeFalse();
        user.CreatedBy.Should().Be("system");
        user.UpdatedBy.Should().Be("admin");
        user.DeletedBy.Should().BeNull();
    }

    [Test]
    public void User_HasDefaultRol()
    {
        var user = new User();
        user.Rol.Should().Be("USER");
    }

    [Test]
    public void User_CanHaveNullAvatar()
    {
        var user = new User { Avatar = null };
        user.Avatar.Should().BeNull();
    }

    [Test]
    public void User_SupportsCollections()
    {
        var user = new User
        {
            Products = new List<Product>(),
            Purchases = new List<Purchase>(),
            Favorites = new List<Favorite>(),
            Ratings = new List<Rating>(),
            CarritoItems = new List<CarritoItem>()
        };

        user.Products.Should().NotBeNull();
        user.Purchases.Should().NotBeNull();
        user.Favorites.Should().NotBeNull();
        user.Ratings.Should().NotBeNull();
        user.CarritoItems.Should().NotBeNull();
    }

    [Test]
    public void User_CanSetAuditFields()
    {
        var now = DateTime.UtcNow;
        var user = new User
        {
            CreatedAt = now,
            UpdatedAt = now.AddHours(1),
            CreatedBy = "admin",
            UpdatedBy = "admin2"
        };

        user.CreatedAt.Should().Be(now);
        user.UpdatedAt.Should().Be(now.AddHours(1));
        user.CreatedBy.Should().Be("admin");
        user.UpdatedBy.Should().Be("admin2");
    }

    [Test]
    public void User_CanSetDeletedProperties()
    {
        var now = DateTime.UtcNow;
        var user = new User
        {
            Deleted = true,
            DeletedAt = now,
            DeletedBy = "admin"
        };

        user.Deleted.Should().BeTrue();
        user.DeletedAt.Should().Be(now);
        user.DeletedBy.Should().Be("admin");
    }

    [Test]
    public void User_CanHaveNullDeletedAt()
    {
        var user = new User { Deleted = false, DeletedAt = null, DeletedBy = null };
        user.DeletedAt.Should().BeNull();
        user.DeletedBy.Should().BeNull();
    }

    [Test]
    public void User_CanImplementITimestamped()
    {
        var user = new User();
        var timestamped = user as ITimestamped;
        
        timestamped.Should().NotBeNull();
    }

    [Test]
    public void User_ImplementsIdentityUser()
    {
        var user = new User();
        user.Should().BeAssignableTo<IdentityUser<long>>();
    }
}

public class RatingModelTests
{
    [Test]
    public void Rating_CanSetProperties()
    {
        var rating = new Rating
        {
            Id = 1,
            Puntuacion = 5,
            Comentario = "Great!",
            UsuarioId = 10,
            ProductoId = 100
        };

        rating.Id.Should().Be(1);
        rating.Puntuacion.Should().Be(5);
        rating.Comentario.Should().Be("Great!");
        rating.UsuarioId.Should().Be(10);
        rating.ProductoId.Should().Be(100);
    }

    [Test]
    public void Rating_DefaultPuntuacion_IsZero()
    {
        var rating = new Rating();
        rating.Puntuacion.Should().Be(0);
    }

    [Test]
    public void Rating_CanHaveNullComentario()
    {
        var rating = new Rating { Comentario = null };
        rating.Comentario.Should().BeNull();
    }
}

public class FavoriteModelTests
{
    [Test]
    public void Favorite_CreatesRelationship()
    {
        var favorite = new Favorite
        {
            Id = 1,
            UsuarioId = 5,
            ProductoId = 50
        };

        favorite.Id.Should().Be(1);
        favorite.UsuarioId.Should().Be(5);
        favorite.ProductoId.Should().Be(50);
    }
}

public class CarritoItemModelTests
{
    [Test]
    public void CarritoItem_CanSetProperties()
    {
        var item = new CarritoItem
        {
            Id = 1,
            UsuarioId = 10,
            ProductoId = 100,
            Precio = 99.99m
        };

        item.Id.Should().Be(1);
        item.UsuarioId.Should().Be(10);
        item.ProductoId.Should().Be(100);
        item.Precio.Should().Be(99.99m);
    }

    [Test]
    public void CarritoItem_CanHaveNullRowVersion()
    {
        var item = new CarritoItem();
        item.RowVersion.Should().BeNull();
    }
}

public class PurchaseModelTests
{
    [Test]
    public void Purchase_CanSetProperties()
    {
        var purchase = new Purchase
        {
            Id = 1,
            Total = 299.99m,
            CompradorId = 10,
            FechaCompra = DateTime.UtcNow
        };

        purchase.Id.Should().Be(1);
        purchase.Total.Should().Be(299.99m);
        purchase.CompradorId.Should().Be(10);
        purchase.FechaCompra.Should().NotBe(default);
    }

    [Test]
    public void Purchase_DefaultTotal_IsZero()
    {
        var purchase = new Purchase();
        purchase.Total.Should().Be(0);
    }

    [Test]
    public void Purchase_CanAddProducts()
    {
        var purchase = new Purchase
        {
            Products = new List<Product>
            {
                new Product { Id = 1, Nombre = "Product 1" },
                new Product { Id = 2, Nombre = "Product 2" }
            }
        };

        purchase.Products.Should().HaveCount(2);
    }
}

public class ProductCategoryTests
{
    [Test]
    public void ProductCategory_HasAllValues()
    {
        var categories = Enum.GetValues<ProductCategory>();
        categories.Should().Contain(ProductCategory.SMARTPHONES);
        categories.Should().Contain(ProductCategory.LAPTOPS);
        categories.Should().Contain(ProductCategory.AUDIO);
        categories.Should().Contain(ProductCategory.GAMING);
        categories.Should().Contain(ProductCategory.ACCESSORIES);
    }

    [Test]
    public void ProductCategory_Count_IsFive()
    {
        var categories = Enum.GetValues<ProductCategory>();
        categories.Length.Should().Be(5);
    }
}
