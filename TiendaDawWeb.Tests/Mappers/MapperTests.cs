using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.ViewModels;
using TiendaDawWeb.Web.Mappers;

namespace TiendaDawWeb.Tests.Mappers;

/// <summary>
/// OBJETIVO: Verificar que la transformación de datos entre capas es exacta.
/// LO QUE BUSCA: Asegurar que no se pierde información al pasar de Entidad a ViewModel y viceversa.
/// </summary>
[TestFixture]
public class MapperTests
{
    [Test]
    public void Product_ToViewModel_ShouldMapAllFields()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Nombre = "iPhone",
            Descripcion = "Test",
            Precio = 999.99m,
            Categoria = ProductCategory.SMARTPHONES,
            Imagen = "foto.jpg"
        };

        // Act
        var vm = product.ToViewModel();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(vm.Id, Is.EqualTo(product.Id));
            Assert.That(vm.Nombre, Is.EqualTo(product.Nombre));
            Assert.That(vm.Descripcion, Is.EqualTo(product.Descripcion));
            Assert.That(vm.Precio, Is.EqualTo(product.Precio));
            Assert.That(vm.Categoria, Is.EqualTo(product.Categoria));
            Assert.That(vm.ImagenUrl, Is.EqualTo(product.Imagen));
        });
    }

    [Test]
    public void ProductViewModel_ToEntity_ShouldMapCorrectly()
    {
        // Arrange
        var vm = new ProductViewModel
        {
            Nombre = "New",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.AUDIO
        };
        var userId = 10L;
        var newImageUrl = "new.jpg";

        // Act
        var entity = vm.ToEntity(userId, newImageUrl);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(entity.Nombre, Is.EqualTo(vm.Nombre));
            Assert.That(entity.PropietarioId, Is.EqualTo(userId));
            Assert.That(entity.Imagen, Is.EqualTo(newImageUrl));
            Assert.That(entity.Precio, Is.EqualTo(vm.Precio));
        });
    }

    [Test]
    public void RegisterViewModel_ToEntity_ShouldSetDefaults()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Email = "test@test.com",
            Nombre = "User",
            Apellidos = "Test"
        };

        // Act
        var user = model.ToEntity();

        // Assert
        Assert.That(user.Email, Is.EqualTo(model.Email));
        Assert.That(user.UserName, Is.EqualTo(model.Email));
        Assert.That(user.Rol, Is.EqualTo("USER"));
        Assert.That(user.Avatar, Does.Contain("robohash"));
    }
}
