# Fix-IdentityPackageConflict.ps1  (v2 - robusto ante CRLF/LF)
#
# CAUSA RAIZ del "AntiforgeryValidationException / MissingMethodException:
# Method not found: Boolean Microsoft.AspNetCore.Cryptography.CryptoUtil.TimeConstantBuffersAreEqual(...)"
#
# Alakai.FestivalManager.Application referencia el paquete NuGet
#   Microsoft.AspNetCore.Identity 2.3.11 (netstandard2.0, de la era ASP.NET Core 2.x)
# solo para usar PasswordHasher<User>. Esa version antigua arrastra una copia vieja
# de Microsoft.AspNetCore.Cryptography.Internal que se publica junto al binario y
# choca con la version moderna de .NET 9 en tiempo de ejecucion en Azure Linux.
#
# Fix: quitar el PackageReference viejo y usar FrameworkReference al shared
# framework de ASP.NET Core, que resuelve PasswordHasher<T> desde .NET 9 sin
# arrastrar ninguna DLL antigua.
#
# v2: la version anterior fallaba con "anchor no encontrado" en checkouts donde
# Git normaliza el archivo a CRLF (core.autocrlf=true). Esta version normaliza
# saltos de linea antes de comparar, y respeta el estilo de salto de linea
# original del archivo al guardar.
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

    # Normalizar todo a LF para comparar de forma independiente del estilo de salto de linea
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

    # Devolver al estilo de salto de linea original del archivo
    if ($usesCrlf) {
        $updatedFinal = $updatedNormalized -replace "`n", "`r`n"
    } else {
        $updatedFinal = $updatedNormalized
    }

    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$result = Patch-File -Path $applicationCsprojPath -Description "Reemplazar PackageReference Microsoft.AspNetCore.Identity 2.3.11 por FrameworkReference" -OldString @'
  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="16.1.1" />
    <PackageReference Include="FluentValidation" Version="12.1.1" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
    <PackageReference Include="Microsoft.AspNetCore.Identity" Version="2.3.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.16" />
    <PackageReference Include="Microsoft.Extensions.Identity.Core" Version="10.0.9" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.9" />
    <PackageReference Include="QuestPDF" Version="2026.6.1" />
    <PackageReference Include="SixLabors.ImageSharp" Version="3.1.5" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.19.1" />
    <PackageReference Include="ClosedXML" Version="0.102.2" />
  </ItemGroup>
'@ -NewString @'
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="16.1.1" />
    <PackageReference Include="FluentValidation" Version="12.1.1" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.16" />
    <PackageReference Include="Microsoft.Extensions.Identity.Core" Version="10.0.9" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.9" />
    <PackageReference Include="QuestPDF" Version="2026.6.1" />
    <PackageReference Include="SixLabors.ImageSharp" Version="3.1.5" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.19.1" />
    <PackageReference Include="ClosedXML" Version="0.102.2" />
  </ItemGroup>
'@

if (-not $result) {
    Write-Host "`nSigue sin encontrar el anchor. Pega el contenido completo de tu csproj real para comparar directamente." -ForegroundColor Red
    exit 1
}

Write-Host "`nPatch aplicado. Proximos pasos OBLIGATORIOS:" -ForegroundColor Green
Write-Host "  1. dotnet clean (en toda la solucion)" -ForegroundColor Yellow
Write-Host "  2. Borrar manualmente las carpetas obj/ y bin/ de los 6 proyectos" -ForegroundColor Yellow
Write-Host "  3. dotnet restore" -ForegroundColor Yellow
Write-Host "  4. dotnet build" -ForegroundColor Yellow
Write-Host "  5. Confirmar en obj/*/project.assets.json que YA NO aparece Microsoft.AspNetCore.DataProtection 2.3.0" -ForegroundColor Yellow
Write-Host "  6. Redeploy a Azure" -ForegroundColor Yellow