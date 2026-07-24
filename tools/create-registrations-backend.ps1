# Fix-RegistrationCreateAllowAnonymous.ps1
#
# URGENTE: la auditoria de [Authorize] puso "SuperAdmin,Admin" a nivel de
# clase en RegistrationsController - pero la acción Create() de ese mismo
# controller es la que usa el FORMULARIO PUBLICO de inscripcion
# (PublicRegistrationApiClient.SubmitAsync -> POST api/registrations), sin
# usuario logueado. Al restringir la clase entera, se rompio el registro
# publico. Esto SI es un fallo mio.
#
# Fix: [AllowAnonymous] en la accion Create especificamente, que en ASP.NET
# Core anula el [Authorize] de la clase SOLO para esa accion. El resto de
# acciones (GetById, GetAll, GetByUserId, GetByEditionId, Update, Delete)
# siguen exigiendo SuperAdmin/Admin, que es correcto - esas si son de
# gestion interna del Admin.
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

$results = @()
$path = "Alakai.FestivalManager.Api/Controllers/RegistrationsController.cs"

# Por si acaso el [Authorize] de clase aun no se aplico en tu copia, lo
# aseguramos aqui tambien (idempotente, no hace nada si ya esta).
$results += Patch-File -Path $path -Description "RegistrationsController: [Authorize] de clase (por si faltaba)" -OldString @'
public class RegistrationsController : ControllerBase
'@ -NewString @'
[Authorize(Roles = "SuperAdmin,Admin")]
public class RegistrationsController : ControllerBase
'@
if ($results -contains $false) { Write-Host "`nFallo (clase)." -ForegroundColor Red; exit 1 }

$results += Patch-File -Path $path -Description "RegistrationsController.Create: [AllowAnonymous] (usado por el registro publico)" -OldString @'
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegistrationRequest request, CancellationToken cancellationToken)
'@ -NewString @'
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateRegistrationRequest request, CancellationToken cancellationToken)
'@
if ($results -contains $false) { Write-Host "`nFallo (Create)." -ForegroundColor Red; exit 1 }

Write-Host "`nRegistro publico arreglado: Create() vuelve a ser accesible sin login, el resto de RegistrationsController sigue protegido." -ForegroundColor Green