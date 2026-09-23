<#
    Fix-UsersPageSlowLoad.ps1
    ---------------------------
    Independiente de los demas scripts -- no depende de ninguno de ellos.

    Arregla que la pagina Users tarde minutos en cargar.

    Causa: LoadDocumentsAsync() (para rellenar la columna "Document" de la
    tabla) hacia UNA LLAMADA HTTP POR CADA USUARIO, una detras de otra
    (RegistrationApiClient.GetByUserIdAsync dentro de un foreach con
    await). Con pocos usuarios no se notaba, pero cuantos mas usuarios
    haya (especialmente ahora que se pueden importar por CSV), mas
    llamadas secuenciales se hacen -- de ahi que ahora tarde minutos.

    La pagina ya carga TODAS las registrations de golpe en una sola
    llamada (allRegistrations = RegistrationApiClient.GetAllAsync(), justo
    antes), y esa lista ya incluye el DocumentNumber de cada una. No hacia
    falta ninguna llamada adicional: este script hace que
    LoadDocumentsAsync busque el documento en esa lista que ya esta en
    memoria, en vez de volver a pedirlo al servidor usuario por usuario.
    Resultado: la pagina pasa de N llamadas HTTP (una por usuario) a 0
    llamadas adicionales.

    No toca base de datos ni requiere migracion.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-UsersPageSlowLoad.ps1

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

# 1. Alakai.FestivalManager.Admin/Components/Pages/Users.razor -- LoadDataAsync: ya no espera LoadDocumentsAsync (deja de ser async)
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Users.razor' `
    -Description 'LoadDataAsync: ya no espera LoadDocumentsAsync (deja de ser async)' `
    -Anchor @'
            festivals = (await FestivalApiClient.GetAllAsync()).ToList();
            await LoadDocumentsAsync();
'@ `
    -Replacement @'
            festivals = (await FestivalApiClient.GetAllAsync()).ToList();
            LoadDocumentsAsync();
'@

# 2. Alakai.FestivalManager.Admin/Components/Pages/Users.razor -- LoadDocumentsAsync: busca el DocumentNumber en memoria (allRegistrations) en vez de 1 llamada HTTP por usuario
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Users.razor' `
    -Description 'LoadDocumentsAsync: busca el DocumentNumber en memoria (allRegistrations) en vez de 1 llamada HTTP por usuario' `
    -Anchor @'
    private async Task LoadDocumentsAsync()
    {
        documentsByUserId.Clear();

        foreach (UserDto user in users)
        {
            try
            {
                RegistrationDto registration = await RegistrationApiClient.GetByUserIdAsync(user.Id);
                documentsByUserId[user.Id] = registration?.DocumentNumber ?? "-";
            }
            catch (ApiClientException)
            {
                // El usuario no tiene registration asociada, o el backend devolvi� error controlado
                documentsByUserId[user.Id] = "-";
            }
        }
    }
'@ `
    -Replacement @'
    private void LoadDocumentsAsync()
    {
        documentsByUserId.Clear();

        // Antes esto hacia una llamada HTTP por cada usuario (RegistrationApiClient.GetByUserIdAsync),
        // que con muchos usuarios tardaba minutos en cargar la pagina. allRegistrations ya trae todo
        // lo que hace falta (incluido DocumentNumber) en una sola llamada, asi que basta con buscar en memoria.
        foreach (UserDto user in users)
        {
            RegistrationDto? registration = allRegistrations.FirstOrDefault(r => r.UserId == user.Id);
            documentsByUserId[user.Id] = registration?.DocumentNumber ?? "-";
        }
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