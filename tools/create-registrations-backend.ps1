# Fix-SqlConnectionResiliency.ps1
#
# Log de la API muestra: "Connection reset by peer" durante el handshake TLS/login
# contra Azure SQL (tcp:sql-alakai-swimout.database.windows.net). Esto es un patron
# conocido y documentado por Microsoft: Azure SQL corta agresivamente conexiones
# cuyo handshake TLS tarda demasiado (proteccion anti "slow-drip"). Se agrava por
# la distancia geografica: el App Service esta en West US 2 y el SQL Server en
# UK South (~9000 km, RTT alto).
#
# Fix inmediato (este script): activar EnableRetryOnFailure de EF Core, que es la
# recomendacion oficial de Microsoft para este tipo de fallo transitorio.
#
# Fix estructural (pendiente, ya anotado en el roadmap de Terraform): migrar el
# SQL Server a West US 2 para eliminar la latencia entre regiones. Este script
# NO hace eso — solo mitiga el sintoma mientras se planifica la migracion.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$infraPath = "Alakai.FestivalManager.Infrastructure/Extensions/InfrastructureDependencyInjectionExtension.cs"

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

$result = Patch-File -Path $infraPath -Description "Activar EnableRetryOnFailure + timeout ampliado en UseSqlServer" -OldString @'
        services.AddDbContext<FestivalManagerDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));
'@ -NewString @'
        services.AddDbContext<FestivalManagerDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(60);
                }));
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar el patch. Pega el contenido actual del archivo para ajustar el anchor." -ForegroundColor Red
    exit 1
}

Write-Host "`nPatch aplicado: EnableRetryOnFailure activo (5 reintentos, hasta 10s de espera entre intentos)." -ForegroundColor Green
Write-Host "Esto mitiga el sintoma (reintenta automaticamente ante un reset transitorio de handshake TLS)," -ForegroundColor Cyan
Write-Host "pero NO elimina la causa de fondo: la distancia entre West US 2 (App Service) y UK South (SQL Server)." -ForegroundColor Cyan
Write-Host "Esto sigue pendiente en el roadmap de Terraform: migrar/recrear el SQL Server en West US 2." -ForegroundColor Yellow