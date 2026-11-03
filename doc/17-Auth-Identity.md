# 17. Authentication Identity

## Índice

[17. Authentication Identity](#17-authentication-identity)
  - [17.1. Configuración](#171-configuración)
  - [17.2. Claims](#172-claims)
  - [17.3. Autorización](#173-autorización)
  - [17.4. Claims Identity](#174-claims-identity)
  - [17.5. Login Externo con OAuth 2.0](#175-login-externo-con-oauth-20)
    - [17.5.1. ¿Qué es OAuth 2.0 y por qué usarlo?](#1751--qués-oauth-20-y-por-qué-usarlo)
    - [17.5.2. Arquitectura: Puertas Múltiples con Identity](#1752-arquitectura-puertas-múltiples-con-identity)
    - [17.5.3. Flujo OAuth](#1753-flujo-oauth)
    - [17.5.4. Implementación en el Proyecto](#1754-implementación-en-el-proyecto)
    - [17.5.5. Configuración](#1755-configuración)
    - [17.5.6. Puntos Clave para el Alumno](#1756-puntos-clave-para-el-alumno)
  - [17.6. Passkeys (FIDO2/WebAuthn)](#176-passkeys-fido2webauthn)
    - [17.6.1. Qué es una passkey](#1761-qué-es-una-passkey)
    - [17.6.2. Arquitectura: los dos flujos](#1762-arquitectura-los-dos-flujos)
    - [17.6.3. Configuración](#1763-configuración)
    - [17.6.4. Implementación en el Proyecto](#1764-implementación-en-el-proyecto)
    - [17.6.5. El cliente: passkeys.js](#1765-el-cliente-passkeysjs)
    - [17.6.6. Puntos Clave para el Alumno](#1766-puntos-clave-para-el-alumno)

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

## 17.5. Login Externo con OAuth 2.0

### 17.5.1. ¿Qué es OAuth 2.0 y por qué usarlo?

OAuth 2.0 es un **estándar de autorización** que permite a un usuario autenticarse con un proveedor externo (Google, GitHub, Microsoft) sin crear una contraseña nueva en nuestra aplicación.

**¿Qué aporta?**

| Beneficio | Explicación |
|-----------|-------------|
| **UX mejorada** | El usuario no necesita crear ni recordar otra contraseña |
| **Registro automático** | Primer login crea la cuenta local sin formulario |
| **Seguridad delegada** | Google/Microsoft gestionan 2FA y detección de phishing |
| **Educación** | Estándar real usado en producción |

### 17.5.2. Arquitectura: Puertas Múltiples con Identity

A diferencia de las APIs (que usan JWT manual), **ASP.NET Core Identity ya trae soporte OAuth integrado**:

```mermaid
flowchart TB
    subgraph "Puertas de Login"
        Client[Cliente Browser]
        Client --> Menu{¿Qué puerta?}
        Menu -->|Email/Password| Cookie[Identity Cookies<br/>POST /Auth/Login]
        Menu -->|Google| Google[Google OAuth<br/>GET /Auth/External/Google]
        Menu -->|GitHub| GitHub[GitHub OAuth<br/>GET /Auth/External/GitHub]
        Menu -->|Microsoft| MS[Microsoft OAuth<br/>GET /Auth/External/Microsoft]
    end

    subgraph "Identity procesa todo"
        Cookie --> Session[Cookie de sesión]
        Google -->|Challenge + Callback| Session
        GitHub -->|Challenge + Callback| Session
        MS -->|Challenge + Callback| Session
    end

    Session --> App[Aplicación protegida]

    style Cookie fill:#2ecc71,color:#fff
    style Session fill:#3498db,color:#fff
```

**¿Qué aporta Identity?** (a diferencia de las APIs)

| APIs (custom) | Web (Identity) |
|--------------|----------------|
| ❌ Tabla UserLogin manual | ✅ `AspNetUserLogins` automática |
| ❌ Parsear claims manualmente | ✅ `SignInManager.GetExternalLoginInfoAsync()` |
| ❌ `LoginExternalAsync` custom | ✅ `ExternalLoginSignInAsync()` |
| ❌ Dummy hash para PasswordHash | ✅ Identity maneja usuarios sin password |

### 17.5.3. Flujo OAuth

```mermaid
sequenceDiagram
    participant B as Browser
    participant Web as Web MVC/RazorPages
    participant G as Google OAuth

    B->>GET /Auth/External/Google
    Web->>B: 302 Redirect a accounts.google.com
    B->>G: Login en Google
    G->>B: 302 Redirect a /Auth/ExternalCallback?code=xxx
    B->>GET /Auth/ExternalCallback?code=xxx
    Web->>G: Intercambiar code por tokens
    G-->>Web: Email, Nombre, Avatar
    Web->>Web: GetExternalLoginInfoAsync()
    Web->>Web: ¿Existe vinculación? → SignInAsync()
    Web->>Web: ¿Primera vez? → Crear usuario + AddLoginAsync()
    Web-->>B: 302 Redirect + Cookie de sesión
```

### 17.5.4. Implementación en el Proyecto

**Configuración condicional (`OAuthConfig.cs`):**
```csharp
if (configuration.GetValue<bool>("Auth:Google:Enabled"))
{
    var clientId = configuration["Google:ClientId"];
    var clientSecret = configuration["Google:ClientSecret"];
    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        Log.Warning("🚪 Auth:Google:Enabled=true pero falta credenciales — puerta NO registrada");
    else
        auth.AddGoogle(options => { ... });
}
```

**Endpoints:**

| Proyecto | Challenge | Callback |
|----------|-----------|----------|
| MVC | `GET /Auth/External/{provider}` | `GET /Auth/ExternalCallback` |
| RazorPages | `GET /Auth/External?provider=X` | `GET /Auth/ExternalCallback` |

**Vistas con botones OAuth:**
- `Views/Auth/Login.cshtml` / `Pages/Auth/Login.cshtml`
- `Views/Auth/Register.cshtml` / `Pages/Auth/Register.cshtml`

Los botones aparecen después del `<hr>` y antes del link de cambio de vista.

### 17.5.5. Configuración

**appsettings.json** (Enabled: false por defecto):
```json
{ "Auth": { "Google": { "Enabled": false }, "GitHub": { "Enabled": false }, "Microsoft": { "Enabled": false } } }
```

**user-secrets:**
```bash
dotnet user-secrets set "Google:ClientId" "xxxx.apps.googleusercontent.com"
dotnet user-secrets set "Google:ClientSecret" "GOCSPX-xxxx"
```

### 17.5.6. Puntos Clave para el Alumno

| Concepto | Explicación |
|----------|-------------|
| **No rompe Identity** | OAuth añade esquemas, no modifica Cookies |
| **AspNetUserLogins** | Tabla automática de Identity para vinculaciones |
| **Configuración condicional** | Solo se registra si `Auth:{Provider}:Enabled = true` + credenciales |
| **Mismo patrón en MVC y RazorPages** | 4 vistas con botones + 2 endpoints por proyecto |
| **Identity hace el trabajo** | No necesitamos parsear claims ni crear tablas manualmente |

---

## 17.6. Passkeys (FIDO2/WebAuthn)

**.NET 10 trae las passkeys dentro de Identity: credenciales del dispositivo que sustituyen la contraseña sin cambiar el conducto de cookies que ya tienes montado.** La passkey nace en el navegador, con el autenticador del sistema operativo, y después el acceso es un gesto: el servidor manda un reto, el dispositivo lo firma y el servidor verifica la firma. No hay clave que robar ni que reutilizar entre sitios.

📌 **Ejemplo real:** Google y Apple abren las cuentas con passkey: registras el dispositivo una vez y después solo pides entrar con tu huella o tu cara, sin escribir ninguna clave.

### 17.6.1. Qué es una passkey

Una passkey es un par de claves asimétricas (FIDO2/WebAuthn) que se crea en el dispositivo del usuario. La clave privada nunca sale del autenticador; en el servidor solo se guarda la pública, en la tabla `AspNetUserPasskeys` que añade Identity.

| Aspecto | Descripción |
|---------|-------------|
| **Almacenamiento** | Clave pública en `AspNetUserPasskeys`; la privada vive en el dispositivo |
| **Acceso** | Gesto del dispositivo: huella, cara o PIN |
| **Phishing** | La passkey está atada al origen: en un dominio falso no sirve |
| **Relación con la contraseña** | La complementa: las dos conviven en la vista de acceso |

### 17.6.2. Arquitectura: los dos flujos

Hay dos flujos y los dos acaban escribiendo la misma cookie de identidad que el login con contraseña. El primero entra sin sesión; el segundo se hace con la sesión abierta, desde la zona de perfil.

```mermaid
sequenceDiagram
    participant N as Navegador
    participant W as Tu web
    participant D as Dispositivo (autenticador)

    Note over N,W: Flujo de acceso (anónimo)
    N->>W: POST /Auth/PasskeyRequestOptions
    W-->>N: Opciones con el reto (challenge)
    N->>D: navigator.credentials.get()
    D-->>N: Gesto del usuario (huella/cara/PIN) + firma
    N->>W: POST /Auth/PasskeyAssertion (credencial)
    W->>W: Verifica la firma con la clave pública
    W-->>N: Cookie de identidad + 302

    Note over N,W: Flujo de creación (sesión abierta)
    N->>W: POST /app/perfil/passkeys/crear-opciones
    W-->>N: Opciones de creación
    N->>D: navigator.credentials.create()
    D-->>N: Par de claves nuevo + atestación
    N->>W: POST /app/perfil/passkeys/atestacion
    W->>W: Verifica y guarda en AspNetUserPasskeys
    W-->>N: JSON { ok = true }
```

El patrón de Identity no cambia: el gestor sigue mandando.

```csharp
// El servidor solo pide opciones y verifica; la criptografía la hace el framework
var opciones = await signInManager.MakePasskeyRequestOptionsAsync(usuario);
var resultado = await signInManager.PasskeySignInAsync(credencial);
```

### 17.6.3. Configuración

La única pieza nueva de configuración es el esquema de tablas de Identity. Sin la siguiente línea, Identity crea el esquema antiguo y no existe la tabla `AspNetUserPasskeys`, así que guardar una passkey falla al escribir:

```csharp
// TiendaDawWeb.Shared/Infrastructures/AuthenticationConfig.cs
services.AddIdentity<User, IdentityRole<long>>(options =>
{
    // Esquema 3: añade la tabla AspNetUserPasskeys
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;

    // ... el resto de opciones de contraseña y bloqueo que ya tenías
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

> 📝 **Nota:** EnsureCreated solo crea la base de datos si no existe: en desarrollo el SQLite se borra en cada arranque y la tabla nace sola, pero en una BD ya poblada (producción) no añade nada. Por eso el arranque llama a `IdentitySchemaPatch.CompletarAsync`, que consulta el catálogo del proveedor y crea la tabla que falte con su índice. Sin migraciones, sin pasos manuales y sin tocar los datos existentes: desplegar es reiniciar.

### 17.6.4. Implementación en el Proyecto

Los endpoints van en las dos visiones, con la misma lógica y las mismas rutas adaptadas a cada una. Todos los POST llevan token anti-forgery.

| Paso | MVC | Razor Pages | Protección |
|------|-----|-------------|------------|
| Opciones de acceso | `POST /Auth/PasskeyRequestOptions` | `/Auth/Login?handler=PasskeyRequestOptions` | anónimo + token |
| Verificar y entrar | `POST /Auth/PasskeyAssertion` | `/Auth/Login?handler=PasskeyAssertion` | anónimo + token |
| Zona de gestión | `GET /app/perfil/passkeys` | `/Profile/Passkeys` | `[Authorize]` |
| Opciones de creación | `POST /app/perfil/passkeys/crear-opciones` | `/Profile/Passkeys?handler=CreationOptions` | `[Authorize]` + token |
| Guardar credencial | `POST /app/perfil/passkeys/atestacion` | `/Profile/Passkeys?handler=Attestation` | `[Authorize]` + token |
| Eliminar credencial | `POST /app/perfil/passkeys/eliminar` | `/Profile/Passkeys?handler=Eliminar` | `[Authorize]` + token |

En MVC la lógica vive en `Controllers/AuthController.cs` (acceso) y en `Controllers/ProfileController.cs` (gestión); en Razor Pages, en `Pages/Auth/Login.cshtml.cs` y en `Pages/Profile/Passkeys.cshtml.cs`. El acceso con passkey escribe la cookie exactamente igual que `PasswordSignInAsync`:

```csharp
// AuthController.cs
[HttpPost("/Auth/PasskeyAssertion")]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> PasskeyAssertion(string credential, string? returnUrl = null)
{
    var result = await signInManager.PasskeySignInAsync(credential);

    if (result.Succeeded)
        return Json(new { ok = true, destino = Destino(returnUrl) });

    return StatusCode(StatusCodes.Status401Unauthorized, new
    {
        ok = false,
        mensaje = "La passkey no es válida o ha caducado el reto; vuelve a intentarlo"
    });
}
```

Al guardar, el estado del reto nombra a una cuenta y se comprueba que es la conectada antes de escribir nada en la base:

```csharp
var userId = await userManager.GetUserIdAsync(user);
if (!string.Equals(result.UserEntity?.Id, userId, StringComparison.Ordinal))
    return StatusCode(StatusCodes.Status400BadRequest, new
    {
        ok = false,
        mensaje = "La passkey no pertenece a la cuenta que está conectada"
    });

var guardado = await userManager.AddOrUpdatePasskeyAsync(user, result.Passkey);
```

Las vistas nuevas son `Views/Profile/Passkeys.cshtml` y `Pages/Profile/Passkeys.cshtml` (lista, creación y borrado), y el botón de acceso se añade en `Views/Auth/Login.cshtml` y `Pages/Auth/Login.cshtml`, junto a los botones de OAuth.

### 17.6.5. El cliente: passkeys.js

El navegador no entiende de formularios: hace falta un poco de JavaScript para puentear WebAuthn y el POST clásico de la aplicación. `wwwroot/js/passkeys.js` (en las dos visiones) hace exactamente eso, sin librerías:

| Función | Qué hace |
|---------|----------|
| `entrar()` | Pide las opciones, ejecuta `navigator.credentials.get()` y manda la credencial al servidor |
| `crear()` | Pide las opciones de creación, ejecuta `navigator.credentials.create()` y registra la atestación |
| `serializar()` | Convierte la credencial a JSON en base64url, la forma en que WebAuthn transporta binario |
| `falloConocido()` | Traduce los errores del navegador (`NotAllowedError`, `NotSupportedError`) a mensajes entendibles |

El script se engancha a un contenedor `<div data-passkeys>` con atributos `data-opciones`, `data-afirmacion`, `data-creacion` y `data-atestacion`; el token anti-forgery viaja en el formulario del propio contenedor, así que los POST son idénticos a cualquier otro formulario de la aplicación.

> ⚠️ **Advertencia:** WebAuthn solo funciona en un contexto seguro: HTTPS o `localhost`. La cookie de identidad ya usa `SecurePolicy.Always`, así que en local y en producción el comportamiento es el mismo.

### 17.6.6. Puntos Clave para el Alumno

| Concepto | Explicación |
|----------|-------------|
| **No rompe Identity** | La passkey es un camino más de acceso: la cookie y la contraseña siguen igual |
| **AspNetUserPasskeys** | Tabla que añade `SchemaVersion = IdentitySchemaVersions.Version3` |
| **Mismo patrón en MVC y RazorPages** | 2 endpoints de acceso + 4 de gestión por proyecto |
| **El estado del reto, verificado** | La credencial se guarda solo si el reto nombra a la cuenta conectada |
| **Complementa, no sustituye** | En la vista de acceso conviven contraseña, OAuth y passkey |

---

## Resumen

| Concepto           | Descripción                                              |
| ------------------ | -------------------------------------------------------- |
| **Identity**      | Representa al usuario autenticado                         |
| **Claim**         | Declaración sobre el usuario                             |
| **Principal**     | Contiene la identidad y sus claims                      |
| **Authorization** | Control de acceso basado en roles/claims                 |
| **OAuth 2.0**     | Login externo con Google, GitHub, Microsoft (Identity)    |
| **Passkey**       | Acceso sin contraseña con el dispositivo (FIDO2/WebAuthn, Identity) |

---

**Anterior**: [16. Manejo de Excepciones](../16-Exception-Handling.md)  
**Próximo**: [18. Authentication Cookies](../18-Auth-Cookies.md)
