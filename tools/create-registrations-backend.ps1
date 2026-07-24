# Fix-RevertAuthorizeAudit-v2.ps1
#
# La v1 tenia un fallo: su comprobacion de "ya aplicado" hacia
# content.Contains(NewString), y como NewString ("public class X :
# ControllerBase") es literalmente parte de OldString
# ("[Authorize(...)]\npublic class X : ControllerBase"), SIEMPRE decia
# "ya aplicado" sin quitar el atributo de verdad. Por eso salio todo SKIP.
#
# Esta version comprueba directamente si el atributo [Authorize(Roles=...)]
# esta presente justo antes de la clase, y si lo esta, lo quita - sin el
# atajo defectuoso de antes.
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

function Remove-Attribute {
    param([string]$Path, [string]$AttributeLine, [string]$ClassLine, [string]$Description)

    if (-not (Test-Path $Path)) { Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow; return }

    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")
    $normalizedContent = $rawContent -replace "`r`n", "`n"

    $combined = "$AttributeLine`n$ClassLine"
    $count = ([regex]::Matches($normalizedContent, [regex]::Escape($combined))).Count

    if ($count -eq 0) {
        # Puede que ya se haya quitado de verdad, o que nunca se aplicara.
        if ($normalizedContent.Contains($ClassLine)) {
            Write-Host "SKIP (el atributo ya no esta - correcto): $Description" -ForegroundColor Cyan
        }
        else {
            Write-Host "SKIP (ni el atributo ni la clase encontrados - revisa a mano): $Description" -ForegroundColor Yellow
        }
        return
    }

    if ($count -gt 1) {
        Write-Host "SKIP (aparece $count veces, no es unico - revisa a mano): $Description" -ForegroundColor Yellow
        return
    }

    $updatedNormalized = $normalizedContent.Replace($combined, $ClassLine)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
}

$controllersDir = "Alakai.FestivalManager.Api/Controllers"

$adminOnly = @(
    @{ File = "RegistrationsController.cs"; Class = "RegistrationsController" },
    @{ File = "RegistrationFestivalInfoController.cs"; Class = "RegistrationFestivalInfoController" },
    @{ File = "CompetitionEntriesController.cs"; Class = "CompetitionEntriesController" },
    @{ File = "CompetitionsController.cs"; Class = "CompetitionsController" },
    @{ File = "BusesController.cs"; Class = "BusesController" },
    @{ File = "BusReservationsController.cs"; Class = "BusReservationsController" },
    @{ File = "AccommodationsController.cs"; Class = "AccommodationsController" },
    @{ File = "AccommodationZonesController.cs"; Class = "AccommodationZonesController" },
    @{ File = "AccommodationBuildingsController.cs"; Class = "AccommodationBuildingsController" },
    @{ File = "AccommodationReservationsController.cs"; Class = "AccommodationReservationsController" },
    @{ File = "MealPreferencesController.cs"; Class = "MealPreferencesController" },
    @{ File = "DiscountCodesController.cs"; Class = "DiscountCodesController" },
    @{ File = "PassTypeController.cs"; Class = "PassTypesController" },
    @{ File = "LevelController.cs"; Class = "LevelsController" },
    @{ File = "InvoicesController.cs"; Class = "InvoicesController" },
    @{ File = "InvoiceSettingsController.cs"; Class = "InvoiceSettingsController" },
    @{ File = "InvoiceTemplatesController.cs"; Class = "InvoiceTemplatesController" },
    @{ File = "EmailsController.cs"; Class = "EmailsController" },
    @{ File = "EmailTemplatesController.cs"; Class = "EmailTemplatesController" },
    @{ File = "EmailLayoutController.cs"; Class = "EmailLayoutController" },
    @{ File = "EmailLogsController.cs"; Class = "EmailLogsController" },
    @{ File = "AnalyticsController.cs"; Class = "AnalyticsController" },
    @{ File = "DashboardController.cs"; Class = "DashboardController" },
    @{ File = "UploadsController.cs"; Class = "UploadsController" },
    @{ File = "UsersController.cs"; Class = "UsersController" }
)

foreach ($entry in $adminOnly) {
    $path = Join-Path $controllersDir $entry.File
    Remove-Attribute -Path $path -AttributeLine '[Authorize(Roles = "SuperAdmin,Admin")]' -ClassLine "public class $($entry.Class) : ControllerBase" -Description "$($entry.Class): quitar [Authorize(SuperAdmin,Admin)]"
}

$sharedWithProduction = @(
    @{ File = "EditionController.cs"; Class = "EditionsController" },
    @{ File = "FestivalsController.cs"; Class = "FestivalsController" },
    @{ File = "ReportsController.cs"; Class = "ReportsController" }
)

foreach ($entry in $sharedWithProduction) {
    $path = Join-Path $controllersDir $entry.File
    Remove-Attribute -Path $path -AttributeLine '[Authorize(Roles = "SuperAdmin,Admin,Production")]' -ClassLine "public class $($entry.Class) : ControllerBase" -Description "$($entry.Class): quitar [Authorize(SuperAdmin,Admin,Production)]"
}

Write-Host "`nRevisa arriba: cada linea debe decir OK o 'el atributo ya no esta - correcto'. Si alguna dice 'revisa a mano', pegame el contenido de ese archivo concreto (con ese si necesito verlo, es el unico caso donde no puedo hacerlo a ciegas)." -ForegroundColor Green