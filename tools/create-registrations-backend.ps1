# Fix-Step51-ModalWidthMatchesApp.ps1
# Los modales de vista previa de email (Pasos 47 y 49) usaban 95vw - mucho mas
# ancho que el resto de modales de la app. El resto de modales de 2 columnas
# (Festivals, Discount Codes, Payment Settings) usan max-w-lg de forma
# consistente. Se ajustan ambos visores a ese mismo ancho - como el email ya
# es responsive (EmailShellWidth + max-width:100%), deberia encogerse bien
# dentro de ese contenedor mas estrecho, igual que en movil.
#
# Ejecutar DESPUES de Fix-Step47 y Fix-Step49.
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

$results = @()

# ── 1. EmailLogs.razor: ajustar el modal de Preview ─────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/EmailLogs.razor" `
    -Description "EmailLogs: ajustar el modal a max-w-lg (igual que el resto de la app)" -OldString @'
    <div class="fixed inset-0 bg-black/60 z-[999]">
        <div class="flex items-center justify-center h-screen p-4">
            <div class="relative mx-auto overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder flex flex-col" style="width:95vw; height:95vh;">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder shrink-0">
                    <div>
                        <h3 class="text-lg font-semibold text-black dark:text-white">@previewingLog.Subject</h3>
                        <p class="text-xs text-black/50 dark:text-white/60">@previewingLog.RecipientEmail - @previewingLog.TemplateKey</p>
                    </div>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="ClosePreview"><i class="ri-close-line text-2xl"></i></button>
                </div>

                <div class="flex-1 bg-gray-100 p-2">
                    <iframe srcdoc="@previewingLog.BodyHtml" style="width:100%; height:100%; border:1px solid #e5e7eb; border-radius:6px; background:#fff;"></iframe>
                </div>
            </div>
        </div>
    </div>
'@ -NewString @'
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <div>
                        <h3 class="text-lg font-semibold text-black dark:text-white">@previewingLog.Subject</h3>
                        <p class="text-xs text-black/50 dark:text-white/60">@previewingLog.RecipientEmail - @previewingLog.TemplateKey</p>
                    </div>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="ClosePreview"><i class="ri-close-line text-2xl"></i></button>
                </div>

                <div class="p-2 bg-gray-100">
                    <iframe srcdoc="@previewingLog.BodyHtml" style="width:100%; height:75vh; border:1px solid #e5e7eb; border-radius:6px; background:#fff;"></iframe>
                </div>
            </div>
        </div>
    </div>
'@

# ── 2. Emails.razor: ajustar el modal de Preview de plantilla ───────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Emails.razor" `
    -Description "Emails: ajustar el modal a max-w-lg (igual que el resto de la app)" -OldString @'
    <div class="fixed inset-0 bg-black/60 z-[999]">
        <div class="flex items-center justify-center h-screen p-4">
            <div class="relative mx-auto overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder flex flex-col" style="width:95vw; height:95vh;">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder shrink-0">
                    <div>
                        <h3 class="text-lg font-semibold text-black dark:text-white">@previewSubject</h3>
                        <p class="text-xs text-black/50 dark:text-white/60">Design preview - sample data, not a real send</p>
                    </div>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="ClosePreview"><i class="ri-close-line text-2xl"></i></button>
                </div>

                <div class="flex-1 bg-gray-100 p-2">
                    @if (previewHtml is null)
                    {
                        <p class="text-center text-black/50 mt-10">Loading...</p>
                    }
                    else
                    {
                        <iframe srcdoc="@previewHtml" style="width:100%; height:100%; border:1px solid #e5e7eb; border-radius:6px; background:#fff;"></iframe>
                    }
                </div>
            </div>
        </div>
    </div>
'@ -NewString @'
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <div>
                        <h3 class="text-lg font-semibold text-black dark:text-white">@previewSubject</h3>
                        <p class="text-xs text-black/50 dark:text-white/60">Design preview - sample data, not a real send</p>
                    </div>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="ClosePreview"><i class="ri-close-line text-2xl"></i></button>
                </div>

                <div class="p-2 bg-gray-100">
                    @if (previewHtml is null)
                    {
                        <p class="text-center text-black/50 py-10">Loading...</p>
                    }
                    else
                    {
                        <iframe srcdoc="@previewHtml" style="width:100%; height:75vh; border:1px solid #e5e7eb; border-radius:6px; background:#fff;"></iframe>
                    }
                </div>
            </div>
        </div>
    </div>
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nAmbos visores ajustados a max-w-lg (mismo ancho que el resto de modales de la app). dotnet build para confirmar." -ForegroundColor Green