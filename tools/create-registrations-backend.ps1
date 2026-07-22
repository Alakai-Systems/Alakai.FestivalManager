# Fix-Step75-InvoiceEditDeleteUI.ps1
# Parte 2: cliente del Admin + botones y modales en Invoices.razor, con el
# mismo estilo estandar del resto de la app (visto en DiscountCodes.razor).
#
# Ejecutar DESPUES de Fix-Step74-InvoiceEditDelete.ps1.
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"

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

# ── 1. Admin InvoiceDto: anadir campos de direccion fiscal ──────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Contracts/Invoices/DTOs/InvoiceDto.cs" `
    -Description "Admin InvoiceDto: anadir Address/City/PostalCode/Country" -OldString @'
    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? CustomerFirstName { get; set; }
'@ -NewString @'
    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? CustomerFirstName { get; set; }
'@

# ── 2. InvoiceApiClient: UpdateAsync + DeleteAsync + request DTO ────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Services/Api/InvoiceApiClient.cs" `
    -Description "Cliente: anadir UpdateAsync y DeleteAsync" -OldString @'
        return response.Data?.Invoices ?? [];
    }
}
'@ -NewString @'
        return response.Data?.Invoices ?? [];
    }

    public async Task UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/invoices/{id}", request, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Could not update invoice: {errorBody}", null);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/invoices/{id}", cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Could not delete invoice: {errorBody}", null);
        }
    }
}

public class UpdateInvoiceRequest
{
    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 3. Invoices.razor: botones Edit/Delete junto al de Download ─────────────
$path = "Alakai.FestivalManager.Admin/Components/Pages/Invoices.razor"
$results2 = @()

$results2 += Patch-File -Path $path -Description "Anadir botones Edit y Delete junto al de Download" -OldString @'
                                <td class="px-3 py-3 text-right">
                                    <a href="@invoice.PdfUrl" target="_blank" class="text-black dark:text-white/80" title="Download">
                                        <i class="ri-download-line text-lg"></i>
                                    </a>
                                </td>
'@ -NewString @'
                                <td class="px-3 py-3 text-right">
                                    <div class="flex items-center justify-end gap-3">
                                        <a href="@invoice.PdfUrl" target="_blank" class="text-black dark:text-white/80" title="Download">
                                            <i class="ri-download-line text-lg"></i>
                                        </a>
                                        <button type="button" class="text-black dark:text-white/80" title="Edit" @onclick="() => OpenEditModal(invoice)"><i class="ri-pencil-line text-lg"></i></button>
                                        <button type="button" class="text-danger" title="Delete" @onclick="() => OpenDeleteModal(invoice)"><i class="ri-delete-bin-line text-lg"></i></button>
                                    </div>
                                </td>
'@

# ── 4. Modales de Edit y Delete, antes de "@implements IDisposable" ────────
$results2 += Patch-File -Path $path -Description "Anadir los modales de Edit y Delete" -OldString @'
@implements IDisposable
@code {
'@ -NewString @'
@if (editingInvoice is not null)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold">Edit Invoice @editingInvoice.Number</h3>
                    <button type="button" @onclick="CloseEditModal"><i class="ri-close-line text-2xl"></i></button>
                </div>

                <EditForm Model="editFormModel" OnValidSubmit="SaveEditAsync">
                    <div class="grid grid-cols-1 gap-4 p-5 md:grid-cols-2">
                        <div class="md:col-span-2">
                            <label class="form-label">Fiscal Name</label>
                            <InputText class="form-input" @bind-Value="editFormModel.FiscalName" />
                        </div>

                        <div>
                            <label class="form-label">Tax ID</label>
                            <InputText class="form-input" @bind-Value="editFormModel.TaxId" />
                        </div>

                        <div>
                            <label class="form-label">Country</label>
                            <InputText class="form-input" @bind-Value="editFormModel.Country" />
                        </div>

                        <div class="md:col-span-2">
                            <label class="form-label">Address</label>
                            <InputText class="form-input" @bind-Value="editFormModel.Address" />
                        </div>

                        <div>
                            <label class="form-label">City</label>
                            <InputText class="form-input" @bind-Value="editFormModel.City" />
                        </div>

                        <div>
                            <label class="form-label">Postal Code</label>
                            <InputText class="form-input" @bind-Value="editFormModel.PostalCode" />
                        </div>
                    </div>

                    @if (!string.IsNullOrWhiteSpace(editErrorMessage))
                    {
                        <div class="px-5 pb-3 text-sm text-danger">@editErrorMessage</div>
                    }

                    <div class="flex justify-end gap-3 p-5 border-t">
                        <button type="button" class="btn border" @onclick="CloseEditModal">Cancel</button>
                        <button type="submit" class="btn bg-purple text-white" disabled="@isSavingEdit">@(isSavingEdit ? "Saving..." : "Save")</button>
                    </div>
                </EditForm>
            </div>
        </div>
    </div>
}

@if (deletingInvoice is not null)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-[92vw] md:w-[380px] overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="px-5 py-4">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Delete Invoice</h3>
                    <p class="mt-2 text-sm text-black/60 dark:text-white/60">Are you sure you want to delete invoice <strong>@deletingInvoice.Number</strong>? This cannot be undone.</p>
                </div>
                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isDeletingInvoice" @onclick="CloseDeleteModal">Cancel</button>
                    <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@isDeletingInvoice" @onclick="ConfirmDeleteAsync">@(isDeletingInvoice ? "Deleting..." : "Delete")</button>
                </div>
            </div>
        </div>
    </div>
}

@implements IDisposable
@code {
'@

if ($results2 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (markup). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 5. @code: estado + metodos de Edit/Delete ───────────────────────────────
$results3 = Patch-File -Path $path -Description "Anadir el estado y los metodos de Edit/Delete" -OldString @'
    private bool isLoading = true;
    private string? errorMessage;
'@ -NewString @'
    private bool isLoading = true;
    private string? errorMessage;

    private InvoiceDto? editingInvoice;
    private UpdateInvoiceRequest editFormModel = new();
    private bool isSavingEdit;
    private string? editErrorMessage;

    private InvoiceDto? deletingInvoice;
    private bool isDeletingInvoice;

    private void OpenEditModal(InvoiceDto invoice)
    {
        editingInvoice = invoice;
        editErrorMessage = null;
        editFormModel = new UpdateInvoiceRequest
        {
            FiscalName = invoice.FiscalName,
            TaxId = invoice.TaxId,
            Address = invoice.Address,
            City = invoice.City,
            PostalCode = invoice.PostalCode,
            Country = invoice.Country
        };
    }

    private void CloseEditModal()
    {
        editingInvoice = null;
    }

    private async Task SaveEditAsync()
    {
        if (editingInvoice is null)
        {
            return;
        }

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

    private void OpenDeleteModal(InvoiceDto invoice)
    {
        deletingInvoice = invoice;
    }

    private void CloseDeleteModal()
    {
        deletingInvoice = null;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deletingInvoice is null)
        {
            return;
        }

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
'@

if (-not $results3) {
    Write-Host "`nNo se pudo aplicar el patch del @code." -ForegroundColor Red
    exit 1
}

Write-Host "`nBoton y modales de Edit/Delete anadidos, mismo estilo que el resto de la app. dotnet build para confirmar." -ForegroundColor Green