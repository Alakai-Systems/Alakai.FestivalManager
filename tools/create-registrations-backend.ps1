# Fix-Step57-CleanupDuplicates.ps1
#
# BUG REAL en mi propia herramienta: la comprobacion de "ya aplicado" en
# Patch-File fallaba quando el NewString contenia el OldString dentro (patron
# usado en varios pasos de hoy, incluido el Paso 56) - al ejecutar el script
# mas de una vez, volvia a insertar el mismo bloque cada vez, acumulando
# duplicados. Este script limpia los duplicados generados en
# PublicRegistrationApiClient.cs y Register.razor, dejando solo una copia de
# cada cosa.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"

function Collapse-Duplicates {
    param(
        [string]$Path,
        [string]$Block,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow
        return
    }

    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")
    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedBlock = $Block -replace "`r`n", "`n"

    $firstIndex = $normalizedContent.IndexOf($normalizedBlock)

    if ($firstIndex -lt 0) {
        Write-Host "SKIP (bloque no encontrado): $Description" -ForegroundColor Yellow
        return
    }

    $keepEnd = $firstIndex + $normalizedBlock.Length
    $before = $normalizedContent.Substring(0, $keepEnd)
    $after = $normalizedContent.Substring($keepEnd)

    if (-not $after.Contains($normalizedBlock)) {
        Write-Host "SKIP (solo una copia, nada que limpiar): $Description" -ForegroundColor Cyan
        return
    }

    $countBefore = 1
    while ($after.Contains($normalizedBlock)) {
        $after = $after.Replace($normalizedBlock, "")
        $countBefore++
    }

    $result = $before + $after

    if ($usesCrlf) {
        $result = $result -replace "`n", "`r`n"
    }

    Set-Content -Path $Path -Value $result -NoNewline
    Write-Host "OK: $Description ($countBefore copias encontradas -> 1)" -ForegroundColor Green
}

# ── 1. PublicRegistrationApiClient.cs: metodo duplicado ─────────────────────
$methodBlock = @'
    public async Task<PublicEmailLayoutDto?> GetEmailLayoutAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PublicEmailLayoutDto>($"api/public/festivals/email-layout/{editionId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

'@

Collapse-Duplicates -Path "Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs" `
    -Block $methodBlock -Description "GetEmailLayoutAsync duplicado"

# ── 2. PublicRegistrationApiClient.cs: record duplicado ─────────────────────
$recordBlock = @'
public record PublicEmailLayoutDto(string HeaderHtml, string FooterHtml);

'@

Collapse-Duplicates -Path "Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs" `
    -Block $recordBlock -Description "PublicEmailLayoutDto duplicado"

# ── 3. Register.razor: campos duplicados ────────────────────────────────────
$fieldsBlock = @'
    private string? HeaderHtml;
    private string? FooterHtml;
'@

Collapse-Duplicates -Path "Alakai.FestivalManager.Admin/Components/Pages/Register.razor" `
    -Block $fieldsBlock -Description "Campos HeaderHtml/FooterHtml duplicados"

Write-Host "`nLimpieza completa. dotnet build para confirmar que ya compila." -ForegroundColor Green