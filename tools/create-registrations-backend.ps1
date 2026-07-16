# Fix-Step15-EmailTemplateLanguageEditBug.ps1
#
# BUG REAL: OpenEditModal nunca copiaba template.Language al formulario, asi
# que el desplegable siempre mostraba "English" por defecto al editar
# cualquier plantilla, sin importar su idioma real. Si guardabas sin corregir
# el desplegable a mano, se sobreescribia el idioma real de la plantilla.
#
# Fix:
#   1) OpenEditModal ahora si copia el idioma real de la plantilla.
#   2) El desplegable se BLOQUEA (disabled) mientras se edita una plantilla
#      existente - evita el caso de "cambio idioma sin querer y guardo,
#      sobreescribiendo otro idioma". Para crear la version en otro idioma,
#      se sigue usando "New Template" (el desplegable ahi si esta libre).
#   3) Se anade la opcion de Catalan (ca) que faltaba en el desplegable.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Pages/Emails.razor"

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

# --- 1. OpenEditModal: copiar el idioma real ---
$results += Patch-File -Path $path -Description "OpenEditModal: cargar el idioma real de la plantilla" -OldString @'
    private void OpenEditModal(EmailTemplateDto template)
    {
        editingTemplate = template;
        formModel = new EmailTemplateFormModel
        {
            EditionId = template.EditionId,
            TemplateKey = template.TemplateKey,
            Name = template.Name,
            Subject = template.Subject,
            BodyHtml = template.BodyHtml,
            BodyText = template.BodyText,
            IsSystem = template.IsSystem,
            IsActive = template.IsActive
        };
        showModal = true;
    }
'@ -NewString @'
    private void OpenEditModal(EmailTemplateDto template)
    {
        editingTemplate = template;
        formModel = new EmailTemplateFormModel
        {
            EditionId = template.EditionId,
            TemplateKey = template.TemplateKey,
            Name = template.Name,
            Subject = template.Subject,
            BodyHtml = template.BodyHtml,
            BodyText = template.BodyText,
            IsSystem = template.IsSystem,
            IsActive = template.IsActive,
            Language = template.Language
        };
        showModal = true;
    }
'@

# --- 2. Bloquear el desplegable de idioma mientras se edita + anadir Catalan ---
$results += Patch-File -Path $path -Description "Bloquear Language en modo edicion y anadir opcion de Catalan" -OldString @'
                                <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Language</label>
                                <select class="form-select" @bind="formModel.Language">
                                    <option value="en">English</option>
                                    <option value="es">Español</option>
                                    <option value="fr">Français</option>
                                </select>
'@ -NewString @'
                                <label class="block text-sm text-black/60 dark:text-white/60 mb-1">Language</label>
                                <select class="form-select" @bind="formModel.Language" disabled="@(editingTemplate is not null)">
                                    <option value="en">English</option>
                                    <option value="es">Español</option>
                                    <option value="fr">Français</option>
                                    <option value="ca">Català</option>
                                </select>
                                @if (editingTemplate is not null)
                                {
                                    <p class="mt-1 text-xs text-black/40 dark:text-white/40">Use "New Template" to create a version in another language.</p>
                                }
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nBug de idioma en Email Templates corregido. dotnet build para confirmar." -ForegroundColor Green