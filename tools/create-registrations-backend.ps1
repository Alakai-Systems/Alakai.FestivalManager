# Fix-Step76-InvoiceSuccessErrorAlerts.ps1
# Anade el patron ShowSuccess/ShowError con auto-cierre a los 3.5s (igual que
# en el resto de la app, por ejemplo DiscountCodes.razor) a las acciones de
# Editar y Eliminar factura, que se quedaron sin el, con solo un mensaje de
# error suelto dentro del modal.
#
# Ejecutar DESPUES de Fix-Step75.
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Pages/Invoices.razor"

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

# ── 1. Markup: banner de exito junto al de error ya existente ──────────────
$results += Patch-File -Path $path -Description "Anadir el banner de exito" -OldString @'
        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
        }
'@ -NewString @'
        @if (!string.IsNullOrWhiteSpace(successMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">@successMessage</div>
        }

        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
        }
'@

# ── 2. Quitar el mensaje de error suelto dentro del modal de Edit ──────────
$results += Patch-File -Path $path -Description "Quitar el mensaje de error suelto del modal de Edit" -OldString @'
                    @if (!string.IsNullOrWhiteSpace(editErrorMessage))
                    {
                        <div class="px-5 pb-3 text-sm text-danger">@editErrorMessage</div>
                    }

                    <div class="flex justify-end gap-3 p-5 border-t">
'@ -NewString @'
                    <div class="flex justify-end gap-3 p-5 border-t">
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (markup). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 3. @code: successMessage + ShowSuccess/ShowError + usarlos en Save/Delete ──
$results2 = Patch-File -Path $path -Description "Anadir successMessage y los metodos ShowSuccess/ShowError" -OldString @'
    private bool isLoading = true;
    private string? errorMessage;
'@ -NewString @'
    private bool isLoading = true;
    private string? errorMessage;
    private string? successMessage;

    private void ShowSuccess(string message)
    {
        successMessage = message;
        errorMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            successMessage = null;
            StateHasChanged();
        });
    }

    private void ShowError(string message)
    {
        errorMessage = message;
        successMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            errorMessage = null;
            StateHasChanged();
        });
    }
'@

if (-not $results2) {
    Write-Host "`nNo se pudo aplicar el patch de ShowSuccess/ShowError." -ForegroundColor Red
    exit 1
}

$results3 = Patch-File -Path $path -Description "SaveEditAsync: usar ShowSuccess/ShowError" -OldString @'
        isSavingEdit = true;
        editErrorMessage = null;

        try
        {
            await InvoiceApiClient.UpdateAsync(editingInvoice.Id, editFormModel);
            editingInvoice = null;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            editErrorMessage = ex.Message;
        }
        finally
        {
            isSavingEdit = false;
        }
    }
'@ -NewString @'
        isSavingEdit = true;

        try
        {
            await InvoiceApiClient.UpdateAsync(editingInvoice.Id, editFormModel);
            string number = editingInvoice.Number;
            editingInvoice = null;
            await LoadDataAsync();
            ShowSuccess($"Invoice {number} updated successfully.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            isSavingEdit = false;
        }
    }
'@

if (-not $results3) {
    Write-Host "`nNo se pudo aplicar el patch de SaveEditAsync." -ForegroundColor Red
    exit 1
}

$results4 = Patch-File -Path $path -Description "ConfirmDeleteAsync: usar ShowSuccess/ShowError" -OldString @'
        isDeletingInvoice = true;

        try
        {
            await InvoiceApiClient.DeleteAsync(deletingInvoice.Id);
            deletingInvoice = null;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isDeletingInvoice = false;
        }
    }
'@ -NewString @'
        isDeletingInvoice = true;

        try
        {
            string number = deletingInvoice.Number;
            await InvoiceApiClient.DeleteAsync(deletingInvoice.Id);
            deletingInvoice = null;
            await LoadDataAsync();
            ShowSuccess($"Invoice {number} deleted successfully.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            isDeletingInvoice = false;
        }
    }
'@

if (-not $results4) {
    Write-Host "`nNo se pudo aplicar el patch de ConfirmDeleteAsync." -ForegroundColor Red
    exit 1
}

# ── 4. Quitar el campo editErrorMessage que ya no se usa ────────────────────
$results5 = Patch-File -Path $path -Description "Quitar el campo editErrorMessage (ya no se usa)" -OldString @'
    private bool isSavingEdit;
    private string? editErrorMessage;
'@ -NewString @'
    private bool isSavingEdit;
'@

if (-not $results5) {
    Write-Host "`nNo se pudo quitar editErrorMessage - no es grave si se queda, simplemente sin usar." -ForegroundColor Yellow
}

Write-Host "`nHecho. dotnet build para confirmar." -ForegroundColor Green