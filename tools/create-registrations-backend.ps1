# Fix-Step41-UserPanelMissingIncludes.ps1
# BUG REAL: GetLatestRegistrationByUserIdAsync solo incluia PassType y Level -
# nunca Edition ni Festival. Por eso EditionName y FaviconUrl del Paso 37/38
# siempre salian null, sin importar nada del mecanismo de HeadContent - los
# datos nunca llegaban a cargarse.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Infrastructure/Repositories/UserPanelRepository.cs"

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

$result = Patch-File -Path $path -Description "Anadir Include de Edition->Festival a GetLatestRegistrationByUserIdAsync" -OldString @'
        return await _context.Registrations
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
'@ -NewString @'
        return await _context.Registrations
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.Edition).ThenInclude(e => e.Festival)
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual de GetLatestRegistrationByUserIdAsync." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Ahora EditionName y FaviconUrl del panel de usuario deberian rellenarse de verdad." -ForegroundColor Green