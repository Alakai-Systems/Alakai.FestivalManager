<#
    Fix-UserEditRoleValidation.ps1
    -------------------------------
    Independiente de los demas scripts -- no depende de ninguno de ellos.

    Arregla el error "Validation failed." al editar CUALQUIER usuario desde
    Users > editar (lapiz).

    Causa: el formulario de "Edit User" nunca ha incluido el Role del
    usuario (no hay ese campo en el modal). Al guardar, se manda Role = 0
    al servidor porque el modelo del formulario no lo lleva. El enum
    UserRole empieza en 1 (SuperAdmin=1, Admin=2, User=3, Production=4), o
    sea que 0 no es un valor valido, y el validador del servidor
    (RuleFor(command => command.Role).IsInEnum()) rechaza SIEMPRE la
    peticion, para cualquier usuario. Por eso ves "Validation failed." al
    intentar guardar el usuario Jose Maria Camacho (y pasaria con
    cualquier otro).

    No es un problema de datos en produccion ni de un usuario concreto: es
    un bug del formulario, asi que no hace falta tocar nada en la base de
    datos.

    Este script NO anade un selector de Role al formulario (eso cambiaria
    quien puede tocar el rol de un usuario desde aqui, y no me has pedido
    eso). Simplemente hace que el formulario conserve el Role que el
    usuario ya tiene al abrir el modal y lo reenvie tal cual al guardar,
    para que la validacion del servidor no lo vea como invalido.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-UserEditRoleValidation.ps1

    Idempotente y todo-o-nada: si algun anchor no encaja porque algun archivo
    local difiere de lo esperado, no escribe nada y lista el problema.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath 'Alakai.FestivalManager.sln')) {
    Write-Error "No se encuentra Alakai.FestivalManager.sln en el directorio actual. Ejecuta este script desde la raiz del repo (Alakai.FestivalManager/)."
    exit 1
}

function Get-NormalizedContent {
    param([string]$Path)

    $raw = [System.IO.File]::ReadAllText($Path)
    $usesCrlf = $raw.Contains("`r`n")
    $normalized = $raw.Replace("`r`n", "`n")

    return [PSCustomObject]@{
        Raw        = $raw
        Normalized = $normalized
        UsesCrlf   = $usesCrlf
    }
}

function Set-NormalizedContent {
    param(
        [string]$Path,
        [string]$NormalizedContent,
        [bool]$UsesCrlf
    )

    $final = if ($UsesCrlf) { $NormalizedContent.Replace("`n", "`r`n") } else { $NormalizedContent }
    [System.IO.File]::WriteAllText($Path, $final, [System.Text.UTF8Encoding]::new($false))
}

function Convert-ToLf {
    param([string]$Text)
    return $Text.Replace("`r`n", "`n")
}

$script:PlanErrors = @()
$script:Plan = @()

function Add-PatchOperation {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Anchor,
        [Parameter(Mandatory)][string]$Replacement,
        [Parameter(Mandatory)][string]$Description
    )

    $script:Plan += [PSCustomObject]@{
        Type        = 'Patch'
        Path        = $Path
        Anchor      = Convert-ToLf $Anchor
        Replacement = Convert-ToLf $Replacement
        Description = $Description
    }
}

function Test-PatchOperation {
    param($Op)

    if (-not (Test-Path -LiteralPath $Op.Path)) {
        return "No existe el archivo: $($Op.Path)"
    }

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 1) {
        return $null
    }

    if ($anchorCount -eq 0) {
        $replacementCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Replacement))).Count
        if ($replacementCount -ge 1) {
            return $null  # ya aplicado -> idempotente
        }
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado). Descripcion: $($Op.Description)"
    }

    return "Anchor encontrado $anchorCount veces en $($Op.Path) (deberia ser unico). Descripcion: $($Op.Description)"
}

function Invoke-PatchOperation {
    param($Op)

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 0) {
        Write-Host "  = ya aplicado: $($Op.Path) -- $($Op.Description)" -ForegroundColor DarkGray
        return
    }

    $newNormalized = $file.Normalized.Replace($Op.Anchor, $Op.Replacement)
    Set-NormalizedContent -Path $Op.Path -NormalizedContent $newNormalized -UsesCrlf $file.UsesCrlf

    Write-Host "  + patched: $($Op.Path) -- $($Op.Description)" -ForegroundColor Green
}

# 1. Alakai.FestivalManager.Admin/Components/Pages/Users.razor -- OpenEditModal: conserva el Role actual del usuario al abrir el formulario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Users.razor' `
    -Description 'OpenEditModal: conserva el Role actual del usuario al abrir el formulario' `
    -Anchor @'
    private void OpenEditModal(UserDto user)
    {
        editingUser = user;
        formModel = new UserFormModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Country = user.Country,
            City = user.City,
            MustChangePassword = user.MustChangePassword,
            IsActive = user.IsActive
        };
        showModal = true;
    }
'@ `
    -Replacement @'
    private void OpenEditModal(UserDto user)
    {
        editingUser = user;
        formModel = new UserFormModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Country = user.Country,
            City = user.City,
            MustChangePassword = user.MustChangePassword,
            IsActive = user.IsActive,
            Role = user.Role
        };
        showModal = true;
    }
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/Users.razor -- SaveAsync: envia el Role preservado (si no, siempre llega 0 y falla la validacion del servidor)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Users.razor' `
    -Description 'SaveAsync: envia el Role preservado (si no, siempre llega 0 y falla la validacion del servidor)' `
    -Anchor @'
                UpdateUserRequest request = new()
                {
                    FirstName = formModel.FirstName,
                    LastName = formModel.LastName,
                    Email = formModel.Email,
                    Phone = formModel.Phone,
                    Country = formModel.Country,
                    City = formModel.City,
                    MustChangePassword = formModel.MustChangePassword,
                    IsActive = formModel.IsActive
                };
'@ `
    -Replacement @'
                UpdateUserRequest request = new()
                {
                    FirstName = formModel.FirstName,
                    LastName = formModel.LastName,
                    Email = formModel.Email,
                    Phone = formModel.Phone,
                    Country = formModel.Country,
                    City = formModel.City,
                    MustChangePassword = formModel.MustChangePassword,
                    IsActive = formModel.IsActive,
                    Role = formModel.Role
                };
'@

# 3. Alakai.FestivalManager.Admin/Components/Pages/Users.razor -- UserFormModel: nuevo campo Role para poder transportarlo por el formulario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Users.razor' `
    -Description 'UserFormModel: nuevo campo Role para poder transportarlo por el formulario' `
    -Anchor @'
    private class UserFormModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Password { get; set; }
        public bool MustChangePassword { get; set; }
        public bool IsActive { get; set; }
    }
'@ `
    -Replacement @'
    private class UserFormModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Password { get; set; }
        public bool MustChangePassword { get; set; }
        public bool IsActive { get; set; }
        public int Role { get; set; }
    }
'@


Write-Host ""
Write-Host "Validando $($script:Plan.Count) cambios contra los archivos locales..." -ForegroundColor Cyan

foreach ($op in $script:Plan) {
    $err = Test-PatchOperation -Op $op
    if ($err) {
        $script:PlanErrors += $err
    }
}

if ($script:PlanErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "NO se ha escrito nada. $($script:PlanErrors.Count) problema(s) encontrados:" -ForegroundColor Red
    foreach ($e in $script:PlanErrors) {
        Write-Host "  - $e" -ForegroundColor Red
    }
    Write-Host ""
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    Invoke-PatchOperation -Op $op
}

Write-Host ""
Write-Host "Listo. Solo falta 'dotnet build' -- no hace falta migracion, no se toca la BD." -ForegroundColor Cyan
Write-Host ""