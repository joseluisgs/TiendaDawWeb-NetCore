# 17. Authentication Identity

## Índice

[17. Authentication Identity](#17-authentication-identity)
  - [17.1. Configuración](#171-configuración)
  - [17.2. Claims](#172-claims)
  - [17.3. Autorización](#173-autorización)
  - [17.4. Claims Identity](#174-claims-identity)

---

## 17.1. Configuración

### AddIdentityCore

```csharp
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
```

### Configuración de Cookies

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
```

---

## 17.2. Claims

### ¿Qué es un Claim?

Un **Claim** es una declaración sobre una entidad, típicamente el usuario.

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.Email),
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Role, user.Rol),
    new Claim("Department", user.Department),
    new Claim("Level", user.Level.ToString())
};

var identity = new ClaimsIdentity(claims, 
    CookieAuthenticationDefaults.AuthenticationScheme);

var principal = new ClaimsPrincipal(identity);
```

### ClaimTypes Comunes

| Claim Type              | Descripción                              |
| ---------------------- | --------------------------------------- |
| `ClaimTypes.Name`       | Nombre del usuario                      |
| `ClaimTypes.Email`      | Correo electrónico                      |
| `ClaimTypes.NameIdentifier`| Identificador único del usuario       |
| `ClaimTypes.Role`      | Rol del usuario                          |
| `ClaimTypes.GivenName`  | Nombre propio                           |
| `ClaimTypes.Surname`   | Apellido                                |

---

## 17.3. Autorización

### Autorización por Roles

```csharp
[Authorize(Roles = "ADMIN")]
public IActionResult AdminPanel()
{
    return View();
}

[Authorize(Roles = "ADMIN,MANAGER")]
public IActionResult ManagementDashboard()
{
    return View();
}
```

### Autorización por Políticas

```csharp
// Registrar política
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireModerator", policy => 
        policy.RequireRole("ADMIN", "MODERATOR"));
    
    options.AddPolicy("SeniorUser", policy => 
        policy.RequireClaim("Level", "SENIOR", "LEAD"));
    
    options.AddPolicy("MinimumAge", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

// Usar política
[Authorize(Policy = "RequireModerator")]
public IActionResult ModeratorPanel()
{
    return View();
}
```

### Claims-based Authorization

```csharp
public class DepartmentHandler : AuthorizationHandler<DepartmentRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentRequirement requirement)
    {
        if (context.User.HasClaim(c => 
            c.Type == "Department" && 
            c.Value == requirement.Department))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

---

## 17.4. Claims Identity

### ClaimsIdentity vs FormsIdentity

| Aspecto              | FormsIdentity              | ClaimsIdentity              |
| ------------------- | ------------------------ | -------------------------- |
| **Tipo**            | Simple (Name, IsAuth)    | Flexible (colección Claims) |
| **Roles**           | Simple boolean            | Claim                      |
| **Propiedades**     | Limitadas                | Ilimitadas                |
| **Flexibilidad**    | Baja                      | Alta                       |

### ClaimsPrincipal Actual

```csharp
// Obtener usuario actual
var user = User;

// Claims del usuario actual
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var userName = User.Identity?.Name;
var userEmail = User.FindFirstValue(ClaimTypes.Email);
var userRole = User.FindFirstValue(ClaimTypes.Role);

// Verificar roles
if (User.IsInRole("ADMIN"))
{
    // Es admin
}

// Verificar claims
if (User.HasClaim(c => c.Type == "Department" && c.Value == "IT"))
{
    // Es del departamento de IT
}
```

---

## Resumen

| Concepto           | Descripción                                              |
| ------------------ | -------------------------------------------------------- |
| **Identity**      | Representa al usuario autenticado                         |
| **Claim**         | Declaración sobre el usuario                             |
| **Principal**     | Contiene la identidad y sus claims                      |
| **Authorization** | Control de acceso basado en roles/claims                 |

---

**Anterior**: [16. Manejo de Excepciones](../16-Exception-Handling.md)  
**Próximo**: [18. Authentication Cookies](../18-Auth-Cookies.md)
