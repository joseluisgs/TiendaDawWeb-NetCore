# WalaDaw 🛒

![logo](./logo.svg)

[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10-blue)](https://dotnet.microsoft.com/en-us/apps/aspnet)
[![C#](https://img.shields.io/badge/C%23-14-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10-blue)](https://docs.microsoft.com/en-us/ef/core/)
[![Razor](https://img.shields.io/badge/Razor-purple)](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/razor)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Ejemplo didáctico de web dinámicas con .NET 10 y ASP.NET Core MVC.**

Una aplicación web de comercio electrónico de segunda mano con características avanzadas de seguridad, Railway Oriented
Programming y gestión de usuarios con ASP.NET Core Identity.

## 🎯 Descripción

WalaDaw es un marketplace moderno desarrollado con .NET 10 que permite a los usuarios:

- Comprar y vender productos de segunda mano
- Gestionar perfiles de usuario con avatares
- Sistema de valoraciones y comentarios
- Panel de administración completo
- Subida de archivos e imágenes

## 📑 Tabla de Contenidos

- [WalaDaw 🛒](#waladaw-)
  - [🎯 Descripción](#-descripción)
  - [📑 Tabla de Contenidos](#-tabla-de-contenidos)
  - [✨ Características](#-características)
    - [Funcionalidades Principales](#funcionalidades-principales)
    - [Productos 2024-2025](#productos-2024-2025)
  - [🚀 Tecnologías](#-tecnologías)
  - [🏃‍♂️ Inicio Rápido](#️-inicio-rápido)
    - [Desarrollo Local](#desarrollo-local)
    - [Build y Tests](#build-y-tests)
  - [⚒️ Diagrama](#️-diagrama)
  - [📂 Estructura del Proyecto](#-estructura-del-proyecto)
  - [🏗️ Arquitectura](#️-arquitectura)
    - [Railway Oriented Programming (ROP)](#railway-oriented-programming-rop)
    - [ASP.NET Core Identity](#aspnet-core-identity)
  - [👥 Usuarios Demo](#-usuarios-demo)
  - [🔒 Seguridad](#-seguridad)
  - [🌐 Características](#-características-1)
    - [Para Usuarios](#para-usuarios)
    - [Para Administradores](#para-administradores)
  - [� Documentación](#-documentación)
  - [📝 Licencia](#-licencia)
  - [👨‍💻 Autor](#-autor)
    - [Contacto](#contacto)
  - [Licencia de uso](#licencia-de-uso)


## ✨ Características

### Funcionalidades Principales

- 🛍️ **Marketplace de Segunda Mano**: Compra y vende productos usados
- 🔐 **Sistema de Roles**: ADMIN, USER, MODERATOR con permisos diferenciados
- 📧 **Notificaciones por Email**: Confirmación automática asíncrona de compras con templates HTML
- 📊 **Panel de Administración Completo**: Dashboard con estadísticas en tiempo real, gestión de usuarios, productos y
  compras
- 🔍 **Búsqueda Avanzada**: Filtros por nombre, categoría y precio
- 🖼️ **Gestión de Imágenes**: Subida, validación y redimensionado automático con ImageSharp
- 📱 **Responsive Design**: Bootstrap 5.3 optimizado para todos los dispositivos
- 📄 **Generación de PDFs**: Facturas automáticas con iText7 y diseño profesional
- ❤️ **Sistema de Favoritos**: Gestión en tiempo real
- ⭐ **Valoraciones y Ratings**: Sistema completo de reviews con estrellas interactivas
- 🛒 **Carrito de Compras**: Control de concurrencia con transacciones SERIALIZABLE
- 🛡️ **Seguridad CSRF**: Protección completa contra ataques Cross-Site Request Forgery
- 👤 **Gestión de Perfil**: Edición de perfil con avatar y cambio de contraseña
- 🔄 **Control de Concurrencia**: Manejo de race conditions con Optimistic Concurrency Control

### Productos 2024-2025

La aplicación incluye productos actuales y relevantes:

- 📱 **Smartphones**: iPhone 15 Pro Max, Samsung Galaxy S24 Ultra, Google Pixel 8 Pro
- 💻 **Laptops**: MacBook Pro M3
- 🎧 **Audio**: AirPods Pro 2ª Generación
- 🎮 **Gaming**: Steam Deck OLED

## 🚀 Tecnologías

- **.NET 10 con C# 14** - Plataforma principal
- **ASP.NET Core MVC** - Framework web con patrón MVC
- **Razor Views** - Motor de vistas del lado servidor
- **Entity Framework Core InMemory** - ORM con base de datos en memoria
- **ASP.NET Core Identity** - Sistema completo de autenticación y autorización
- **CSharpFunctionalExtensions** - Railway Oriented Programming (ROP)
- **Bootstrap 5.3** - Framework CSS responsive
- **Bootstrap Icons** - Iconografía moderna
- **Localization (I18n/L10n)** - Soporte multilenguaje

## 🏃‍♂️ Inicio Rápido

### Desarrollo Local

```bash
# Clonar repositorio
git clone https://github.com/joseluisgs/TiendaDawWeb-NetCore.git
cd TiendaDawWeb-NetCore

# Restaurar dependencias
dotnet restore

# Ejecutar aplicación
dotnet run

# Acceder a la aplicación
http://localhost:5000
```

### Build y Tests

```bash
# Compilar proyecto
dotnet build

# Ejecutar en modo watch (desarrollo)
dotnet watch run

# Limpiar build
dotnet clean
```

## ⚒️ Diagrama

```mermaid
classDiagram
  direction TB

%% ENUMS
  class ProductCategory {
    <<enumeration>>
    SMARTPHONES
    LAPTOPS
    AUDIO
    GAMING
    ACCESSORIES
  }

  class UserRole {
    <<enumeration>>
    USER
    ADMIN
    MODERATOR
  }

%% CLASES PRINCIPALES

  class User {
    +long Id
    +string Nombre
    +string Apellidos
    +string Email
    +string Rol
    +string? Avatar
    +DateTime FechaAlta
    +bool Deleted
    +DateTime? DeletedAt
    +string? DeletedBy
  }

  class Product {
    +long Id
    +string Nombre
    +string Descripcion
    +decimal Precio
    +string? Imagen
    +ProductCategory Categoria
    +bool Reservado
    +bool Deleted
    +DateTime? DeletedAt
    +string? DeletedBy
    +DateTime CreatedAt
    +double RatingPromedio
    +string ImagenOrDefault
  }

  class Purchase {
    +long Id
    +DateTime FechaCompra
    +decimal Total
  }

  class CarritoItem {
    +long Id
    +DateTime CreatedAt
    +decimal Precio
    +byte[]? RowVersion
  }

  class Favorite {
    +long Id
    +DateTime CreatedAt
  }

  class Rating {
    +long Id
    +int Puntuacion
    +string? Comentario
    +DateTime CreatedAt
  }

%% RELACIONES

  User "1" -- "*" Product : Propietario
  User "1" -- "*" Purchase : Purchases
  User "1" -- "*" CarritoItem : CarritoItems
  User "1" -- "*" Favorite : Favorites
  User "1" -- "*" Rating : Ratings
  User "1" -- "1" UserRole : Rol

  Product "*" -- "1" ProductCategory : Categoria
  Product "1" -- "0..1" Purchase : Compra
  Product "1" -- "*" CarritoItem : CarritoItems
  Product "1" -- "*" Favorite : Favoritos
  Product "1" -- "*" Rating : Valoraciones

  Purchase "1" -- "*" Product : Products

  Favorite "*" -- "1" User : Usuario
  Favorite "*" -- "1" Product : Producto

  Rating "*" -- "1" User : Usuario
  Rating "*" -- "1" Product : Producto

  CarritoItem "*" -- "1" User : Usuario
  CarritoItem "*" -- "1" Product : Producto
```

## 📂 Estructura del Proyecto

```
TiendaDawWeb-NetCore/
├── Program.cs
│   # Punto de entrada. Configura servicios y la app web (host, middlewares, rutas, etc).
├── TiendaDawWeb.csproj
│   # Archivo de proyecto y dependencias NuGet.
├── appsettings.json
│   # Configuración de cadena de conexión, opciones de la app, etc.
│
├── Data/
│   ├── ApplicationDbContext.cs      # DbContext de Entity Framework, define DbSets/relaciones.
│   └── SeedData.cs                  # Opcional: inicialización de datos de ejemplo/pruebas.
│
├── Models/
│   ├── Enums/
│   │   ├── ProductCategory.cs       # Enum de categorías de producto.
│   │   └── UserRole.cs              # Enum de roles de usuario, si lo usas así.
│   ├── User.cs                      # Entidad usuario con Identity (tiene Products, Purchases, etc.)
│   ├── Product.cs                   # Entidad principal producto.
│   ├── Purchase.cs                  # Compra (1 usuario, muchos productos)
│   ├── Favorite.cs                  # Relación Favorite (usuario <-> producto)
│   ├── Rating.cs                    # Valoración sobre producto.
│   └── CarritoItem.cs               # Número de producto en carrito (sin cantidad).
│
├── Services/                        # Lógica de negocio centralizada.
│   ├── Interfaces/
│   │   ├── IProductService.cs
│   │   ├── IFavoriteService.cs
│   │   ├── IRatingService.cs
│   │   └── ...                      # Interfaces para inversión de dependencias.
│   └── Implementations/
│       ├── ProductService.cs
│       ├── FavoriteService.cs
│       ├── RatingService.cs
│       └── ...                      # Implementación real de la lógica.
│
├── Controllers/
│   ├── HomeController.cs            # Inicio y páginas generales.
│   ├── AuthController.cs            # Registro/inicio de sesión/cierre sesión.
│   ├── ProductController.cs         # Listado, detalle, crear, editar, eliminar producto.
│   ├── FavoriteController.cs        # Añadir/quitar/listar favoritos.
│   ├── CarritoController.cs         # Añadir/quitar/cargar el carrito.
│   ├── PurchaseController.cs        # Comprar, ver historial y detalle de compras.
│   ├── RatingController.cs          # Añadir/ver valoraciones vía AJAX/API.
│   ├── AdminController.cs           # Panel de admin.
│   └── ProfileController.cs         # Detalle, edición y seguridad de perfil usuario.
│
├── ViewModels/
│   ├── ProductViewModel.cs          # Datos compuestos para vistas de producto.
│   ├── UserViewModel.cs             # Datos compuestos para vistas de usuario.
│   ├── PurchaseViewModel.cs         # Para vistas de compras.
│   ├── RatingViewModel.cs           # Valoraciones (si no usas entidades directas).
│   ├── CarritoItemViewModel.cs      # Visualización del carrito.
│   ├── LoginViewModel.cs            # Login.
│   ├── RegisterViewModel.cs         # Registro.
│   └── ...                          # Otros, según necesidades de formularios/vistas.
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml           # Layout principal de la web.
│   │   ├── _LoginPartial.cshtml     # Login/logout parcial menú.
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml             # Página general de error.
│   ├── Home/
│   │   ├── Index.cshtml             # Home (landing).
│   │   └── About.cshtml             # Acerca de, ayuda, etc.
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   ├── ForgotPassword.cshtml
│   │   └── ResetPassword.cshtml
│   ├── Product/
│   │   ├── Index.cshtml             # Listado de productos.
│   │   ├── Details.cshtml           # Ficha de producto.
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   ├── Favorite/
│   │   └── Index.cshtml             # Listado de favoritos.
│   ├── Carrito/
│   │   ├── Index.cshtml             # Carrito de usuario.
│   │   ├── Checkout.cshtml          # Confirmar compra.
│   ├── Purchase/
│   │   ├── Index.cshtml             # Historial de compras.
│   │   ├── Details.cshtml           # Detalle de compra.
│   ├── Profile/
│   │   ├── Index.cshtml             # Mi perfil.
│   │   ├── Edit.cshtml              # Editar datos.
│   │   ├── ChangePassword.cshtml    # Cambiar contraseña.
│   └── Admin/
│       ├── Index.cshtml             # Dashboard.
│       ├── Usuarios.cshtml          # Administración de usuarios.
│       ├── Productos.cshtml         # Administración de productos.
│       ├── Compras.cshtml           # Administración de compras.
│       ├── Estadisticas.cshtml      # Estadísticas, gráficas, etc.
│       └── Logs.cshtml              # Logs del sistema (opcional).
│
├── Errors/
│   └── ErrorViewModel.cs            # ViewModel de errores.
│
└── wwwroot/
    ├── css/
    │   ├── site.css
    │   └── styles.css
    ├── js/
    │   ├── ratings.js               # Valoraciones AJAX.
    │   ├── favorites.js             # Lógica de favoritos AJAX.
    │   ├── carrito.js               # Carrito AJAX.
    │   └── ...                      # Otros scripts propios.
    └── images/
        └── default-product.jpg      # Imagen por defecto, otros media.
```

## 🏗️ Arquitectura

### Railway Oriented Programming (ROP)

El proyecto implementa el patrón ROP usando `CSharpFunctionalExtensions`:

```csharp
public async Task<Result<Product, DomainError>> GetByIdAsync(long id)
{
    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    
    return product != null
        ? Result.Success<Product, DomainError>(product)
        : Result.Failure<Product, DomainError>(ProductError.NotFound(id));
}
```



### ASP.NET Core Identity

Sistema completo de autenticación y autorización:

- Roles personalizados (ADMIN, USER, MODERATOR)
- Password hashing seguro
- Cookie authentication
- Claims-based authorization

## 👥 Usuarios Demo

| Usuario   | Email                 | Password | Rol       |
|-----------|-----------------------|----------|-----------|
| Admin     | admin@waladaw.com     | admin    | ADMIN     |
| Prueba    | prueba@prueba.com     | user123  | USER      |
| Moderador | moderador@waladaw.com | user123  | MODERATOR |

## 🔒 Seguridad

- ✅ Autenticación basada en ASP.NET Core Identity
- ✅ Autorización por roles ([Authorize(Roles = "ADMIN")])
- ✅ Protección CSRF con Anti-Forgery Tokens
- ✅ Validación de subida de archivos (tipo y tamaño)
- ✅ Sanitización de nombres de archivo
- ✅ Control de concurrencia optimista (RowVersion)
- ✅ Transacciones SERIALIZABLE para carrito/compras
- ✅ Soft delete para usuarios y productos
- ✅ Password hashing seguro (Identity)
- ✅ Validación de propiedad de recursos
- ✅ Nullable reference types habilitadas
- ✅ TreatWarningsAsErrors activo

## 🌐 Características

### Para Usuarios

- ✅ Registro y login seguro
- ✅ Perfil con avatar personalizable
- ✅ Publicar productos con imágenes
- ✅ Editar y eliminar productos propios
- ✅ Sistema de valoraciones con estrellas
- ✅ Gestión de favoritos en tiempo real
- ✅ Carrito de compras con control de concurrencia
- ✅ Proceso de checkout completo
- ✅ Historial de compras
- ✅ Descarga de facturas en PDF
- ✅ Búsqueda avanzada con filtros
- ✅ Localización con I18n y L10n

### Para Administradores

- ✅ Panel de control completo (`/admin`)
- ✅ Dashboard con estadísticas en tiempo real
- ✅ Gestión de usuarios (ver, editar roles, eliminar)
- ✅ Gestión de productos (ver, filtrar, eliminar)
- ✅ Historial de todas las compras
- ✅ Estadísticas avanzadas:
    - Categorías más vendidas
    - Top 10 compradores
    - Top 10 vendedores
    - Ventas por mes (últimos 12 meses)
- ✅ Filtros por fecha y categoría



## 📚 Documentación

- Repositorio de apuntes de
  curso: [Desarrollo Web en Entornos Servidor - 05 Desarrollo de páginas web dinámicas .NET](https://github.com/joseluisgs/DesarrolloWebEntornosServidor-05-2025-2026)

## 📝 Licencia

Este proyecto es un ejemplo educativo con fines didácticos.

## 👨‍💻 Autor

Codificado con :sparkling_heart: por [José Luis González Sánchez](https://twitter.com/JoseLuisGS_)

[![Twitter](https://img.shields.io/twitter/follow/JoseLuisGS_?style=social)](https://twitter.com/JoseLuisGS_)
[![GitHub](https://img.shields.io/github/followers/joseluisgs?style=social)](https://github.com/joseluisgs)
[![GitHub](https://img.shields.io/github/stars/joseluisgs?style=social)](https://github.com/joseluisgs)

### Contacto

<p>
  Cualquier cosa que necesites házmelo saber por si puedo ayudarte 💬.
</p>
<p>
 <a href="https://joseluisgs.dev" target="_blank">
        <img src="https://joseluisgs.github.io/img/favicon.png" 
    height="30">
    </a>  &nbsp;&nbsp;
    <a href="https://github.com/joseluisgs" target="_blank">
        <img src="https://distreau.com/github.svg" 
    height="30">
    </a> &nbsp;&nbsp;
        <a href="https://twitter.com/JoseLuisGS_" target="_blank">
        <img src="https://i.imgur.com/U4Uiaef.png" 
    height="30">
    </a> &nbsp;&nbsp;
    <a href="https://www.linkedin.com/in/joseluisgonsan" target="_blank">
        <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/c/ca/LinkedIn_logo_initials.png/768px-LinkedIn_logo_initials.png" 
    height="30">
    </a>  &nbsp;&nbsp;
    <a href="https://g.dev/joseluisgs" target="_blank">
        <img loading="lazy" src="https://googlediscovery.com/wp-content/uploads/google-developers.png" 
    height="30">
    </a>  &nbsp;&nbsp;
<a href="https://www.youtube.com/@joseluisgs" target="_blank">
        <img loading="lazy" src="https://upload.wikimedia.org/wikipedia/commons/e/ef/Youtube_logo.png" 
    height="30">
    </a>  
</p>

## Licencia de uso

Este repositorio y todo su contenido está licenciado bajo licencia **Creative Commons**, si desea saber más, vea
la [LICENSE](https://joseluisgs.dev/docs/license/). Por favor si compartes, usas o modificas este proyecto cita a su
autor, y usa las mismas condiciones para su uso docente, formativo o educativo y no comercial.

<a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/"><img alt="Licencia de Creative Commons" style="border-width:0" src="https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png" /></a><br /><span xmlns:dct="http://purl.org/dc/terms/" property="dct:title">
JoseLuisGS</span>
by <a xmlns:cc="http://creativecommons.org/ns#" href="https://joseluisgs.dev/" property="cc:attributionName" rel="cc:attributionURL">
José Luis González Sánchez</a> is licensed under
a <a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons
Reconocimiento-NoComercial-CompartirIgual 4.0 Internacional License</a>.<br />Creado a partir de la obra
en <a xmlns:dct="http://purl.org/dc/terms/" href="https://github.com/joseluisgs" rel="dct:source">https://github.com/joseluisgs</a>.
