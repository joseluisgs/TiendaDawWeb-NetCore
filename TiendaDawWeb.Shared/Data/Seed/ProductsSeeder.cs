using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Shared.Data.Seed;

/// <summary>
/// Siembra el catálogo de 42 productos de ejemplo repartido por las cinco
/// categorías, con los dueños tomados de <see cref="SeedCredentials"/>.
///
/// 🎓 Idempotencia por nombre de producto: no basta con «si la tabla está
/// vacía»; si la BD tiene un producto suelto, el catálogo completo debe poder
/// reponerse. Solo se dan de alta los que falten, sin tocar los existentes.
/// </summary>
public class ProductsSeeder(
    ApplicationDbContext context,
    ILogger<ProductsSeeder> logger)
{
    /// <summary>Total del catálogo de ejemplo (para el mensaje de arranque).</summary>
    public const int Total = 42;

    /// <summary>Crea los productos del catálogo que falten.</summary>
    public async Task SeedAsync()
    {
        // Email → Id de cada usuario de prueba (el catálogo les asigna productos).
        var propietarios = await context.Users
            .Where(u => SeedCredentials.Usuarios.Select(s => s.Email).Contains(u.Email))
            .ToDictionaryAsync(u => u.Email!, u => u.Id);

        long Id(string email) => propietarios[email];

        var catalogo = Catalogo(Id);
        var existentes = await context.Products.Select(p => p.Nombre).ToListAsync();
        var faltantes = catalogo.Where(p => !existentes.Contains(p.Nombre)).ToList();

        if (faltantes.Count == 0)
        {
            logger.LogInformation("Catálogo completo ({Total} productos): nada que sembrar", Total);
            return;
        }

        await context.Products.AddRangeAsync(faltantes);
        await context.SaveChangesAsync();
        logger.LogInformation("Productos creados: {Creados} de {Total}",
            faltantes.Count, Total);
    }

    /// <summary>
    /// El catálogo completo, en el mismo orden que la versión previa:
    /// SMARTPHONES (10), LAPTOPS (8), AUDIO (8), GAMING (8), ACCESSORIES (8).
    /// </summary>
    private static List<Product> Catalogo(Func<string, long> Id)
    {
        var admin = Id(SeedCredentials.AdminEmail);
        var usuario = Id(SeedCredentials.PruebaEmail);
        var moderador = Id("moderador@waladaw.com");
        var usuario2 = Id("otro@otro.com");
        var maria = Id("maria@email.com");
        var carlos = Id("carlos@email.com");
        var ana = Id("ana@email.com");
        var david = Id("david@email.com");
        var laura = Id("laura@email.com");
        var javier = Id("javier@email.com");

        return
        [
            // SMARTPHONES - 10 productos
            new()
            {
                Nombre = "iPhone 17 Pro Max",
                Descripcion = "El iPhone más avanzado de Apple con chip A17 Pro, titanio aeroespacial y cámara de 48MP. Estado impecable, apenas usado.",
                Precio = 1199.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = usuario,
                Imagen = "https://medias.lapostemobile.fr/fiche_mobile/layer/9724_Layer_2.png"
            },
            new()
            {
                Nombre = "Samsung Galaxy S24 Ultra",
                Descripcion = "Flagship de Samsung con S Pen integrado, pantalla Dynamic AMOLED 2X y cámara de 200MP. Como nuevo, con todos los accesorios.",
                Precio = 1099.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = usuario,
                Imagen = "https://cdn.grupoelcorteingles.es/SGFM/dctm/MEDIA03/202404/11/00157063608129009_22__1200x1200.jpg"
            },
            new()
            {
                Nombre = "Google Pixel 8 Pro",
                Descripcion = "El mejor teléfono para fotografía con IA de Google Tensor G3. Pantalla LTPO de 120Hz. Excelente estado de conservación.",
                Precio = 899.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = usuario2,
                Imagen = "https://http2.mlstatic.com/D_NQ_NP_802433-MLU78081005713_072024-O.webp"
            },
            new()
            {
                Nombre = "iPhone 15 Pro",
                Descripcion = "iPhone 15 Pro con titanio natural, cámara principal de 48MP y zoom óptico 3x. Perfecto estado, sin rayones.",
                Precio = 999.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = maria,
                Imagen = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=400"
            },
            new()
            {
                Nombre = "OnePlus 12",
                Descripcion = "Flagship killer con Snapdragon 8 Gen 3, carga rápida 100W y pantalla AMOLED 120Hz. Como recién salido de caja.",
                Precio = 749.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = carlos,
                Imagen = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400"
            },
            new()
            {
                Nombre = "Xiaomi 14 Ultra",
                Descripcion = "Cámara Leica profesional, Snapdragon 8 Gen 3 y diseño premium. Ideal para fotografía móvil avanzada.",
                Precio = 899.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = ana,
                Imagen = "https://images.unsplash.com/photo-1592899677977-9c10ca588bbd?w=400"
            },
            new()
            {
                Nombre = "iPhone 14",
                Descripcion = "iPhone 14 en azul, 128GB. Batería excelente, pantalla perfecta. Incluye cargador y funda original.",
                Precio = 699.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = david,
                Imagen = "https://images.unsplash.com/photo-1678685888221-cda773a3dcdb?w=400"
            },
            new()
            {
                Nombre = "Nothing Phone 2",
                Descripcion = "Diseño transparente único con Glyph Interface. Android puro y rendimiento excepcional.",
                Precio = 599.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = laura,
                Imagen = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=400"
            },
            new()
            {
                Nombre = "Samsung Galaxy Z Flip 5",
                Descripcion = "Plegable compacto con pantalla externa mejorada. Perfecto para quienes buscan innovación.",
                Precio = 999.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = javier,
                Imagen = "https://images.unsplash.com/photo-1567721913486-6585f069b332?w=400"
            },
            new()
            {
                Nombre = "Google Pixel 7a",
                Descripcion = "El mejor Pixel en relación calidad-precio. Cámara excepcional y Android puro garantizado.",
                Precio = 449.0m,
                Categoria = ProductCategory.SMARTPHONES,
                PropietarioId = moderador,
                Imagen = "https://images.unsplash.com/photo-1598300042247-d088f8ab3a91?w=400"
            },

            // LAPTOPS - 8 productos
            new()
            {
                Nombre = "MacBook Pro M3",
                Descripcion = "MacBook Pro 14 con chip M3 Max, 36GB RAM, 1TB SSD. Perfecto para profesionales creativos. Garantía Apple vigente.",
                Precio = 1999.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = admin,
                Imagen = "https://www.notebookcheck.org/fileadmin/Notebooks/Apple/MacBook_Pro_14_2023_M3_Max/IMG_1008.JPG"
            },
            new()
            {
                Nombre = "MacBook Air M2",
                Descripcion = "MacBook Air M2 de 13 pulgadas, 16GB RAM, 512GB SSD. Ultra portátil y silencioso. Perfecto para estudiantes.",
                Precio = 1299.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = usuario,
                Imagen = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=400"
            },
            new()
            {
                Nombre = "Dell XPS 13",
                Descripcion = "Ultrabook premium con Intel i7, 16GB RAM, pantalla 4K táctil. Ideal para productividad y diseño.",
                Precio = 1149.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = maria,
                Imagen = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400"
            },
            new()
            {
                Nombre = "ASUS ROG Strix",
                Descripcion = "Laptop gaming con RTX 4070, Intel i7-13700H, 32GB RAM. Perfecto para gaming y streaming profesional.",
                Precio = 1799.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = carlos,
                Imagen = "https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=400"
            },
            new()
            {
                Nombre = "ThinkPad X1 Carbon",
                Descripcion = "Laptop empresarial premium, ultra liviano, teclado excepcional. Ideal para profesionales y ejecutivos.",
                Precio = 1599.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = ana,
                Imagen = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=400"
            },
            new()
            {
                Nombre = "HP Spectre x360",
                Descripcion = "Convertible 2-en-1 con pantalla táctil OLED, Intel i7 y diseño premium. Versatilidad máxima.",
                Precio = 1249.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = david,
                Imagen = "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?w=400"
            },
            new()
            {
                Nombre = "Surface Laptop 5",
                Descripcion = "Microsoft Surface con procesador Intel de 12ª gen, pantalla PixelSense táctil. Elegancia y rendimiento.",
                Precio = 1399.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = laura,
                Imagen = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400"
            },
            new()
            {
                Nombre = "Framework Laptop",
                Descripcion = "Laptop modular y reparable, diseño sostenible. Perfecto para desarrolladores conscientes.",
                Precio = 999.0m,
                Categoria = ProductCategory.LAPTOPS,
                PropietarioId = javier,
                Imagen = "https://images.unsplash.com/photo-1484788984921-03950022c9ef?w=400"
            },

            // AUDIO - 8 productos
            new()
            {
                Nombre = "AirPods Pro 2ª Gen",
                Descripcion = "Auriculares con cancelación de ruido adaptativa y audio espacial personalizado. Nuevos en caja sellada.",
                Precio = 249.0m,
                Categoria = ProductCategory.AUDIO,
                PropietarioId = moderador,
                Imagen = "https://cdsassets.apple.com/live/SZLF0YNV/images/sp/111851_sp880-airpods-Pro-2nd-gen.png"
            },
            new()
            {
                Nombre = "Sony WH-1000XM5",
                Descripcion = "Auriculares noise-cancelling líderes del mercado. Sonido excepcional y comodidad todo el día.",
                Precio = 299.0m,
                Categoria = ProductCategory.AUDIO,
                PropietarioId = usuario,
                Imagen = "https://images.unsplash.com/photo-1583394838336-acd977736f90?w=400"
            },
            new()
            {
                Nombre = "Bose QuietComfort",
                Descripcion = "Cancelación de ruido premium de Bose. Perfectos para viajes largos y trabajo concentrado.",
                Precio = 249.0m,
                Categoria = ProductCategory.AUDIO,
                PropietarioId = maria,
                Imagen = "https://images.unsplash.com/photo-1545127398-14699f92334b?w=400"
            },
            new()
            {
                Nombre = "Marshall Acton III",
                Descripcion = "Altavoz Bluetooth vintage con sonido Marshall icónico. Perfecto para ambientar cualquier espacio.",
                Precio = 199.0m,
                Categoria = ProductCategory.AUDIO,
                PropietarioId = carlos,
                Imagen = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400"
            },
            new()
            {
                Nombre = "JBL Flip 6",
                Descripcion = "Altavoz portátil resistente al agua, sonido potente y batería de 12 horas. Ideal para exteriores.",
                Precio = 89.0m,
                Categoria = ProductCategory.AUDIO,
                PropietarioId = ana,
                Imagen = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400"
            },
            new()
            {
                Nombre = "Sennheiser HD 660S",
                Descripcion = "Auriculares audiófilo de referencia con drivers dinámicos mejorados. Para los más exigentes.",
                Precio = 399.0m,
                Categoria = ProductCategory.AUDIO,
                PropietarioId = david,
                Imagen = "https://images.unsplash.com/photo-1484704849700-f032a568e944?w=400"
            },
            new()
            {
                Nombre = "Jabra Elite 85h",
                Descripcion = "Auriculares wireless con cancelación de ruido inteligente y 36h de batería. Perfectos para oficina.",
                Precio = 179.0m,
                Categoria = ProductCategory.AUDIO,
                PropietarioId = laura,
                Imagen = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=400"
            },
            new()
            {
                Nombre = "Audio-Technica ATH-M50x",
                Descripcion = "Auriculares de estudio profesional con sonido neutro y construcción robusta. Clásico atemporal.",
                Precio = 149.0m,
                Categoria = ProductCategory.AUDIO,
                PropietarioId = javier,
                Imagen = "https://images.unsplash.com/photo-1487215078519-e21cc028cb29?w=400"
            },

            // GAMING - 8 productos
            new()
            {
                Nombre = "Steam Deck OLED",
                Descripcion = "Consola portátil con pantalla OLED HDR de 7.4 pulgadas. Modelo de 512GB. Gaming en cualquier lugar. Como nuevo.",
                Precio = 549.0m,
                Categoria = ProductCategory.GAMING,
                PropietarioId = usuario2,
                Imagen = "https://i.blogs.es/420d82/steam-deck-oled-portada/1366_521.jpeg"
            },
            new()
            {
                Nombre = "PlayStation 5",
                Descripcion = "PS5 en perfecto estado con un mando adicional. Incluye 3 juegos digitales. Como nueva.",
                Precio = 499.0m,
                Categoria = ProductCategory.GAMING,
                PropietarioId = usuario,
                Imagen = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=400"
            },
            new()
            {
                Nombre = "Xbox Series X",
                Descripcion = "Consola next-gen con 1TB, Game Pass incluido por 3 meses. Potencia 4K para gaming hardcore.",
                Precio = 459.0m,
                Categoria = ProductCategory.GAMING,
                PropietarioId = maria,
                Imagen = "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?w=400"
            },
            new()
            {
                Nombre = "Nintendo Switch OLED",
                Descripcion = "Switch OLED con pantalla mejorada, incluye Pro Controller y 5 juegos físicos. Diversión garantizada.",
                Precio = 329.0m,
                Categoria = ProductCategory.GAMING,
                PropietarioId = carlos,
                Imagen = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400"
            },
            new()
            {
                Nombre = "Logitech G Pro X",
                Descripcion = "Auriculares gaming profesionales con micrófono Blue VO!CE. Usados en esports profesional.",
                Precio = 159.0m,
                Categoria = ProductCategory.GAMING,
                PropietarioId = ana,
                Imagen = "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=400"
            },
            new()
            {
                Nombre = "Razer DeathAdder V3",
                Descripcion = "Ratón gaming ergonómico con sensor Focus Pro 30K. Precisión profesional para competir.",
                Precio = 69.0m,
                Categoria = ProductCategory.GAMING,
                PropietarioId = david,
                Imagen = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400"
            },
            new()
            {
                Nombre = "ROG Ally",
                Descripcion = "Handheld PC gaming con Windows 11, juega tu biblioteca Steam en cualquier lugar.",
                Precio = 699.0m,
                Categoria = ProductCategory.GAMING,
                PropietarioId = laura,
                Imagen = "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=400"
            },
            new()
            {
                Nombre = "Valve Index VR",
                Descripcion = "Sistema VR premium con tracking de dedos y audio off-ear. La mejor experiencia de realidad virtual.",
                Precio = 999.0m,
                Categoria = ProductCategory.GAMING,
                PropietarioId = javier,
                Imagen = "https://images.unsplash.com/photo-1622979135225-d2ba269cf1ac?w=400"
            },

            // ACCESSORIES - 8 productos
            new()
            {
                Nombre = "Apple Watch Series 9",
                Descripcion = "Smartwatch más avanzado de Apple, GPS + Cellular, correa deportiva. Salud y fitness al máximo.",
                Precio = 399.0m,
                Categoria = ProductCategory.ACCESSORIES,
                PropietarioId = usuario,
                Imagen = "https://images.unsplash.com/photo-1434493789847-2f02dc6ca35d?w=400"
            },
            new()
            {
                Nombre = "iPad Pro 12.9",
                Descripcion = "iPad Pro con M2, Apple Pencil incluido. Perfecto para diseño digital y productividad profesional.",
                Precio = 1099.0m,
                Categoria = ProductCategory.ACCESSORIES,
                PropietarioId = moderador,
                Imagen = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=400"
            },
            new()
            {
                Nombre = "MagSafe Charger Pack",
                Descripcion = "Cargador inalámbrico MagSafe original + soporte + cable USB-C. Carga rápida garantizada.",
                Precio = 79.0m,
                Categoria = ProductCategory.ACCESSORIES,
                PropietarioId = maria,
                Imagen = "https://images.unsplash.com/photo-1588423771073-b8903fbb85b5?w=400"
            },
            new()
            {
                Nombre = "Anker PowerCore",
                Descripcion = "Batería portátil 20.000mAh con carga rápida PD. Ideal para viajes y emergencias energéticas.",
                Precio = 45.0m,
                Categoria = ProductCategory.ACCESSORIES,
                PropietarioId = carlos,
                Imagen = "https://images.unsplash.com/photo-1609091839311-d5365f9ff1c5?w=400"
            },
            new()
            {
                Nombre = "Samsung Galaxy Watch",
                Descripcion = "Smartwatch Android con GPS, monitor cardíaco y resistencia al agua. Perfecto para fitness.",
                Precio = 289.0m,
                Categoria = ProductCategory.ACCESSORIES,
                PropietarioId = ana,
                Imagen = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400"
            },
            new()
            {
                Nombre = "Mechanical Keyboard",
                Descripcion = "Teclado mecánico RGB con switches Cherry MX Blue. Perfecto para programadores y gamers.",
                Precio = 149.0m,
                Categoria = ProductCategory.ACCESSORIES,
                PropietarioId = david,
                Imagen = "https://images.unsplash.com/photo-1541140532154-b024d705b90a?w=400"
            },
            new()
            {
                Nombre = "4K Webcam",
                Descripcion = "Cámara web 4K con autoenfoque, ideal para streaming y videoconferencias profesionales.",
                Precio = 89.0m,
                Categoria = ProductCategory.ACCESSORIES,
                PropietarioId = laura,
                Imagen = "https://images.unsplash.com/photo-1587825140708-dfaf72ae4b04?w=400"
            },
            new()
            {
                Nombre = "Wireless Charger Stand",
                Descripcion = "Soporte cargador inalámbrico con ventilador de refrigeración. Compatible con todos los estándares Qi.",
                Precio = 39.0m,
                Categoria = ProductCategory.ACCESSORIES,
                PropietarioId = javier,
                Imagen = "https://images.unsplash.com/photo-1583394838336-acd977736f90?w=400"
            }
        ];
    }
}
