# Script para generar PageModels automáticamente desde controladores MVC
# Uso: .\generate-pagemodels.ps1

$ErrorActionPreference = "Stop"

# Configuración
$projectRoot = "C:\Users\joseluisgs\Projects\Tienda\TiendaDawWeb-NetCore"
$razorPagesRoot = "$projectRoot\TiendaDawWeb.Web.RazorPages\Pages"
$controllersRoot = "$projectRoot\TiendaDawWeb.Web\Controllers"

# Estadísticas
$createdCount = 0
$modifiedCount = 0
$errorCount = 0
$filesProcessed = @()

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GENERADOR DE PAGEMODELS RAZOR PAGES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Mapeo de vistas a controladores y acciones
$viewMapping = @{
    # Public
    "Public\Index.cshtml" = @{ Controller = "PublicController"; Action = "Index"; Methods = @("GET") }
    
    # Auth
    "Auth\Login.cshtml" = @{ Controller = "AuthController"; Action = "Login"; Methods = @("GET", "POST") }
    "Auth\Register.cshtml" = @{ Controller = "AuthController"; Action = "Register"; Methods = @("GET", "POST") }
    "Auth\AccessDenied.cshtml" = @{ Controller = "AuthController"; Action = "AccessDenied"; Methods = @("GET") }
    
    # Product
    "Product\Index.cshtml" = @{ Controller = "ProductController"; Action = "Index"; Methods = @("GET") }
    "Product\Details.cshtml" = @{ Controller = "ProductController"; Action = "Details"; Methods = @("GET") }
    "Product\Create.cshtml" = @{ Controller = "ProductController"; Action = "Create"; Methods = @("GET", "POST") }
    "Product\Edit.cshtml" = @{ Controller = "ProductController"; Action = "Edit"; Methods = @("GET", "POST") }
    "Product\MyProducts.cshtml" = @{ Controller = "ProductController"; Action = "MyProducts"; Methods = @("GET") }
    
    # Admin
    "Admin\Index.cshtml" = @{ Controller = "AdminController"; Action = "Index"; Methods = @("GET") }
    "Admin\Usuarios.cshtml" = @{ Controller = "AdminController"; Action = "Usuarios"; Methods = @("GET") }
    "Admin\UsuarioDetails.cshtml" = @{ Controller = "AdminController"; Action = "UsuarioDetails"; Methods = @("GET") }
    "Admin\Productos.cshtml" = @{ Controller = "AdminController"; Action = "Productos"; Methods = @("GET") }
    "Admin\Compras.cshtml" = @{ Controller = "AdminController"; Action = "Compras"; Methods = @("GET") }
    "Admin\Ventas.cshtml" = @{ Controller = "AdminController"; Action = "Ventas"; Methods = @("GET") }
    "Admin\Estadisticas.cshtml" = @{ Controller = "AdminController"; Action = "Estadisticas"; Methods = @("GET") }
    
    # Carrito
    "Carrito\Index.cshtml" = @{ Controller = "CarritoController"; Action = "Index"; Methods = @("GET") }
    "Carrito\Resumen.cshtml" = @{ Controller = "CarritoController"; Action = "Resumen"; Methods = @("GET") }
    
    # Purchase
    "Purchase\Index.cshtml" = @{ Controller = "PurchaseController"; Action = "Index"; Methods = @("GET") }
    "Purchase\Details.cshtml" = @{ Controller = "PurchaseController"; Action = "Details"; Methods = @("GET") }
    "Purchase\Confirmacion.cshtml" = @{ Controller = "PurchaseController"; Action = "Confirmacion"; Methods = @("GET") }
    
    # Profile
    "Profile\Index.cshtml" = @{ Controller = "ProfileController"; Action = "Index"; Methods = @("GET") }
    "Profile\Edit.cshtml" = @{ Controller = "ProfileController"; Action = "Edit"; Methods = @("GET", "POST") }
    "Profile\ChangePassword.cshtml" = @{ Controller = "ProfileController"; Action = "ChangePassword"; Methods = @("GET", "POST") }
    
    # Favorite
    "Favorite\Index.cshtml" = @{ Controller = "FavoriteController"; Action = "Index"; Methods = @("GET") }
}

# Función para leer el controlador y extraer información
function Get-ControllerInfo {
    param($controllerPath, $actionName)
    
    if (-not (Test-Path $controllerPath)) {
        return $null
    }
    
    $content = Get-Content $controllerPath -Raw
    return $content
}

# Función para generar PageModel
function Generate-PageModel {
    param(
        $area,
        $pageName,
        $controllerName,
        $actionName,
        $methods,
        $controllerContent
    )
    
    # Determinar namespace y modelo
    $namespace = "TiendaDawWeb.Web.RazorPages.Pages.$area"
    $className = "${pageName}Model"
    
    # Extraer usings del controlador
    $usings = @(
        "using Microsoft.AspNetCore.Mvc;",
        "using Microsoft.AspNetCore.Mvc.RazorPages;",
        "using Microsoft.AspNetCore.Identity;",
        "using TiendaDawWeb.Shared.Models;",
        "using TiendaDawWeb.Shared.ViewModels;"
    )
    
    # Agregar usings adicionales según el controlador
    if ($controllerContent -match "using ([^;]+Services[^;]+);") {
        $usings += $Matches[0]
    }
    if ($controllerContent -match "using TiendaDawWeb\.Shared\.Data;") {
        $usings += "using TiendaDawWeb.Shared.Data;"
    }
    if ($controllerContent -match "using TiendaDawWeb\.Shared\.Models\.Enums;") {
        $usings += "using TiendaDawWeb.Shared.Models.Enums;"
    }
    if ($controllerContent -match "using TiendaDawWeb\.Shared\.Services\.Storage;") {
        $usings += "using TiendaDawWeb.Shared.Services.Storage;"
    }
    if ($controllerContent -match "using Microsoft\.AspNetCore\.SignalR;") {
        $usings += "using Microsoft.AspNetCore.SignalR;"
        $usings += "using TiendaDawWeb.Shared.Web.Hubs;"
    }
    if ($controllerContent -match "using Microsoft\.EntityFrameworkCore;") {
        $usings += "using Microsoft.EntityFrameworkCore;"
    }
    if ($controllerContent -match "using Microsoft\.AspNetCore\.Localization;") {
        $usings += "using Microsoft.AspNetCore.Localization;"
    }
    if ($controllerContent -match "using Microsoft\.AspNetCore\.OutputCaching;") {
        $usings += "using Microsoft.AspNetCore.OutputCaching;"
    }
    
    $usings = $usings | Sort-Object -Unique
    
    # Extraer parámetros del constructor del controlador
    $constructorParams = ""
    if ($controllerContent -match "public class $controllerName\(([^)]+)\)") {
        $constructorParams = $Matches[1].Trim()
    }
    
    # Determinar si necesita autorización
    $authorize = ""
    if ($controllerContent -match "\[Authorize\]") {
        $authorize = "[Microsoft.AspNetCore.Authorization.Authorize]`n"
    }
    if ($controllerContent -match "\[Authorize\(Roles = ""([^""]+)""\)\]") {
        $authorize = "[Microsoft.AspNetCore.Authorization.Authorize(Roles = ""$($Matches[1])"")]`n"
    }
    
    # Generar métodos OnGet/OnPost
    $onGetMethod = ""
    $onPostMethod = ""
    $bindProperty = ""
    
    # Determinar si hay POST y qué modelo usa
    $hasPost = $methods -contains "POST"
    $viewModelType = ""
    
    if ($hasPost) {
        # Buscar el tipo del modelo en el método POST
        if ($controllerContent -match "Task<IActionResult> $actionName\(([^)]+\s+)(\w+ViewModel)") {
            $viewModelType = $Matches[2]
            $bindProperty = @"
    [BindProperty]
    public $viewModelType Input { get; set; } = default!;

"@
        }
        elseif ($controllerContent -match "Task<IActionResult> $actionName\(long id,\s*([^)]+\s+)(\w+ViewModel)") {
            $viewModelType = $Matches[2]
            $bindProperty = @"
    [BindProperty]
    public $viewModelType Input { get; set; } = default!;

"@
        }
    }
    
    # Extraer la lógica del método GET
    if ($controllerContent -match "(?s)public (?:async )?Task<IActionResult> $actionName\([^)]*\)\s*\{([^}]+(?:\{[^}]+\}[^}])*)\}") {
        $getLogic = $Matches[1].Trim()
        
        # Convertir return View() a return Page()
        $getLogic = $getLogic -replace "return View\((.*?)\);", "return Page(`$1);"
        $getLogic = $getLogic -replace "return RedirectToAction", "return RedirectToPage"
        
        # Determinar si es async
        $isAsync = $controllerContent -match "public async Task<IActionResult> $actionName"
        $methodSignature = if ($isAsync) { "public async Task<IActionResult> OnGetAsync(" } else { "public IActionResult OnGet(" }
        
        # Extraer parámetros de
