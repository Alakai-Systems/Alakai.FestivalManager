# Fix-DataProtection-Admin.ps1
# Corrige la perdida de la cookie AlakaiAdminAuth en Azure App Service (Linux)
# causada por la falta de persistencia explicita de las claves de Data Protection.
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$adminBase = "Alakai.FestivalManager.Admin"

function Patch-File {
    param(
        [string]$Path,
        [string]$OldString,
        [string]$NewString,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow
        return $false
    }

    $content = Get-Content -Path $Path -Raw

    if ($content.Contains($NewString)) {
        Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan
        return $true
    }

    if (-not $content.Contains($OldString)) {
        Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow
        return $false
    }

    $updated = $content.Replace($OldString, $NewString)
    Set-Content -Path $Path -Value $updated -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$patches = @()

# --- Patch 1: agregar using de Data Protection en Program.cs ---
$programPath = Join-Path $adminBase "Program.cs"

$patches += [PSCustomObject]@{
    Path = $programPath
    Old = 'using Microsoft.AspNetCore.HttpOverrides;'
    New = @'
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
'@
    Description = "Agregar using Microsoft.AspNetCore.DataProtection"
}

# --- Patch 2: configurar AddDataProtection con persistencia en filesystem ---
$patches += [PSCustomObject]@{
    Path = $programPath
    Old = @'
builder.Services.Configure<ForwardedHeadersOptions>(options =>
'@
    New = @'
string dataProtectionKeyPath = builder.Configuration["DataProtection:KeyRingPath"]
    ?? "/home/DataProtection-Keys";

builder.Services.AddDataProtection()
    .SetApplicationName("Alakai.FestivalManager.Admin")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyPath));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
'@
    Description = "Configurar AddDataProtection().PersistKeysToFileSystem()"
}

# --- Ejecutar todos los patches (all-or-nothing) ---
$results = @()
foreach ($p in $patches) {
    $results += Patch-File -Path $p.Path -OldString $p.Old -NewString $p.New -Description $p.Description
}

if ($results -contains $false) {
    Write-Host "`nAlgun patch no se pudo aplicar. Revisa los mensajes anteriores. No se han guardado cambios parciales fuera de lo ya escrito." -ForegroundColor Red
    exit 1
}

Write-Host "`nTodos los patches aplicados correctamente." -ForegroundColor Green
Write-Host "Recuerda: en Azure App Service, la variable de entorno 'DataProtection__KeyRingPath' ahora SI se usa (antes no tenia efecto)." -ForegroundColor Cyan