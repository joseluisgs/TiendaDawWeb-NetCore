using FluentAssertions;
using TiendaDawWeb.Shared.Mappers;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Tests.Shared.Mappers;

public class UserMapperTests
{
    [Test]
    public void ToEntity_MapsAllProperties_Success()
    {
        var model = new RegisterViewModel
        {
            Nombre = "Juan",
            Apellidos = "García López",
            Email = "juan@example.com",
            Password = "password123",
            ConfirmPassword = "password123",
            Avatar = null
        };

        var result = model.ToEntity();

        result.UserName.Should().Be("juan@example.com");
        result.Email.Should().Be("juan@example.com");
        result.Nombre.Should().Be("Juan");
        result.Apellidos.Should().Be("García López");
        result.Rol.Should().Be("USER");
        result.Avatar.Should().Contain("robohash.org");
        result.Avatar.Should().Contain("juan@example.com");
    }

    [Test]
    public void ToEntity_UsesCustomAvatar_WhenProvided()
    {
        var model = new RegisterViewModel
        {
            Nombre = "María",
            Apellidos = "Pérez",
            Email = "maria@example.com",
            Password = "pass",
            ConfirmPassword = "pass",
            Avatar = "https://custom.com/avatar.jpg"
        };

        var result = model.ToEntity();

        result.Avatar.Should().Be("https://custom.com/avatar.jpg");
    }

    [Test]
    public void ToEntity_SetsDefaultAvatar_WithEmailHash()
    {
        var model = new RegisterViewModel
        {
            Nombre = "Test",
            Apellidos = "User",
            Email = "test@example.com",
            Password = "1234",
            ConfirmPassword = "1234",
            Avatar = null
        };

        var result = model.ToEntity();

        result.Avatar.Should().Be($"https://robohash.org/{model.Email}?size=150x150");
    }
}

public class RatingMapperTests
{
    [Test]
    public void ToViewModel_MapsProperties_Success()
    {
        var rating = new Rating
        {
            Id = 1,
            Puntuacion = 5,
            Comentario = "Excellent product!",
            ProductoId = 100
        };

        var result = rating.ToViewModel();

        result.Puntuacion.Should().Be(5);
        result.Comentario.Should().Be("Excellent product!");
        result.ProductoId.Should().Be(100);
    }

    [Test]
    public void ToViewModel_HandlesNullComment_Success()
    {
        var rating = new Rating
        {
            Id = 1,
            Puntuacion = 3,
            Comentario = null,
            ProductoId = 100
        };

        var result = rating.ToViewModel();

        result.Puntuacion.Should().Be(3);
        result.Comentario.Should().BeNull();
    }

    [Test]
    public void ToEntity_CreatesRating_Success()
    {
        var model = new RatingViewModel
        {
            Puntuacion = 4,
            Comentario = "Good quality",
            ProductoId = 200
        };

        var result = model.ToEntity(usuarioId: 50);

        result.Puntuacion.Should().Be(4);
        result.Comentario.Should().Be("Good quality");
        result.ProductoId.Should().Be(200);
        result.UsuarioId.Should().Be(50);
    }

    [Test]
    public void ToEntity_HandlesNullComment_Success()
    {
        var model = new RatingViewModel
        {
            Puntuacion = 5,
            Comentario = null,
            ProductoId = 100
        };

        var result = model.ToEntity(usuarioId: 1);

        result.Comentario.Should().BeNull();
        result.Puntuacion.Should().Be(5);
    }
}
