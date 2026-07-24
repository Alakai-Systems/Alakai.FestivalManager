# Fix-AuthorizeAudit.ps1
#
# Auditoria completa de [Authorize] en la Api. Antes de este script, solo
# 10 de 38 controllers tenian algo de autenticacion - el resto estaban
# totalmente abiertos.
#
# Esquema aplicado:
#
#   SIN TOCAR (publico, sin login - lo llama la web de inscripcion o el
#   propio banco, nunca un admin logueado):
#     - PublicFestivalsController, PublicRegistrationsController
#     - PaymentsController (tiene el webhook de Redsys y los retornos del
#       navegador del participante, no pueden llevar un JWT de admin)
#
#   YA ESTABAN BIEN, NO TOCAR:
#     - AdminImpersonationController (SuperAdmin)
#     - AuthController, UserPanelController ([Authorize] generico)
#     - Los 8 controllers de Produccion (SuperAdmin,Admin,Production)
#
#   NUEVO: [Authorize(Roles = "SuperAdmin,Admin")] - gestion de
#   participantes/festival que Produccion no toca:
#     Registrations, RegistrationFestivalInfo, CompetitionEntries,
#     Competitions, Buses, BusReservations, Accommodations,
#     AccommodationZones, AccommodationBuildings, AccommodationReservations,
#     MealPreferences, DiscountCodes, PassType, Level, Invoices,
#     InvoiceSettings, InvoiceTemplates, Emails, EmailTemplates,
#     EmailLayout, EmailLogs, Analytics, Dashboard, Uploads, Users
#
#   NUEVO: [Authorize(Roles = "SuperAdmin,Admin,Production")] - Produccion
#   los necesita de forma indirecta para sus propios filtros/reports:
#     Editions, Festivals, Reports
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

function Patch-File {
    param([string]$Path, [string]$OldString, [string]$NewString, [string]$Description)
    if (-not (Test-Path $Path)) { Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow; return $false }
    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")
    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"
    if ($normalizedContent.Contains($normalizedNew)) { Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan; return $true }
    if (-not $normalizedContent.Contains($normalizedOld)) { Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow; return $false }
    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$anyFailed = $false
$controllersDir = "Alakai.FestivalManager.Api/Controllers"

# ---------------------------------------------------------------------------
# Grupo: SuperAdmin,Admin
# ---------------------------------------------------------------------------
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
    $ok = Patch-File -Path $path -Description "$($entry.Class): [Authorize(SuperAdmin,Admin)]" -OldString @"
public class $($entry.Class) : ControllerBase
"@ -NewString @"
[Authorize(Roles = "SuperAdmin,Admin")]
public class $($entry.Class) : ControllerBase
"@
    if (-not $ok) { $anyFailed = $true }
}

# ---------------------------------------------------------------------------
# Grupo: SuperAdmin,Admin,Production
# ---------------------------------------------------------------------------
$sharedWithProduction = @(
    @{ File = "EditionController.cs"; Class = "EditionsController" },
    @{ File = "FestivalsController.cs"; Class = "FestivalsController" },
    @{ File = "ReportsController.cs"; Class = "ReportsController" }
)

foreach ($entry in $sharedWithProduction) {
    $path = Join-Path $controllersDir $entry.File
    $ok = Patch-File -Path $path -Description "$($entry.Class): [Authorize(SuperAdmin,Admin,Production)]" -OldString @"
public class $($entry.Class) : ControllerBase
"@ -NewString @"
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class $($entry.Class) : ControllerBase
"@
    if (-not $ok) { $anyFailed = $true }
}

if ($anyFailed) {
    Write-Host "`nAlguno de los controllers no encontro su ancla - revisa los SKIP de arriba." -ForegroundColor Red
}
else {
    Write-Host "`nAuditoria de [Authorize] completa: 25 controllers a SuperAdmin+Admin, 3 compartidos con Produccion (Editions/Festivals/Reports). PublicFestivals, PublicRegistrations y Payments se quedan tal cual (acceso publico necesario)." -ForegroundColor Green
}