<#
.SYNOPSIS
    Ejecuta tests E2E contra MVC y Razor Pages simultáneamente.
#>

$ErrorActionPreference = "Stop"
$SolutionRoot = (Get-Location).Path

$MvcPort = 5000
$RazorPagesPort = 5002
$MvcUrl = "http://localhost:$MvcPort"
$RazorPagesUrl = "http://localhost:$RazorPagesPort"

$MvcProcess = $null
$RazorPagesProcess = $null

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
}

function Write-Step {
    param([string]$Message)
    Write-Host "[STEP] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Stop-Servers {
    Write-Step "Deteniendo servidores..."
    if ($MvcProcess -and -not $MvcProcess.HasExited) {
        Stop-Process -Id $MvcProcess.Id -Force -ErrorAction SilentlyContinue
    }
    if ($RazorPagesProcess -and -not $RazorPagesProcess.HasExited) {
        Stop-Process -Id $RazorPagesProcess.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 2
}

function Wait-ForUrl {
    param([string]$Url, [int]$TimeoutSeconds = 90)
    Write-Step "Esperando $Url..."
    $elapsed = 0
    while ($elapsed -lt $TimeoutSeconds) {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Step "$Url disponible!" -ForegroundColor Green
                return $true
            }
        } catch {}
        Start-Sleep -Seconds 2
        $elapsed += 2
    }
    Write-Error "$Url no disponible"
    return $false
}

function Start-Server {
    param([string]$ProjectPath, [int]$Port, [string]$Name)
    Write-Step "Iniciando $Name en puerto $Port..."
    return Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", $ProjectPath, "--urls", "http://localhost:$Port", "--no-build" -PassThru -NoNewWindow
}

# ============================================
Write-Header "E2E Test Runner"

# 1. Compilar
Write-Header "Compilando..."
dotnet build ".\TiendaDawWeb.Mvc\TiendaDawWeb.Mvc.csproj" -nologo -v q
dotnet build ".\TiendaDawWeb.RazorPages\TiendaDawWeb.RazorPages.csproj" -nologo -v q
dotnet build ".\TiendaDawWeb.Tests.E2E\TiendaDawWeb.Tests.E2E.csproj" -nologo -v q

# 2. Arrancar servidores
Write-Header "Iniciando servidores..."
$MvcProcess = Start-Server -ProjectPath ".\TiendaDawWeb.Mvc\TiendaDawWeb.Mvc.csproj" -Port $MvcPort -Name "MVC"
$RazorPagesProcess = Start-Server -ProjectPath ".\TiendaDawWeb.RazorPages\TiendaDawWeb.RazorPages.csproj" -Port $RazorPagesPort -Name "Razor Pages"

# 3. Esperar
if (-not (Wait-ForUrl -Url $MvcUrl)) { exit 1 }
if (-not (Wait-ForUrl -Url $RazorPagesUrl)) { exit 1 }

# 4. Tests MVC
Write-Header "Tests MVC ($MvcUrl)"
$env:E2E_BASE_URL = $MvcUrl
dotnet test ".\TiendaDawWeb.Tests.E2E\TiendaDawWeb.Tests.E2E.csproj" --filter "FullyQualifiedName~Tests" --no-build -v n
$MvcResult = $LASTEXITCODE

# 5. Tests Razor Pages
Write-Header "Tests Razor Pages ($RazorPagesUrl)"
$env:E2E_BASE_URL = $RazorPagesUrl
dotnet test ".\TiendaDawWeb.Tests.E2E\TiendaDawWeb.Tests.E2E.csproj" --filter "FullyQualifiedName~Tests" --no-build -v n
$RazorPagesResult = $LASTEXITCODE

# 6. Resumen
Write-Header "Resumen"
Write-Host "MVC:        $(if ($MvcResult -eq 0) { '✅' } else { '❌' })" -ForegroundColor $(if ($MvcResult -eq 0) { 'Green' } else { 'Red' })
Write-Host "RazorPages: $(if ($RazorPagesResult -eq 0) { '✅' } else { '❌' })" -ForegroundColor $(if ($RazorPagesResult -eq 0) { 'Green' } else { 'Red' })

Stop-Servers

if ($MvcResult -ne 0 -or $RazorPagesResult -ne 0) {
    exit 1
}
Write-Host ""
Write-Host "Todos los tests pasaron!" -ForegroundColor Green
exit 0
