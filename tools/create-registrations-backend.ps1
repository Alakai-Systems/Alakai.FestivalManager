# Fix-Step84-ImpersonateLifecycleTiming.ps1
#
# BUG REAL, y probablemente la causa de fondo de todo: Impersonate.razor
# llamaba a TokenStorageService.SetTokenAsync (que usa ProtectedLocalStorage,
# necesita JS interop) dentro de OnInitializedAsync - un momento del ciclo de
# vida de Blazor Server donde el circuito interactivo puede NO estar listo
# todavia para JS interop, especialmente en una pestana nueva recien abierta.
#
# El login normal SI funciona porque alli SetTokenAsync se llama desde un
# manejador de clic de boton (@onclick) - en ese momento el circuito YA esta
# totalmente interactivo por definicion (el usuario pudo hacer clic).
#
# Fix: mover la llamada a OnAfterRenderAsync(firstRender), el momento correcto
# del ciclo de vida para JS interop - exactamente como hace el propio panel de
# usuario para cargar sus datos.
#
# Ejecutar DESPUES de Fix-Step78.
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Pages/Impersonate.razor"

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

$result = Patch-File -Path $path -Description "Mover SetTokenAsync a OnAfterRenderAsync (momento correcto para JS interop)" -OldString @'
@code {
    [SupplyParameterFromQuery(Name = "token")]
    public string? Token { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrWhiteSpace(Token))
        {
            await TokenStorageService.SetTokenAsync(Token);
        }

        Navigation.NavigateTo("/user-panel/dashboard/en", forceLoad: true);
    }
}
'@ -NewString @'
@code {
    [SupplyParameterFromQuery(Name = "token")]
    public string? Token { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!string.IsNullOrWhiteSpace(Token))
            {
                await TokenStorageService.SetTokenAsync(Token);
            }

            Navigation.NavigateTo("/user-panel/dashboard/en", forceLoad: true);
        }
    }
}
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual de Impersonate.razor." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Prueba de nuevo, con el navegador ya limpio de antes." -ForegroundColor Cyan