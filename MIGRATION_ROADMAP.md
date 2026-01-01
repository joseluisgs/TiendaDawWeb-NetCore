# 🚀 Migration Roadmap: TiendaDawWeb SpringBoot → .NET 10

This document provides a comprehensive guide for completing the migration of the TiendaDawWeb e-commerce application from Java/Spring Boot to .NET 10.

## 📊 Migration Status

### ✅ Completed Components

#### 1. Solution Structure & Build Configuration
- ✅ Solution file (`.sln`) with main project and test project
- ✅ Strict nullable reference types enabled
- ✅ TreatWarningsAsErrors configuration
- ✅ NuGet packages: Serilog, MailKit, iText7, ImageSharp, EF Core, Identity
- ✅ Test infrastructure: NUnit, Moq, FluentAssertions, Coverlet

#### 2. Core Domain Models
- ✅ **User**: ASP.NET Core Identity with soft delete (Deleted, DeletedAt, DeletedBy)
- ✅ **Product**: With reservation fields (Reservado, ReservadoHasta) and soft delete
- ✅ **CarritoItem**: NEW model with optimistic concurrency control (RowVersion)
- ✅ **Favorite**: Many-to-many relationship
- ✅ **Rating**: Product reviews and ratings
- ✅ **Purchase**: Purchase history

#### 3. Error Handling (Railway Oriented Programming)
- ✅ DomainError base class
- ✅ ProductError, FavoriteError, UserError
- ✅ **CarritoError**: NEW with comprehensive error cases
- ✅ **GenericError**: For infrastructure errors
- ✅ Result<T, TError> pattern established

#### 4. Services with ROP
- ✅ **ICarritoService & CarritoService**: COMPLETE implementation with:
  - Optimistic concurrency control
  - DbUpdateConcurrencyException handling
  - Add/Update/Remove/Clear operations
  - Total and count calculations
  - Product availability validation
- ✅ IProductService & ProductService (partial)
- ✅ IFavoriteService & FavoriteService
- ✅ IStorageService & StorageService

#### 5. Testing
- ✅ Test project with NUnit framework
- ✅ **CarritoServiceTests**: 11 comprehensive test cases including:
  - Add to cart scenarios
  - Update quantity
  - Remove and clear operations
  - Total and count calculations
  - Concurrency handling
  - Error cases validation

#### 6. Infrastructure
- ✅ ApplicationDbContext with EF Core InMemory
- ✅ ASP.NET Core Identity configuration
- ✅ Cookie authentication
- ✅ Session management
- ✅ Blazor Server support enabled

### 🔨 In Progress / Needs Completion

#### 1. Additional Services Required

```csharp
// Priority 1: Purchase flow
public interface IPurchaseService
{
    Task<Result<Purchase, DomainError>> CreatePurchaseAsync(long usuarioId);
    Task<Result<byte[], DomainError>> GeneratePdfAsync(long purchaseId);
    Task<Result<bool, DomainError>> SendConfirmationEmailAsync(long purchaseId);
}

// Priority 2: User management
public interface IUserService
{
    Task<Result<User, DomainError>> GetByIdAsync(long id);
    Task<Result<User, DomainError>> UpdateProfileAsync(long id, UpdateUserDto dto);
    Task<Result<bool, DomainError>> SoftDeleteAsync(long id, string deletedBy);
    Task<Result<IEnumerable<User>, DomainError>> GetAllAsync(int page, int pageSize);
}

// Priority 3: Product enhancements
// Add to existing IProductService:
- Task<Result<bool, DomainError>> ReserveTemporallyAsync(long productId, TimeSpan duration);
- Task<Result<IEnumerable<Product>, DomainError>> SearchAsync(string query);
- Task<Result<IEnumerable<Product>, DomainError>> FilterAsync(ProductFilter filter);

// Priority 4: Rating service
public interface IRatingService
{
    Task<Result<Rating, DomainError>> CreateAsync(long usuarioId, long productoId, CreateRatingDto dto);
    Task<Result<IEnumerable<Rating>, DomainError>> GetByProductIdAsync(long productoId);
    Task<Result<double, DomainError>> GetAverageRatingAsync(long productoId);
}

// Priority 5: Image processing
public interface IImageService
{
    Task<Result<string, DomainError>> ProcessAndSaveAsync(IFormFile file, int width = 800, int height = 600);
    Task<Result<bool, DomainError>> DeleteAsync(string filename);
}

// Priority 6: Email service
public interface IEmailService
{
    Task<Result<bool, DomainError>> SendPurchaseConfirmationAsync(Purchase purchase);
    Task<Result<bool, DomainError>> SendWelcomeEmailAsync(User user);
}

// Priority 7: PDF service
public interface IPdfService
{
    Task<Result<byte[], DomainError>> GenerateInvoiceAsync(Purchase purchase);
}
```

#### 2. Controllers Needed

```csharp
// Priority 1: Shopping cart
[Route("Carrito")]
public class CarritoController : Controller
{
    // GET: /Carrito
    // POST: /Carrito/Add
    // POST: /Carrito/Update
    // POST: /Carrito/Remove
    // POST: /Carrito/Clear
}

// Priority 2: Purchase flow
[Route("Compra")]
public class CompraController : Controller
{
    // POST: /Compra/Finalizar
    // GET: /Compra/Confirmacion/{id}
    // GET: /Compra/Pdf/{id}
}

// Priority 3: Admin dashboard
[Authorize(Roles = "ADMIN")]
[Route("Admin")]
public class AdminController : Controller
{
    // GET: /Admin/Dashboard
    // GET: /Admin/Usuarios
    // GET: /Admin/Productos
    // GET: /Admin/Ventas
}

// Priority 4: User profile
[Authorize]
[Route("Perfil")]
public class PerfilController : Controller
{
    // GET: /Perfil
    // POST: /Perfil/Editar
    // POST: /Perfil/CambiarAvatar
}

// Priority 5: Ratings
[Route("Rating")]
public class RatingController : Controller
{
    // POST: /Rating/Create
    // GET: /Rating/Product/{id}
}
```

#### 3. Blazor Server Components (Replace AJAX)

```razor
@* Priority 1: Cart summary in navbar *@
<CartSummary></CartSummary>

@* Priority 2: Favorites list *@
<FavoritesList UserId="@userId"></FavoritesList>

@* Priority 3: Rating stars *@
<RatingStars ProductId="@productId" OnRatingChanged="@HandleRating"></RatingStars>

@* Priority 4: Ratings list *@
<RatingsList ProductId="@productId"></RatingsList>

@* Priority 5: Product card *@
<ProductCard Product="@product"></ProductCard>

@* Priority 6: Notifications *@
<Notifications></Notifications>
```

#### 4. Razor Views from Pebble Templates

**Critical views to migrate:**

1. **Admin/Dashboard.cshtml** - Must include Chart.js for statistics
2. **Carrito/Index.cshtml** - Shopping cart view
3. **Compra/Confirmacion.cshtml** - Purchase confirmation
4. **Perfil/Index.cshtml** - User profile
5. **Admin views** - Users, Products, Sales management with pagination
6. **Shared partials** - _Footer, _Carousel

#### 5. Background Services (Hosted Services)

```csharp
// Priority 1: Clean expired reservations
public class LimpiezaReservasHostedService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Clean products with expired ReservadoHasta
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}

// Priority 2: Clean abandoned carts
public class LimpiezaCarritosHostedService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Remove cart items older than 30 days
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
```

#### 6. Internationalization

Create `.resx` files for 5 languages (es-ES, en-US, fr-FR, de-DE, pt-PT):

```
/Resources
  ├── SharedResources.es-ES.resx
  ├── SharedResources.en-US.resx
  ├── SharedResources.fr-FR.resx
  ├── SharedResources.de-DE.resx
  └── SharedResources.pt-PT.resx
```

#### 7. Seed Data Enhancement

Update `SeedData.cs` to replicate EXACTLY the 10 users and products from Java's `DataFactory.java`.

#### 8. Docker & CI/CD

```dockerfile
# Dockerfile - Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TiendaDawWeb.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TiendaDawWeb.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    volumes:
      - ./uploads:/app/uploads
```

```yaml
# .github/workflows/ci.yml
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
      - name: Test Coverage
        run: dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 📝 Documentation to Create

1. **RazorViews.md** - Guide for Razor views and partial views
2. **BlazorServer.md** - Blazor components integration
3. **RailwayOrientedProgramming.md** - ROP pattern explanation with examples
4. **Concurrency.md** - Optimistic concurrency control implementation
5. **Testing.md** - Testing strategy and guidelines
6. **Deployment.md** - Production deployment guide

## 🎯 Next Steps (Priority Order)

1. **Fix Test Project** - Resolve NUnit package references
2. **Complete CarritoController** - Implement shopping cart operations
3. **Create PurchaseService** - With PDF and email
4. **Implement Hosted Services** - Background cleanup tasks
5. **Create Admin Dashboard** - With Chart.js statistics
6. **Add Blazor Components** - CartSummary, RatingStars, etc.
7. **Internationalization** - .resx files for 5 languages
8. **Docker & CI/CD** - Complete DevOps setup
9. **Documentation** - Create all markdown guides
10. **Final Testing** - Achieve 80%+ code coverage

## 💡 Implementation Notes

### Concurrency Control
The CarritoItem model uses `[Timestamp]` attribute for optimistic concurrency:
```csharp
[Timestamp]
public byte[]? RowVersion { get; set; }
```

This automatically generates concurrency tokens in EF Core.

### Railway Oriented Programming
All services return `Result<T, DomainError>`:
```csharp
public async Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(...)
{
    if (condition) 
        return Result.Failure<CarritoItem, DomainError>(CarritoError.InvalidQuantity(...));
    
    return Result.Success<CarritoItem, DomainError>(item);
}
```

### Soft Delete Pattern
Applied to User and Product:
```csharp
public bool Deleted { get; set; } = false;
public DateTime? DeletedAt { get; set; }
public string? DeletedBy { get; set; }
```

Query filter in DbContext:
```csharp
entity.HasQueryFilter(p => !p.Deleted);
```

## 📚 References

- **Original Java Project**: https://github.com/joseluisgs/TiendaDawWeb-SpringBoot
- **Railway Oriented Programming**: https://fsharpforfunandprofit.com/rop/
- **ASP.NET Core Docs**: https://docs.microsoft.com/en-us/aspnet/core/
- **Blazor Server**: https://docs.microsoft.com/en-us/aspnet/core/blazor/

---

**Estimated Remaining Work**: 80-100 hours for complete migration with all features, tests, and documentation.
