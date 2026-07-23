# Fix-Step81-ImpersonationShowError.ps1
# Los metodos ImpersonateAsync (Registrations.razor y AccommodationOperations.razor)
# asignaban el mensaje de error directamente en vez de usar ShowError, que ya
# existe en ambos archivos con su auto-cierre a los 3.5s. Se corrige para que
# use el mismo patron que el resto de la pagina.
#
# Ejecutar DESPUES de Fix-Step78 (y preferiblemente Fix-Step80 tambien).
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

# ── Registrations.razor ──────────────────────────────────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor" `
    -Description "Registrations: ImpersonateAsync usa ShowError" -OldString @'
    private async Task ImpersonateAsync(Guid registrationId)
    {
        try
        {
            string? token = await ImpersonationApiClient.GetTokenForRegistrationAsync(registrationId);

            if (!string.IsNullOrWhiteSpace(token))
            {
                await JsRuntime.InvokeVoidAsync("open", $"/impersonate?token={Uri.EscapeDataString(token)}", "_blank");
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }
'@ -NewString @'
    private async Task ImpersonateAsync(Guid registrationId)
    {
        try
        {
            string? token = await ImpersonationApiClient.GetTokenForRegistrationAsync(registrationId);

            if (!string.IsNullOrWhiteSpace(token))
            {
                await JsRuntime.InvokeVoidAsync("open", $"/impersonate?token={Uri.EscapeDataString(token)}", "_blank");
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }
'@

# ── AccommodationOperations.razor ────────────────────────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/AccommodationOperations.razor" `
    -Description "Accommodation: ImpersonateAsync usa ShowError" -OldString @'
    private async Task ImpersonateAsync(Guid registrationId)
    {
        try
        {
            string? token = await ImpersonationApiClient.GetTokenForRegistrationAsync(registrationId);

            if (!string.IsNullOrWhiteSpace(token))
            {
                await JsRuntime.InvokeVoidAsync("open", $"/impersonate?token={Uri.EscapeDataString(token)}", "_blank");
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }
'@ -NewString @'
    private async Task ImpersonateAsync(Guid registrationId)
    {
        try
        {
            string? token = await ImpersonationApiClient.GetTokenForRegistrationAsync(registrationId);

            if (!string.IsNullOrWhiteSpace(token))
            {
                await JsRuntime.InvokeVoidAsync("open", $"/impersonate?token={Uri.EscapeDataString(token)}", "_blank");
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido en los dos archivos. dotnet build para confirmar." -ForegroundColor Green