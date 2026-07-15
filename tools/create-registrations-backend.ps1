param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$adminBase = "Alakai.FestivalManager.Admin"

function Patch-File {
    param(
        [string]$RelativePath,
        [string]$OldText,
        [string]$NewText
    )
    $fullPath = Join-Path (Get-Location) $RelativePath
    if (-not (Test-Path $fullPath)) {
        Write-Error "FILE NOT FOUND: $fullPath"
        exit 1
    }
    $content = [System.IO.File]::ReadAllText($fullPath, [System.Text.Encoding]::UTF8)
    if (-not $content.Contains($OldText)) {
        Write-Host "SKIP (anchor not found): $RelativePath"
        return
    }
    $newContent = $content.Replace($OldText, $NewText)
    [System.IO.File]::WriteAllText($fullPath, $newContent, [System.Text.Encoding]::UTF8)
    Write-Host "PATCHED: $RelativePath"
}

# ==============================================================================
# FIX: Dashboard.razor
#
# PROBLEMA: OnInitializedAsync llama SyncActiveFestivalStateAsync, que siempre
# escribe en localStorage via setActiveFestivalId. Esto sobreescribe el valor
# guardado (Swim Out) con el festival por defecto (La Jam) ANTES de que
# OnAfterRenderAsync lo pueda leer.
#
# SOLUCIÓN: añadir parámetro saveToStorage a SyncActiveFestivalStateAsync.
# - OnInitializedAsync lo llama con saveToStorage: false (no toca localStorage)
# - OnAfterRenderAsync lo llama con saveToStorage: true (es el momento correcto)
# - OnFestivalChangedAsync lo llama con saveToStorage: true (cambio manual del usuario)
# ==============================================================================
$dashboardPath = "$adminBase/Components/Pages/Dashboard.razor"

# 1. OnInitializedAsync: pasar saveToStorage: false
Patch-File `
    -RelativePath $dashboardPath `
    -OldText @'
            await SyncActiveFestivalStateAsync();

            EditionDto? defaultEdition = editionsForSelectedFestival.FirstOrDefault(e => e.IsActive)
                ?? editionsForSelectedFestival.FirstOrDefault();

            if (defaultEdition is not null)
            {
                selectedEditionId = defaultEdition.Id;
                await LoadAllAsync();
            }
            else
            {
                isLoading = false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            isLoading = false;
        }
    }
'@ `
    -NewText @'
            await SyncActiveFestivalStateAsync(saveToStorage: false);

            EditionDto? defaultEdition = editionsForSelectedFestival.FirstOrDefault(e => e.IsActive)
                ?? editionsForSelectedFestival.FirstOrDefault();

            if (defaultEdition is not null)
            {
                selectedEditionId = defaultEdition.Id;
                await LoadAllAsync();
            }
            else
            {
                isLoading = false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            isLoading = false;
        }
    }
'@

# 2. OnAfterRenderAsync: pasar saveToStorage: true (ya es el comportamiento correcto)
Patch-File `
    -RelativePath $dashboardPath `
    -OldText @'
            selectedFestivalId = savedFestivalId;
            await SyncActiveFestivalStateAsync();

            EditionDto? defaultEdition = editionsForSelectedFestival.FirstOrDefault(e => e.IsActive)
                ?? editionsForSelectedFestival.FirstOrDefault();

            selectedEditionId = defaultEdition?.Id ?? Guid.Empty;
'@ `
    -NewText @'
            selectedFestivalId = savedFestivalId;
            await SyncActiveFestivalStateAsync(saveToStorage: true);

            EditionDto? defaultEdition = editionsForSelectedFestival.FirstOrDefault(e => e.IsActive)
                ?? editionsForSelectedFestival.FirstOrDefault();

            selectedEditionId = defaultEdition?.Id ?? Guid.Empty;
'@

# 3. OnFestivalChangedAsync: pasar saveToStorage: true
Patch-File `
    -RelativePath $dashboardPath `
    -OldText @'
    private async Task OnFestivalChangedAsync()
    {
        await SyncActiveFestivalStateAsync();
'@ `
    -NewText @'
    private async Task OnFestivalChangedAsync()
    {
        await SyncActiveFestivalStateAsync(saveToStorage: true);
'@

# 4. SyncActiveFestivalStateAsync: añadir el parámetro
Patch-File `
    -RelativePath $dashboardPath `
    -OldText @'
    private async Task SyncActiveFestivalStateAsync()
    {
        FestivalDto? festival = festivals.FirstOrDefault(f => f.Id == selectedFestivalId);

        if (festival is null)
        {
            return;
        }

        if (!ActiveFestivalState.IsInitialized)
        {
            ActiveFestivalState.Initialize(festivals, festival.Id);
        }
        else
        {
            ActiveFestivalState.SetActive(festival);
        }

        try
        {
            await JsRuntime.InvokeVoidAsync("setActiveFestivalId", festival.Id.ToString());
        }
        catch
        {
            // Non-critical: the Sidebar just won't remember the choice across reloads if this fails.
        }
    }
'@ `
    -NewText @'
    private async Task SyncActiveFestivalStateAsync(bool saveToStorage = true)
    {
        FestivalDto? festival = festivals.FirstOrDefault(f => f.Id == selectedFestivalId);

        if (festival is null)
        {
            return;
        }

        if (!ActiveFestivalState.IsInitialized)
        {
            ActiveFestivalState.Initialize(festivals, festival.Id);
        }
        else
        {
            ActiveFestivalState.SetActive(festival);
        }

        if (saveToStorage)
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("setActiveFestivalId", festival.Id.ToString());
            }
            catch
            {
                // Non-critical: the Sidebar just won't remember the choice across reloads if this fails.
            }
        }
    }
'@

Write-Host ""
Write-Host "PATCHED: Dashboard.razor"
Write-Host "  OnInitializedAsync  -> SyncActiveFestivalStateAsync(saveToStorage: false)"
Write-Host "  OnAfterRenderAsync  -> SyncActiveFestivalStateAsync(saveToStorage: true)"
Write-Host "  OnFestivalChangedAsync -> SyncActiveFestivalStateAsync(saveToStorage: true)"
Write-Host "  SyncActiveFestivalStateAsync ahora tiene parametro saveToStorage"