# Fix-Step50-ResendFromEmailLogs.ps1
# Anade un boton "Resend" en Email Logs, reutilizando el mismo endpoint que
# ya usan los iconos de reenvio de Registrations.razor. Se deshabilita cuando
# el log no tiene RegistrationId (por ejemplo, PasswordReset, que va por
# usuario, no por registro). El Preview existente no se toca.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Pages/EmailLogs.razor"

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

$results = @()

# ── 1. Inyectar EmailNotificationApiClient ──────────────────────────────────
$results += Patch-File -Path $path -Description "Inyectar EmailNotificationApiClient" -OldString @'
@inject ActiveFestivalState ActiveFestivalState
'@ -NewString @'
@inject ActiveFestivalState ActiveFestivalState
@inject EmailNotificationApiClient EmailNotificationApiClient
'@

# ── 2. Banner de exito, junto al de error ya existente ──────────────────────
$results += Patch-File -Path $path -Description "Anadir banner de exito para el reenvio" -OldString @'
        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
        }
'@ -NewString @'
        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
        }

        @if (!string.IsNullOrWhiteSpace(resendMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">@resendMessage</div>
        }
'@

# ── 3. Boton de Resend junto al de Preview ──────────────────────────────────
$results += Patch-File -Path $path -Description "Anadir boton Resend" -OldString @'
                                        <button type="button" class="text-black dark:text-white/80" title="Preview" @onclick="() => OpenPreview(log)">
                                            <i class="ri-eye-line text-lg"></i>
                                        </button>
'@ -NewString @'
                                        <button type="button" class="text-black dark:text-white/80" title="Preview" @onclick="() => OpenPreview(log)">
                                            <i class="ri-eye-line text-lg"></i>
                                        </button>
                                        <button type="button" class="text-black dark:text-white/80 disabled:opacity-30" title="@(log.RegistrationId.HasValue ? "Resend" : "Cannot resend - not linked to a registration")" disabled="@(!log.RegistrationId.HasValue || resendingLogId == log.Id)" @onclick="() => ResendAsync(log)">
                                            <i class="@(resendingLogId == log.Id ? "ri-loader-4-line animate-spin" : "ri-send-plane-line") text-lg"></i>
                                        </button>
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 4. @code: estado + metodo ResendAsync ───────────────────────────────────
$results2 = Patch-File -Path $path -Description "Anadir el estado y el metodo ResendAsync" -OldString @'
    private EmailLogDto? previewingLog;
'@ -NewString @'
    private EmailLogDto? previewingLog;
    private string? resendMessage;
    private Guid? resendingLogId;

    private async Task ResendAsync(EmailLogDto log)
    {
        if (!log.RegistrationId.HasValue)
        {
            return;
        }

        resendingLogId = log.Id;
        resendMessage = null;
        errorMessage = null;

        try
        {
            await EmailNotificationApiClient.SendRegistrationEmailAsync(log.RegistrationId.Value, log.TemplateKey);
            resendMessage = $"Email resent to {log.RecipientEmail}.";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            resendingLogId = null;
        }
    }
'@

if (-not $results2) {
    Write-Host "`nNo se pudo aplicar el patch del @code. Puede que el nombre del metodo de carga no sea LoadLogsAsync - avisame." -ForegroundColor Red
    exit 1
}

Write-Host "`nBoton de reenviar anadido, Preview sin tocar. dotnet build para confirmar." -ForegroundColor Green