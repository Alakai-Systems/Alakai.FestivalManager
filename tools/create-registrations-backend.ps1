# Fix-ApplyAuthorizeAudit-v2.ps1
#
# Vuelve a poner [Authorize(Roles = "...")] en los 28 controllers, ahora que
# el motivo real de los 401 esta arreglado (Fix-AttachAuthToApiClients.ps1
# ya deberia estar aplicado - si no lo esta, aplica ese PRIMERO o esto te
# vuelve a dejar en 401).
#
# Incluye tambien el [AllowAnonymous] en RegistrationsController.Create
# (el formulario publico de inscripcion), en el mismo script para no
# repetir el fallo de aplicarlos por separado y dejar una ventana rota.
#
# Esquema (igual que la primera vez, verificado ahora contra tu codigo real):
#
#   [Authorize(Roles = "SuperAdmin,Admin")]:
#     Registrations (salvo Create, que es publico), RegistrationFestivalInfo,
#     CompetitionEntries, Competitions, Buses, BusReservations,
#     Accommodations, AccommodationZones, AccommodationBuildings,
#     AccommodationReservations, MealPreferences, DiscountCodes, PassType,
#     Level, Invoices, InvoiceSettings, InvoiceTemplates, Emails,
#     EmailTemplates, EmailLayout, EmailLogs, Analytics, Dashboard,
#     Uploads, Users
#
#   [Authorize(Roles = "SuperAdmin,Admin,Production")]:
#     Editions, Festivals, Reports
#
#   Sin tocar (publico / pagos, y lo que ya estaba bien):
#     PublicFestivalsController, PublicRegistrationsController,
#     PaymentsController, AdminImpersonationController, AuthController,
#     UserPanelController, los 8 controllers de Produccion
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

function Add-Attribute {
    param([string]$Path, [string]$ClassLine, [string]$AttributeLine, [string]$Description)

    if (-not (Test-Path $Path)) { Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow; return }

    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")
    $normalizedContent = $rawContent -replace "`r`n", "`n"

    $combined = "$AttributeLine`n$ClassLine"

    if ($normalizedContent.Contains($combined)) {
        Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan
        return
    }

    $count = ([regex]::Matches($normalizedContent, [regex]::Escape($ClassLine))).Count
    if ($count -ne 1) {
        Write-Host "SKIP (la clase aparece $count veces, no es unico - revisa a mano): $Description" -ForegroundColor Yellow
        return
    }

    $updatedNormalized = $normalizedContent.Replace($ClassLine, $combined)
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
    Add-Attribute -Path $path -ClassLine "public class $($entry.Class) : ControllerBase" -AttributeLine '[Authorize(Roles = "SuperAdmin,Admin")]' -Description "$($entry.Class): [Authorize(SuperAdmin,Admin)]"
}

$sharedWithProduction = @(
    @{ File = "EditionController.cs"; Class = "EditionsController" },
    @{ File = "FestivalsController.cs"; Class = "FestivalsController" },
    @{ File = "ReportsController.cs"; Class = "ReportsController" }
)

foreach ($entry in $sharedWithProduction) {
    $path = Join-Path $controllersDir $entry.File
    Add-Attribute -Path $path -ClassLine "public class $($entry.Class) : ControllerBase" -AttributeLine '[Authorize(Roles = "SuperAdmin,Admin,Production")]' -Description "$($entry.Class): [Authorize(SuperAdmin,Admin,Production)]"
}

# ---------------------------------------------------------------------------
# AllowAnonymous en el registro publico (en el mismo script, no aparte)
# ---------------------------------------------------------------------------
$regPath = Join-Path $controllersDir "RegistrationsController.cs"

if (Test-Path $regPath) {
    $rawContent = Get-Content -Path $regPath -Raw
    $usesCrlf = $rawContent.Contains("`r`n")
    $normalizedContent = $rawContent -replace "`r`n", "`n"

    $old = "    [HttpPost]`n    public async Task<IActionResult> Create([FromBody] CreateRegistrationRequest request, CancellationToken cancellationToken)"
    $new = "    [HttpPost]`n    [AllowAnonymous]`n    public async Task<IActionResult> Create([FromBody] CreateRegistrationRequest request, CancellationToken cancellationToken)"

    if ($normalizedContent.Contains($new)) {
        Write-Host "SKIP (ya aplicado): RegistrationsController.Create: [AllowAnonymous]" -ForegroundColor Cyan
    }
    elseif ($normalizedContent.Contains($old)) {
        $updatedNormalized = $normalizedContent.Replace($old, $new)
        $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
        Set-Content -Path $regPath -Value $updatedFinal -NoNewline
        Write-Host "OK: RegistrationsController.Create: [AllowAnonymous]" -ForegroundColor Green
    }
    else {
        Write-Host "SKIP (anchor no encontrado - revisa a mano): RegistrationsController.Create: [AllowAnonymous]" -ForegroundColor Yellow
    }
}

Write-Host "`nAuthorize reaplicado en los 28 controllers. Recuerda: esto SOLO funciona bien si Fix-AttachAuthToApiClients.ps1 ya esta aplicado - si no lo esta, aplicalo antes de probar." -ForegroundColor Green