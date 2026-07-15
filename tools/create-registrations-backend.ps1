# Fix-IdentityCoreVersionMismatch.ps1
#
# CAUSA RAIZ REAL del MissingMethodException de todo el dia:
#   Method not found: Boolean Microsoft.AspNetCore.Cryptography.CryptoUtil.TimeConstantBuffersAreEqual(...)
#
# Alakai.FestivalManager.Application referencia:
#   Microsoft.Extensions.Identity.Core              Version="10.0.9"   <- linea de .NET 10
#   Microsoft.Extensions.Options.ConfigurationExtensions Version="10.0.9"   <- linea de .NET 10
#
# El proyecto compila para net9.0, y el framework compartido de la app (DataProtection,
# incluido en Microsoft.AspNetCore.App para net9.0) espera trabajar con la linea 9.x de
# Microsoft.AspNetCore.Cryptography.Internal. Pero Microsoft.Extensions.Identity.Core 10.0.9
# arrastra Cryptography.Internal en su version 10.0.9 (linea .NET 10), que tiene cambios
# internos incompatibles con lo que el DataProtection de net9.0 espera. De ahi el
# MissingMethodException: dos "generaciones" de ASP.NET Core mezcladas en el mismo proceso.
#
# Fix: fijar ambos paquetes a la linea 9.x (9.0.9), coherente con el resto de la solucion
# (net9.0, EF Core 9.0.16, etc.)
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$applicationCsprojPath = "Alakai.FestivalManager.Application/Alakai.FestivalManager.Application.csproj"

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

    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")

    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"

    if ($normalizedContent.Contains($normalizedNew)) {
        Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan
        return $true
    }

    if (-not $normalizedContent.Contains($normalizedOld)) {
        Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow
        return $false
    }

    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)

    if ($usesCrlf) {
        $updatedFinal = $updatedNormalized -replace "`n", "`r`n"
    } else {
        $updatedFinal = $updatedNormalized
    }

    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$results = @()

$results += Patch-File -Path $applicationCsprojPath -Description "Bajar Microsoft.Extensions.Identity.Core de 10.0.9 a 9.0.9" -OldString @'
    <PackageReference Include="Microsoft.Extensions.Identity.Core" Version="10.0.9" />
'@ -NewString @'
    <PackageReference Include="Microsoft.Extensions.Identity.Core" Version="9.0.9" />
'@

$results += Patch-File -Path $applicationCsprojPath -Description "Bajar Microsoft.Extensions.Options.ConfigurationExtensions de 10.0.9 a 9.0.9" -OldString @'
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.9" />
'@ -NewString @'
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.9" />
'@

if ($results -contains $false) {
    Write-Host "`nAlgun patch no se pudo aplicar. Pega el contenido actual del csproj para ajustar el anchor." -ForegroundColor Red
    exit 1
}

Write-Host "`nPatch aplicado. Proximos pasos OBLIGATORIOS:" -ForegroundColor Green
Write-Host "  1. dotnet clean (en toda la solucion)" -ForegroundColor Yellow
Write-Host "  2. Borrar manualmente las carpetas obj/ y bin/ de los 6 proyectos" -ForegroundColor Yellow
Write-Host "  3. dotnet restore" -ForegroundColor Yellow
Write-Host "  4. dotnet build" -ForegroundColor Yellow
Write-Host "  5. Verificar en obj/Alakai.FestivalManager.Application/project.assets.json que" -ForegroundColor Yellow
Write-Host "     Microsoft.AspNetCore.Cryptography.Internal resuelve ahora a la linea 9.x, no 10.x" -ForegroundColor Yellow
Write-Host "  6. Commit + push + esperar el deploy" -ForegroundColor Yellow