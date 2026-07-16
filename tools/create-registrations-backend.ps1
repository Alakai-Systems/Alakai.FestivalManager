# Fix-Step10-DocumentFieldInRegisterForm.ps1
#
# El backend ya soportaba DocumentNumber/DocumentCountry de punta a punta
# (Registration entity, CreateRegistrationCommand, y el CreateRegistrationRequest
# del Admin) - solo faltaba mostrarlo en el formulario publico. Se anade justo
# despues de City, con el mismo estilo que el resto de campos, y sus
# traducciones en EN/ES/FR/CA.
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

# --- 1. Register.razor: anadir los 2 campos tras City ---
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Register.razor" `
    -Description "Anadir campos Document Number / Document Country tras City" `
    -OldString @'
                                <div>
                                    <label class="block text-xs text-muted" style="margin-bottom:0.375rem;">@T.Get("city")</label>
                                    <input class="form-input" @bind="Form.City" placeholder="@T.Get("city")" />
                                </div>
                                <div>
                                    <label class="block text-xs text-muted" style="margin-bottom:0.375rem;">@T.Get("group_code")</label>
'@ `
    -NewString @'
                                <div>
                                    <label class="block text-xs text-muted" style="margin-bottom:0.375rem;">@T.Get("city")</label>
                                    <input class="form-input" @bind="Form.City" placeholder="@T.Get("city")" />
                                </div>
                                <div>
                                    <label class="block text-xs text-muted" style="margin-bottom:0.375rem;">@T.Get("document_number")</label>
                                    <input class="form-input" @bind="Form.DocumentNumber" placeholder="@T.Get("document_number")" />
                                </div>
                                <div>
                                    <label class="block text-xs text-muted" style="margin-bottom:0.375rem;">@T.Get("document_country")</label>
                                    <select class="form-select" @bind="Form.DocumentCountry">
                                        <option value="">-</option>
                                        @foreach (string country in Countries)
                                        {
                                            <option value="@country">@country</option>
                                        }
                                    </select>
                                </div>
                                <div>
                                    <label class="block text-xs text-muted" style="margin-bottom:0.375rem;">@T.Get("group_code")</label>
'@

# --- 2. Traducciones: en.json ---
$results += Patch-File -Path "Alakai.FestivalManager.Admin/wwwroot/i18n/en.json" `
    -Description "en.json: anadir document_number / document_country" `
    -OldString @'
  "city": "City",
'@ `
    -NewString @'
  "city": "City",
  "document_number": "Document Number",
  "document_country": "Document Issuing Country",
'@

# --- 3. Traducciones: es.json ---
$results += Patch-File -Path "Alakai.FestivalManager.Admin/wwwroot/i18n/es.json" `
    -Description "es.json: anadir document_number / document_country" `
    -OldString @'
  "city": "Ciudad",
'@ `
    -NewString @'
  "city": "Ciudad",
  "document_number": "Número de documento",
  "document_country": "País de emisión del documento",
'@

# --- 4. Traducciones: fr.json ---
$results += Patch-File -Path "Alakai.FestivalManager.Admin/wwwroot/i18n/fr.json" `
    -Description "fr.json: anadir document_number / document_country" `
    -OldString @'
  "city": "Ville",
'@ `
    -NewString @'
  "city": "Ville",
  "document_number": "Numéro de document",
  "document_country": "Pays de délivrance du document",
'@

# --- 5. Traducciones: ca.json ---
$results += Patch-File -Path "Alakai.FestivalManager.Admin/wwwroot/i18n/ca.json" `
    -Description "ca.json: anadir document_number / document_country" `
    -OldString @'
  "city": "Ciutat",
'@ `
    -NewString @'
  "city": "Ciutat",
  "document_number": "Número de document",
  "document_country": "País d'emissió del document",
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nCampo de documentacion anadido en las 4 idiomas. dotnet build para confirmar." -ForegroundColor Green