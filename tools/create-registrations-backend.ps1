# Fix-Step17-EmailNotificationApiClientHardcodedUrl.ps1
#
# BUG REAL (preexistente, no de hoy): EmailNotificationApiClient tenia
# "https://localhost:7157/" hardcodeado como BaseAddress, en vez de leer
# ApiSettings:BaseUrl de la configuracion como TODOS los demas clientes.
# En produccion, cualquier llamada a traves de este cliente intenta conectar
# a localhost:7157 (que no existe en el contenedor de Azure), fallando con
# "Cannot assign requested address".
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"

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

    if ($normalizedContent.Contains($normalizedNew) -and -not $normalizedContent.Contains($normalizedOld)) {
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

$result = Patch-File -Path "Alakai.FestivalManager.Admin/Extensions/ApplicationDependencyInjectionExtension.cs" `
    -Description "EmailNotificationApiClient: leer ApiSettings:BaseUrl en vez de localhost hardcodeado" `
    -OldString @'
        services.AddHttpClient<EmailNotificationApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7157/");
        });
'@ `
    -NewString @'
        services.AddHttpClient<EmailNotificationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual alrededor de EmailNotificationApiClient." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green