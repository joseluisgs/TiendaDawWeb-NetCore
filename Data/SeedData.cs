using Microsoft.AspNetCore.Identity;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;

namespace TiendaDawWeb.Data;

/// <summary>
/// Clase para inicializar datos de ejemplo en la base de datos
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        // Crear usuarios (idénticos al proyecto Java)
        var usuarios = new List<(User user, string password)>
        {
            (new User
            {
                Nombre = "Admin",
                Apellidos = "Administrador",
                Email = "admin@waladaw.com",
                UserName = "admin@waladaw.com",
                Rol = "ADMIN",
                Avatar = "https://robohash.org/admin?size=200x200&bgset=bg1",
                EmailConfirmed = true
            }, "admin"),
            
            (new User
            {
                Nombre = "Prueba",
                Apellidos = "Probando Mucho",
                Email = "prueba@prueba.com",
                UserName = "prueba@prueba.com",
                Rol = "USER",
                Avatar = "https://robohash.org/prueba?size=200x200&bgset=bg2",
                EmailConfirmed = true
            }, "user123"),
            
            (new User
            {
                Nombre = "Moderador",
                Apellidos = "User",
                Email = "moderador@waladaw.com",
                UserName = "moderador@waladaw.com",
                Rol = "MODERATOR",
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=moderador",
                EmailConfirmed = true
            }, "user123")
        };

        foreach (var (usuario, password) in usuarios)
        {
            if (await userManager.FindByEmailAsync(usuario.Email!) == null)
            {
                await userManager.CreateAsync(usuario, password);
            }
        }

        // Crear productos de ejemplo (idénticos al proyecto Java)
        if (!context.Products.Any())
        {
            var admin = await userManager.FindByEmailAsync("admin@waladaw.com");
            if (admin != null)
            {
                var productos = new List<Product>
                {
                    new()
                    {
                        Nombre = "iPhone 15 Pro Max",
                        Descripcion = "Último modelo de Apple con chip A17 Pro y pantalla Super Retina XDR de 6.7 pulgadas",
                        Precio = 1299.99m,
                        Categoria = ProductCategory.SMARTPHONES,
                        PropietarioId = admin.Id,
                        Imagen = "/images/iphone15.jpg"
                    },
                    new()
                    {
                        Nombre = "Samsung Galaxy S24 Ultra",
                        Descripcion = "Flagship de Samsung con S Pen integrado y cámara de 200MP",
                        Precio = 1199.99m,
                        Categoria = ProductCategory.SMARTPHONES,
                        PropietarioId = admin.Id,
                        Imagen = "/images/samsung-s24.jpg"
                    },
                    new()
                    {
                        Nombre = "Google Pixel 8 Pro",
                        Descripcion = "Lo mejor de Google AI en un smartphone con cámara profesional",
                        Precio = 999.99m,
                        Categoria = ProductCategory.SMARTPHONES,
                        PropietarioId = admin.Id,
                        Imagen = "/images/pixel8.jpg"
                    },
                    new()
                    {
                        Nombre = "MacBook Pro M3",
                        Descripcion = "Potencia profesional con chip M3, pantalla Liquid Retina XDR de 14 pulgadas",
                        Precio = 2299.99m,
                        Categoria = ProductCategory.LAPTOPS,
                        PropietarioId = admin.Id,
                        Imagen = "/images/macbook-m3.jpg"
                    },
                    new()
                    {
                        Nombre = "AirPods Pro 2ª Generación",
                        Descripcion = "Cancelación de ruido activa mejorada y audio espacial personalizado",
                        Precio = 249.99m,
                        Categoria = ProductCategory.AUDIO,
                        PropietarioId = admin.Id,
                        Imagen = "/images/airpods-pro.jpg"
                    },
                    new()
                    {
                        Nombre = "Steam Deck OLED",
                        Descripcion = "Consola portátil con pantalla OLED de 7.4 pulgadas y batería mejorada",
                        Precio = 549.99m,
                        Categoria = ProductCategory.GAMING,
                        PropietarioId = admin.Id,
                        Imagen = "/images/steam-deck.jpg"
                    }
                };

                await context.Products.AddRangeAsync(productos);
                await context.SaveChangesAsync();
            }
        }
    }
}
