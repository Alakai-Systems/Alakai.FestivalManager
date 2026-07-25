# Fix-PortalUrlToLogin.ps1
#
# La variable {{PortalUrl}} de los emails apuntaba a /user-panel/dashboard
# cuando el festival tiene dominio propio - debe apuntar al login
# (/user-panel/login), no al dashboard directamente.
#
# OJO: esto solo arregla la mitad del camino (festivales CON dominio
# propio, donde la URL se construye aqui mismo en el codigo). Cuando el
# festival NO tiene dominio propio, la URL sale de
# _applicationUrlsOptions.PortalUrl, que es un valor de configuracion
# (appsettings.json / variable de entorno), no algo que este en este
# archivo - revisalo aparte y cambialo ahi si tambien apunta a
# /user-panel/dashboard.
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

$path = "Alakai.FestivalManager.Application/Features/Emails/Services/EmailNotificationService.cs"

if (-not (Test-Path $path)) {
    Write-Host "SKIP (archivo no encontrado): $path" -ForegroundColor Yellow
    exit 1
}

$rawContent = Get-Content -Path $path -Raw
$usesCrlf = $rawContent.Contains("`r`n")
$normalizedContent = $rawContent -replace "`r`n", "`n"

$old = @'
    private string BuildPortalUrl(string? customDomain)
    {
        return string.IsNullOrWhiteSpace(customDomain)
            ? _applicationUrlsOptions.PortalUrl
            : $"https://{customDomain}/user-panel/dashboard";
    }
'@ -replace "`r`n", "`n"

$new = @'
    private string BuildPortalUrl(string? customDomain)
    {
        return string.IsNullOrWhiteSpace(customDomain)
            ? _applicationUrlsOptions.PortalUrl
            : $"https://{customDomain}/user-panel/login";
    }
'@ -replace "`r`n", "`n"

if ($normalizedContent.Contains($new) -and -not $normalizedContent.Contains($old)) {
    Write-Host "SKIP (ya aplicado)" -ForegroundColor Cyan
}
elseif ($normalizedContent.Contains($old)) {
    $updatedNormalized = $normalizedContent.Replace($old, $new)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $path -Value $updatedFinal -NoNewline
    Write-Host "OK: PortalUrl (dominio propio) ahora apunta a /user-panel/login" -ForegroundColor Green
}
else {
    Write-Host "SKIP (anchor no encontrado)" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nIMPORTANTE: esto solo cubre festivales con dominio propio. Para los que NO tienen dominio propio, la URL sale de la configuracion (ApplicationUrls:PortalUrl en appsettings/variables de entorno) - revisa ese valor aparte, no esta en este archivo." -ForegroundColor Yellow