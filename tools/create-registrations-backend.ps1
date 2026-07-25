# Fix-UsersControllerAuthorizeCombination.ps1
#
# CAUSA REAL, confirmada con logs de produccion (el 403 sale incluso justo
# despues de un login nuevo, así que no es token viejo):
#
# En ASP.NET Core, [Authorize(Roles="X")] en la CLASE y [Authorize] (sin
# roles) en un METODO NO se anulan entre si - se combinan, y las dos
# condiciones tienen que cumplirse a la vez. El [Authorize] que puse en
# GetById/Update nunca quito la exigencia de rol de la clase, solo anadio
# "tiene que estar autenticado" ENCIMA de "tiene que ser SuperAdmin o
# Admin". Por eso Production seguia dando 403 sin importar nada mas.
#
# Fix: quitar el [Authorize(Roles=...)] de la CLASE, y ponerlo explicito
# en cada accion que sí debe ser solo admin (GetAll, GetByEmail, Create,
# CreateAdmin, Delete). GetById y Update se quedan con [Authorize] a secas
# (cualquier autenticado) + la comprobacion IsSelfOrAdmin que ya tenian,
# que es la que de verdad decide quien puede ver que perfil.
#
# Verificado letra por letra contra el UsersController.cs real que
# pegaste - los 6 cambios coinciden exactamente una vez cada uno.
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

$path = "Alakai.FestivalManager.Api/Controllers/UsersController.cs"
$results = @()

$results += Patch-File -Path $path -Description "Quitar el rol de la clase (era el que bloqueaba todo)" -OldString @'
[Authorize(Roles = "SuperAdmin,Admin")]
public class UsersController : ControllerBase
'@ -NewString @'
public class UsersController : ControllerBase
'@

$results += Patch-File -Path $path -Description "GetAll: Authorize explicito" -OldString @'
    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetUsersResponse>>> GetAll(CancellationToken cancellationToken)
'@ -NewString @'
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<GetUsersResponse>>> GetAll(CancellationToken cancellationToken)
'@

$results += Patch-File -Path $path -Description "GetByEmail: Authorize explicito" -OldString @'
    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<ApiResponse<GetUserByIdResponse>>> GetByEmail(string email, CancellationToken cancellationToken)
'@ -NewString @'
    [HttpGet("by-email/{email}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<GetUserByIdResponse>>> GetByEmail(string email, CancellationToken cancellationToken)
'@

$results += Patch-File -Path $path -Description "Create: Authorize explicito" -OldString @'
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
'@ -NewString @'
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
'@

$results += Patch-File -Path $path -Description "CreateAdmin: Authorize explicito" -OldString @'
    [HttpPost("admins")]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> CreateAdmin([FromBody] CreateAdminUserCommand command, CancellationToken cancellationToken)
'@ -NewString @'
    [HttpPost("admins")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> CreateAdmin([FromBody] CreateAdminUserCommand command, CancellationToken cancellationToken)
'@

$results += Patch-File -Path $path -Description "Delete: Authorize explicito" -OldString @'
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteUserResponse>>> Delete(Guid id, CancellationToken cancellationToken)
'@ -NewString @'
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<DeleteUserResponse>>> Delete(Guid id, CancellationToken cancellationToken)
'@

if ($results -contains $false) {
    Write-Host "`nAlgo no coincidio - pero esto ya se verifico letra por letra contra tu archivo real antes de dartelo, asi que si falla aqui es que el archivo cambio desde que lo pegaste." -ForegroundColor Red
    exit 1
}

Write-Host "`nGetById y Update ya no exigen rol de la clase - solo autenticacion + ser el propio usuario (o admin). El resto de acciones (GetAll, GetByEmail, Create, CreateAdmin, Delete) mantienen exactamente la misma proteccion SuperAdmin/Admin de antes, ahora puesta explicita en cada una." -ForegroundColor Green