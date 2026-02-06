# 18. Authentication Cookies

## Índice

[18. Authentication Cookies](#18-authentication-cookies)
  - [18.1. Cookies vs JWT](#181-cookies-vs-jwt)
  - [18.2. Configuración](#182-configuración)
  - [18.3. Login/Logout](#183-loginlogout)
  - [18.4. Claims y Sesión](#184-claims-y-sesión)

---

## 18.1. Cookies vs JWT

| Aspecto              | Cookies                     | JWT                       |
| ------------------- | -------------------------- | ------------------------ |
| **Estado**          | Stateful                   | Stateless                |
| **Almacenamiento**   | Cookie del navegador       | LocalStorage, Cookie     |
| **Escalabilidad**   | Sticky sessions            | Horizontal               |
| **Protección CSRF** | Requerido                  | No necesario             |
| **Best for**        | Web apps tradicionales     | APIs, SPAs, móviles      |
| **Expiration**      | Sliding/Fixed              | Token expira             |

### ¿Por qué Cookies en WalaDaw?

- Aplicación web tradicional con Razor Pages
- SEO es importante
- Csrf protection integrada
- Facilidad de uso

---

## 18.2. Configuración

### Configuración Básica

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        
        options.Cookie.Name = ".TiendaDaw.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
```

### Configuración de Seguridad

```csharp
options.Cookie.HttpOnly = true;  // Inaccesible desde JavaScript
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Solo HTTPS
options.Cookie.SameSite = SameSiteMode.Strict;  // CSRF protection
options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
options.SlidingExpiration = true;
```

---

## 18.3. Login/Logout

### Login con SignInManager

```csharp
public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Email no registrado");
            return View(dto);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: dto.RememberMe);
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Cuenta bloqueada");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Contraseña incorrecta");
        }

        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
```

---

## 18.4. Claims y Sesión

### Claims durante el Login

```csharp
// Agregar claims personalizados durante el login
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.UserName),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim("Department", user.Department),
    new Claim("LastLogin", DateTime.UtcNow.ToString())
};

var claimsIdentity = new ClaimsIdentity(
    claims, 
    CookieAuthenticationDefaults.AuthenticationScheme);

var authProperties = new AuthenticationProperties
{
    IsPersistent = dto.RememberMe,
    ExpiresUtc = DateTime.UtcNow.AddHours(24)
};

await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(claimsIdentity),
    authProperties);
```

### Acceder a Claims en la Vista

```html
@if (User.Identity?.IsAuthenticated == true)
{
    <div class="user-info">
        <p>Bienvenido, @User.Identity.Name</p>
        
        @if (User.IsInRole("ADMIN"))
        {
            <a href="/Admin">Panel de Administración</a>
        }
        
        @if (User.HasClaim(c => c.Type == "Department"))
        {
            <span>Departamento: @User.FindFirstValue("Department")</span>
        }
    </div>
}
else
{
    <a href="/Account/Login">Iniciar Sesión</a>
}
```

---

## Resumen

| Concepto           | Descripción                                              |
| ------------------ | -------------------------------------------------------- |
| **Cookie Auth**    | Autenticación basada en cookies                           |
| **SignInManager** | Manejo de login/logout                                   |
| **Claims**        | Información adicional sobre el usuario                   |
| **Sliding Expiration** | Renovar cookie actividad                          |

---

**Anterior**: [17. Authentication Identity](../17-Auth-Identity.md)  
**Próximo**: [19. Unit Testing](../19-Unit-Testing.md)
