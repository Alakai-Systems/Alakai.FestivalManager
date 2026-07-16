# Fix-Step5b-RequestDtoBug.ps1
#
# BUG REAL encontrado: existen DOS clases CreateFestivalRequest/UpdateFestivalRequest
# con el mismo nombre - una en Admin (arma el JSON) y otra en Application (la que
# la API realmente recibe, via [FromBody]). El Paso 4a solo parcheo la de Admin;
# la de Application nunca tuvo GoogleAnalyticsPropertyId/FaviconUrl, asi que esos
# dos campos se descartaban en silencio al deserializar el JSON entrante, antes
# de llegar siquiera a AutoMapper.
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

# --- Application/.../Contracts/Requests/CreateFestivalRequest.cs ---
$results += Patch-File -Path "Alakai.FestivalManager.Application/Features/Festivals/Contracts/Requests/CreateFestivalRequest.cs" `
    -Description "Application CreateFestivalRequest: anadir GA4/FaviconUrl (el bug real)" `
    -OldString @'
    public string? TermsUrl { get; set; }
    public FestivalModule EnabledModules { get; set; } = FestivalModule.Competitions;
'@ `
    -NewString @'
    public string? TermsUrl { get; set; }
    public string? GoogleAnalyticsPropertyId { get; set; }
    public string? FaviconUrl { get; set; }
    public FestivalModule EnabledModules { get; set; } = FestivalModule.Competitions;
'@

# --- Application/.../Contracts/Requests/UpdateFestivalRequest.cs ---
$results += Patch-File -Path "Alakai.FestivalManager.Application/Features/Festivals/Contracts/Requests/UpdateFestivalRequest.cs" `
    -Description "Application UpdateFestivalRequest: anadir GA4/FaviconUrl (el bug real)" `
    -OldString @'
    public string? TermsUrl { get; set; }
    public bool IsActive { get; set; }
    public FestivalModule EnabledModules { get; set; }
'@ `
    -NewString @'
    public string? TermsUrl { get; set; }
    public string? GoogleAnalyticsPropertyId { get; set; }
    public string? FaviconUrl { get; set; }
    public bool IsActive { get; set; }
    public FestivalModule EnabledModules { get; set; }
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nBug real corregido. dotnet build, luego commit+push." -ForegroundColor Green
Write-Host "Esta vez Favicon/GA4 deberian guardarse tanto en Create como en Edit." -ForegroundColor Green