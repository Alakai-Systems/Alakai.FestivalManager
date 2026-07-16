# Fix-Step4e-FestivalsRazorPaymentModal.ps1
#
# Anade a Festivals.razor:
#   - Icono de "Payment Settings" (ri-bank-card-line) por fila, junto a Edit/Delete
#   - Modal separado para Redsys + Email (FestivalCredentials), con los campos
#     de secreto (RedsysSecretKey/EmailPassword) mostrando un placeholder
#     "Already configured - leave blank to keep" cuando ya existen, en vez de
#     recargar el valor real en texto plano.
#
# Replica EXACTAMENTE las mismas clases y estructura de modal ya usadas
# (mismo contenedor fixed/inset-0/z-999, mismo header/footer, mismo form-input).
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$razorPath = "Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor"

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

# --- 1. Icono de Payment Settings en la fila (antes de Edit) ---
$results += Patch-File -Path $razorPath -Description "Anadir icono de Payment Settings junto a Edit/Delete" -OldString @'
                                            <div class="flex items-center justify-end gap-3">
                                                <button type="button" class="text-black dark:text-white/80" title="Edit" @onclick="() => OpenEditModal(festival)">
                                                    <i class="ri-pencil-line text-lg"></i>
                                                </button>
                                                <button type="button" class="text-danger" title="Delete" @onclick="() => OpenDeleteModal(festival)">
                                                    <i class="ri-delete-bin-line text-lg"></i>
                                                </button>
                                            </div>
'@ -NewString @'
                                            <div class="flex items-center justify-end gap-3">
                                                <button type="button" class="text-purple" title="Payment Settings" @onclick="() => OpenCredentialsModal(festival)">
                                                    <i class="ri-bank-card-line text-lg"></i>
                                                </button>
                                                <button type="button" class="text-black dark:text-white/80" title="Edit" @onclick="() => OpenEditModal(festival)">
                                                    <i class="ri-pencil-line text-lg"></i>
                                                </button>
                                                <button type="button" class="text-danger" title="Delete" @onclick="() => OpenDeleteModal(festival)">
                                                    <i class="ri-delete-bin-line text-lg"></i>
                                                </button>
                                            </div>
'@

# --- 2. Nuevo modal de Payment Settings, tras el modal de Delete ---
$results += Patch-File -Path $razorPath -Description "Anadir modal de Payment Settings (Redsys + Email)" -OldString @'
                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="DeleteFestivalAsync">
                        @(isSaving ? "Deleting..." : "Delete")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@code {
'@ -NewString @'
                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="DeleteFestivalAsync">
                        @(isSaving ? "Deleting..." : "Delete")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showCredentialsModal && selectedFestivalForCredentials is not null)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Payment Settings — @selectedFestivalForCredentials.Name</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="p-5 space-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    @if (isLoadingCredentials)
                    {
                        <p class="text-sm text-black/50 dark:text-white/60">Loading payment settings...</p>
                    }
                    else
                    {
                        <p class="text-xs font-semibold tracking-wide uppercase text-black/40 dark:text-white/40">Redsys</p>

                        <label class="block text-sm text-black/60 dark:text-white/60">Merchant Code</label>
                        <input class="form-input" placeholder="Merchant Code" @bind="credentialsRequest.RedsysMerchantCode" />

                        <label class="block text-sm text-black/60 dark:text-white/60">Terminal</label>
                        <input class="form-input" placeholder="Terminal" @bind="credentialsRequest.RedsysTerminal" />

                        <label class="block text-sm text-black/60 dark:text-white/60">Merchant Name</label>
                        <input class="form-input" placeholder="Merchant Name" @bind="credentialsRequest.RedsysMerchantName" />

                        <label class="block text-sm text-black/60 dark:text-white/60">Secret Key</label>
                        <input type="password" class="form-input" placeholder="@(hasRedsysSecretKey ? "Already configured - leave blank to keep" : "Secret Key")" @bind="credentialsRequest.RedsysSecretKey" />

                        <p class="pt-2 text-xs font-semibold tracking-wide uppercase text-black/40 dark:text-white/40">Email (SMTP)</p>

                        <label class="block text-sm text-black/60 dark:text-white/60">Host</label>
                        <input class="form-input" placeholder="smtp.example.com" @bind="credentialsRequest.EmailHost" />

                        <label class="block text-sm text-black/60 dark:text-white/60">Port</label>
                        <input type="number" class="form-input" placeholder="587" @bind="credentialsRequest.EmailPort" />

                        <label class="block text-sm text-black/60 dark:text-white/60">Username</label>
                        <input class="form-input" placeholder="Username" @bind="credentialsRequest.EmailUsername" />

                        <label class="block text-sm text-black/60 dark:text-white/60">Password</label>
                        <input type="password" class="form-input" placeholder="@(hasEmailPassword ? "Already configured - leave blank to keep" : "Password")" @bind="credentialsRequest.EmailPassword" />

                        <label class="block text-sm text-black/60 dark:text-white/60">From Email</label>
                        <input class="form-input" placeholder="info@example.com" @bind="credentialsRequest.EmailFromEmail" />

                        <label class="block text-sm text-black/60 dark:text-white/60">From Name</label>
                        <input class="form-input" placeholder="Festival Name" @bind="credentialsRequest.EmailFromName" />

                        <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                            <input type="checkbox" @bind="credentialsRequest.EmailUseSSL" />
                            Use SSL/TLS
                        </label>
                    }
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@(isSaving || isLoadingCredentials)" @onclick="SaveCredentialsAsync">
                        @(isSaving ? "Saving..." : "Save")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@code {
'@

# --- 3. Estado + logica en @code ---
$results += Patch-File -Path $razorPath -Description "Anadir estado y metodos de Payment Settings al @code" -OldString @'
    private bool isLoading = true;
    private bool isSaving;
    private bool showCreateModal;
    private bool showEditModal;
    private bool showDeleteModal;
'@ -NewString @'
    private bool isLoading = true;
    private bool isSaving;
    private bool showCreateModal;
    private bool showEditModal;
    private bool showDeleteModal;
    private bool showCredentialsModal;
    private bool isLoadingCredentials;
    private bool hasRedsysSecretKey;
    private bool hasEmailPassword;
    private FestivalDto? selectedFestivalForCredentials;
    private UpsertFestivalCredentialsRequest credentialsRequest = new();
'@

$results += Patch-File -Path $razorPath -Description "Actualizar CloseModals para incluir el modal de Payment Settings" -OldString @'
    private void CloseModals()
    {
        showCreateModal = false;
        showEditModal = false;
        showDeleteModal = false;
        selectedFestival = null;
        modalErrorMessage = null;
    }
'@ -NewString @'
    private void CloseModals()
    {
        showCreateModal = false;
        showEditModal = false;
        showDeleteModal = false;
        showCredentialsModal = false;
        selectedFestival = null;
        selectedFestivalForCredentials = null;
        modalErrorMessage = null;
    }

    private async Task OpenCredentialsModal(FestivalDto festival)
    {
        selectedFestivalForCredentials = festival;
        modalErrorMessage = null;
        showCredentialsModal = true;
        isLoadingCredentials = true;
        credentialsRequest = new UpsertFestivalCredentialsRequest();
        hasRedsysSecretKey = false;
        hasEmailPassword = false;

        try
        {
            FestivalCredentialsDto? existing = await FestivalApiClient.GetCredentialsAsync(festival.Id);

            if (existing is not null)
            {
                credentialsRequest.RedsysMerchantCode = existing.RedsysMerchantCode;
                credentialsRequest.RedsysTerminal = existing.RedsysTerminal;
                credentialsRequest.RedsysMerchantName = existing.RedsysMerchantName;
                credentialsRequest.EmailHost = existing.EmailHost;
                credentialsRequest.EmailPort = existing.EmailPort;
                credentialsRequest.EmailUsername = existing.EmailUsername;
                credentialsRequest.EmailFromEmail = existing.EmailFromEmail;
                credentialsRequest.EmailFromName = existing.EmailFromName;
                credentialsRequest.EmailUseSSL = existing.EmailUseSSL;
                hasRedsysSecretKey = existing.HasRedsysSecretKey;
                hasEmailPassword = existing.HasEmailPassword;
            }
        }
        catch (ApiClientException ex)
        {
            modalErrorMessage = BuildErrorMessage(ex);
        }
        catch (Exception ex)
        {
            modalErrorMessage = ex.Message;
        }
        finally
        {
            isLoadingCredentials = false;
        }
    }

    private async Task SaveCredentialsAsync()
    {
        if (isSaving || selectedFestivalForCredentials is null)
        {
            return;
        }

        try
        {
            isSaving = true;
            modalErrorMessage = null;

            await FestivalApiClient.UpsertCredentialsAsync(selectedFestivalForCredentials.Id, credentialsRequest);

            CloseModals();
            successMessage = "Payment settings saved successfully.";
            errorMessage = null;
            _ = ClearMessagesAfterDelayAsync();
        }
        catch (ApiClientException ex)
        {
            modalErrorMessage = BuildErrorMessage(ex);
        }
        catch (Exception ex)
        {
            modalErrorMessage = ex.Message;
        }
        finally
        {
            isSaving = false;
        }
    }
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nPaso 4e completo (icono + modal de Payment Settings). dotnet build para confirmar." -ForegroundColor Green
Write-Host "Con esto se cierra el Paso 4 completo." -ForegroundColor Green