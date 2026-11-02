<#
.SYNOPSIS
    Ejecuta tests E2E contra MVC y Razor Pages.

.DESCRIPTION
    Secuencial a propósito: primero se arranca MVC, se ejecutan sus tests y se
    detiene; después Razor Pages. Si los dos servidores arrancan a la vez
    comparten el fichero SQLite de desarrollo (tiendadb_dev.db) y se produce
    una carrera al crear las tablas ('table "AspNetRoles" already exists').
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

function Stop-Server {
    param($Process)
    if ($Process -and -not $Process.HasExited) {
        Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 2
}

# Libera el puerto por si quedó un servidor huérfano de una ejecución anterior.
function Clear-Port {
    param([int]$Port)
    $conexiones = netstat -ano | Select-String (":" + $Port + "\s+.*LISTENING")
    foreach ($c in $conexiones) {
        $procId = ($c.ToString().Trim() -split '\s+')[-1]
        Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 1
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

# Arranca un servidor, espera a que responda, ejecuta la suite E2E contra él
# y lo detiene. Devuelve el exit code de dotnet test (0 = verde).
function Invoke-Phase {
    param([string]$Nombre, [string]$ProjectPath, [int]$Port)

    Write-Header "Tests $Nombre (puerto $Port)"
    Clear-Port -Port $Port

    $server = Start-Server -ProjectPath $ProjectPath -Port $Port -Name $Nombre
    try {
        $url = "http://localhost:$Port"
        if (-not (Wait-ForUrl -Url $url)) { return 1 }

        $env:E2E_BASE_URL = $url

        # La salida de dotnet test se loguea a fichero: en la consola del
        # script se pierde el resumen y no se ve qué test ha fallado.
        $testLog = Join-Path $env:TEMP ("e2e-oficial-" + ($Nombre -replace ' ', '') + ".log")
        dotnet test ".\TiendaDawWeb.Tests.E2E\TiendaDawWeb.Tests.E2E.csproj" `
            --filter "FullyQualifiedName~Tests" --no-build -v n `
            --logger ("trx;LogFileName=" + ($Nombre -replace ' ', '') + ".trx") 2>&1 |
            Out-File -FilePath $testLog -Encoding utf8
        $exit = $LASTEXITCODE
        Write-Step "${Nombre}: exit=$exit (detalle en $testLog)"
        return $exit
    }
    finally {
        Stop-Server -Process $server
        Clear-Port -Port $Port
    }
}

try {
    # ============================================
    Write-Header "E2E Test Runner (secuencial: una app a la vez)"

    # 1. Compilar
    Write-Header "Compilando..."
    dotnet build ".\TiendaDawWeb.Mvc\TiendaDawWeb.Mvc.csproj" -nologo -v q
    dotnet build ".\TiendaDawWeb.RazorPages\TiendaDawWeb.RazorPages.csproj" -nologo -v q
    dotnet build ".\TiendaDawWeb.Tests.E2E\TiendaDawWeb.Tests.E2E.csproj" -nologo -v q

    # 2. Fases secuenciales: primero MVC, después Razor Pages
    $MvcResult = Invoke-Phase -Nombre "MVC" -ProjectPath ".\TiendaDawWeb.Mvc\TiendaDawWeb.Mvc.csproj" -Port $MvcPort
    $RazorPagesResult = Invoke-Phase -Nombre "Razor Pages" -ProjectPath ".\TiendaDawWeb.RazorPages\TiendaDawWeb.RazorPages.csproj" -Port $RazorPagesPort

    # 3. Resumen
    Write-Header "Resumen"
    Write-Host "MVC:        $(if ($MvcResult -eq 0) { 'OK' } else { 'FAIL' })" -ForegroundColor $(if ($MvcResult -eq 0) { 'Green' } else { 'Red' })
    Write-Host "RazorPages: $(if ($RazorPagesResult -eq 0) { 'OK' } else { 'FAIL' })" -ForegroundColor $(if ($RazorPagesResult -eq 0) { 'Green' } else { 'Red' })

    if ($MvcResult -ne 0 -or $RazorPagesResult -ne 0) {
        exit 1
    }
    Write-Host ""
    Write-Host "Todos los tests pasaron!" -ForegroundColor Green
    exit 0
}
finally {
    # Pase lo que pase, no quedan servidores colgados.
    Stop-Server -Process $MvcProcess
    Stop-Server -Process $RazorPagesProcess
}
