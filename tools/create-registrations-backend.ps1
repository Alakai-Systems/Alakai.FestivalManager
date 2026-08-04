<#
.SINOPSIS
  Segundo intento, con el motivo correcto esta vez: Azure App Service
  bloquea CUALQUIER Application Setting cuyo nombre empiece por "Azure"
  (reservado para sus propios ajustes internos, como AzureWebJobsStorage).
  No era por terminar en "ConnectionString" - de hecho AzureBlobStorage__ConnString
  y hasta AzureBlobStorage__ContainerName (que no termina en nada raro)
  tambien se rechazaron, y las ~29 variables que SI tienes funcionando
  ninguna empieza por "Azure". Ese es el patron real.

  Este script quita el prefijo "Azure" de la seccion de configuracion:
  "AzureBlobStorage" -> "BlobStorage". Los nombres de las propiedades
  (ConnString, ContainerName) NO cambian, ni la clase C# AzureBlobStorageOptions
  (el nombre de la clase no afecta al nombre del Application Setting, solo
  el string de la seccion si).

  Resultado: los Application Settings pasan a llamarse
  BlobStorage__ConnString y BlobStorage__ContainerName (sin "Azure").

  Archivos que modifica:
    - Api\appsettings.json                                    (seccion "AzureBlobStorage" -> "BlobStorage")
    - Infrastructure\Extensions\InfrastructureDependencyInjectionExtension.cs  (lee de "BlobStorage")

  Idempotente.

.USO
  Ejecutar desde la raiz del repo:
    .\19-rename-blobstorage-section-drop-azure-prefix.ps1

  Despues: dotnet build, redeploy, y en el Portal usa BlobStorage__ConnString
  y BlobStorage__ContainerName (no AzureBlobStorage__...).
#>

function Patch-File {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] [string]$OldString,
        [Parameter(Mandatory = $true)] [string]$NewString,
        [string]$Description = ""
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "SKIP: archivo no encontrado -> $Path" -ForegroundColor Yellow
        return
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $rawContent = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    $usesCrlf = $rawContent.Contains("`r`n")

    $content = $rawContent -replace "`r`n", "`n"
    $oldNormalized = $OldString -replace "`r`n", "`n"
    $newNormalized = $NewString -replace "`r`n", "`n"

    if ($content.Contains($newNormalized)) {
        Write-Host "SKIP: ya aplicado -> $Path ($Description)" -ForegroundColor DarkGray
        return
    }

    $occurrences = ([regex]::Matches($content, [regex]::Escape($oldNormalized))).Count

    if ($occurrences -eq 0) {
        Write-Host "SKIP: anchor no encontrado -> $Path ($Description)" -ForegroundColor Red
        return
    }

    if ($occurrences -gt 1) {
        Write-Host "SKIP: anchor ambiguo, aparece $occurrences veces -> $Path ($Description)" -ForegroundColor Red
        return
    }

    $newContent = $content.Replace($oldNormalized, $newNormalized)

    if ($usesCrlf) {
        $newContent = $newContent -replace "`n", "`r`n"
    }

    [System.IO.File]::WriteAllText($Path, $newContent, $utf8NoBom)
    Write-Host "OK: aplicado -> $Path ($Description)" -ForegroundColor Green
}

$ErrorActionPreference = "Stop"

$ApiAppSettings = ".\Alakai.FestivalManager.Api\appsettings.json"
$DiExtPath      = ".\Alakai.FestivalManager.Infrastructure\Extensions\InfrastructureDependencyInjectionExtension.cs"

# ----------------------------------------------------------------------------
# 1. appsettings.json: seccion "AzureBlobStorage" -> "BlobStorage"
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ApiAppSettings `
    -Description "seccion AzureBlobStorage -> BlobStorage" `
    -OldString @'
  "AzureBlobStorage": {
    "ConnString": "",
    "ContainerName": "uploads"
  },
'@ `
    -NewString @'
  "BlobStorage": {
    "ConnString": "",
    "ContainerName": "uploads"
  },
'@

# ----------------------------------------------------------------------------
# 2. InfrastructureDependencyInjectionExtension.cs: lee de la seccion "BlobStorage"
# ----------------------------------------------------------------------------
Patch-File `
    -Path $DiExtPath `
    -Description "lee de la seccion BlobStorage" `
    -OldString @'
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection("AzureBlobStorage"));

        string azureBlobConnectionString = configuration["AzureBlobStorage:ConnString"] ?? string.Empty;
'@ `
    -NewString @'
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection("BlobStorage"));

        string azureBlobConnectionString = configuration["BlobStorage:ConnString"] ?? string.Empty;
'@

Write-Host ""
Write-Host "Deberias ver 2 lineas 'OK: aplicado'." -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO:" -ForegroundColor Cyan
Write-Host "  1. dotnet build (debe compilar limpio)." -ForegroundColor Cyan
Write-Host "  2. Redeploy." -ForegroundColor Cyan
Write-Host "  3. En el Portal (Configuracion de aplicacion), borra las variables" -ForegroundColor Cyan
Write-Host "     AzureBlobStorage__ConnString / AzureBlobStorage__ContainerName si llegaron" -ForegroundColor Cyan
Write-Host "     a crearse a medias, y anade en su lugar:" -ForegroundColor Cyan
Write-Host "       Nombre: BlobStorage__ConnString      Valor: la cadena de conexion" -ForegroundColor Cyan
Write-Host "       Nombre: BlobStorage__ContainerName   Valor: uploads" -ForegroundColor Cyan