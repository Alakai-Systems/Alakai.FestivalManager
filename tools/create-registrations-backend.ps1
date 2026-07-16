# Fix-Step4i-CreateEditColumnsAndIcon.ps1
#
# Arregla en un solo push:
#   1) Create Festival: dos columnas (nunca se aplico antes)
#   2) Edit Festival: dos columnas (nunca se aplico antes)
#   3) Icono de Payment Settings: ri-bank-card-line -> ri-secure-payment-line
#      (mas reconocible como "pago seguro")
#
# El modal de Payment Settings YA esta correcto en el archivo actual (2 columnas,
# max-w-lg) - no se toca, no hace falta.
#
# Verificado letra por letra contra Festivals.razor tal como esta ahora mismo.
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

# --- 1. Icono de Payment Settings ---
$results += Patch-File -Path $razorPath -Description "Icono de Payment Settings mas reconocible" -OldString @'
                                                <button type="button" class="text-purple" title="Payment Settings" @onclick="() => OpenCredentialsModal(festival)">
                                                    <i class="ri-bank-card-line text-lg"></i>
                                                </button>
'@ -NewString @'
                                                <button type="button" class="text-purple" title="Payment Settings" @onclick="() => OpenCredentialsModal(festival)">
                                                    <i class="ri-secure-payment-line text-lg"></i>
                                                </button>
'@

# --- 2. Modal CREATE: dos columnas ---
$results += Patch-File -Path $razorPath -Description "Create modal: dos columnas" -OldString @'
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">New Festival</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="p-5 space-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <input class="form-input" placeholder="Name" @bind="createRequest.Name" />
                    <input class="form-input" placeholder="Slug" @bind="createRequest.Slug" />
                    <input class="form-input" placeholder="Website" @bind="createRequest.Website" />
                    <input class="form-input" placeholder="Logo URL" @bind="createRequest.LogoUrl" />
                    <input class="form-input" placeholder="Favicon URL" @bind="createRequest.FaviconUrl" />
                    <input class="form-input" placeholder="Terms & Conditions URL" @bind="createRequest.TermsUrl" />
                    <input class="form-input" placeholder="Google Analytics Property ID" @bind="createRequest.GoogleAnalyticsPropertyId" />
                    <textarea class="form-input" placeholder="Description" rows="3" @bind="createRequest.Description"></textarea>

                    <div>
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasAccommodation" />
                                Accommodation
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasTransport" />
                                Transport
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasMeals" />
                                Meals
                            </label>
                        </div>
                    </div>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="CreateFestivalAsync">
                        @(isSaving ? "Creating..." : "Create")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showEditModal)
'@ -NewString @'
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">New Festival</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="grid grid-cols-1 gap-4 p-5 md:grid-cols-2">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded md:col-span-2 bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <input class="form-input" placeholder="Name" @bind="createRequest.Name" />
                    <input class="form-input" placeholder="Slug" @bind="createRequest.Slug" />
                    <input class="form-input" placeholder="Website" @bind="createRequest.Website" />
                    <input class="form-input" placeholder="Logo URL" @bind="createRequest.LogoUrl" />
                    <input class="form-input" placeholder="Favicon URL" @bind="createRequest.FaviconUrl" />
                    <input class="form-input" placeholder="Terms & Conditions URL" @bind="createRequest.TermsUrl" />
                    <input class="form-input" placeholder="Google Analytics Property ID" @bind="createRequest.GoogleAnalyticsPropertyId" />
                    <textarea class="form-input md:col-span-2" placeholder="Description" rows="3" @bind="createRequest.Description"></textarea>

                    <div class="md:col-span-2">
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasAccommodation" />
                                Accommodation
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasTransport" />
                                Transport
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="CreateHasMeals" />
                                Meals
                            </label>
                        </div>
                    </div>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="CreateFestivalAsync">
                        @(isSaving ? "Creating..." : "Create")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showEditModal)
'@

# --- 3. Modal EDIT: dos columnas ---
$results += Patch-File -Path $razorPath -Description "Edit modal: dos columnas" -OldString @'
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Edit Festival</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="p-5 space-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <label class="block text-sm text-black/60 dark:text-white/60">Name</label>
                    <input class="form-input" placeholder="Name" @bind="updateRequest.Name" />

                    <label class="block text-sm text-black/60 dark:text-white/60">Slug</label>
                    <input class="form-input" placeholder="Slug" @bind="updateRequest.Slug" />

                    <label class="block text-sm text-black/60 dark:text-white/60">Website</label>
                    <input class="form-input" placeholder="Website" @bind="updateRequest.Website" />

                    <label class="block text-sm text-black/60 dark:text-white/60">Logo URL</label>
                    <input class="form-input" placeholder="Logo URL" @bind="updateRequest.LogoUrl" />

                    <label class="block text-sm text-black/60 dark:text-white/60">Favicon URL</label>
                    <input class="form-input" placeholder="Favicon URL" @bind="updateRequest.FaviconUrl" />

                    <label class="block text-sm text-black/60 dark:text-white/60">Terms & Conditions URL</label>
                    <input class="form-input" placeholder="Terms & Conditions URL" @bind="updateRequest.TermsUrl" />

                    <label class="block text-sm text-black/60 dark:text-white/60">Google Analytics Property ID</label>
                    <input class="form-input" placeholder="Google Analytics Property ID" @bind="updateRequest.GoogleAnalyticsPropertyId" />

                    <label class="block text-sm text-black/60 dark:text-white/60">Description</label>
                    <textarea class="form-input" placeholder="Description" rows="3" @bind="updateRequest.Description"></textarea>
                    <div>
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasAccommodation" />
                                Accommodation
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasTransport" />
                                Transport
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasMeals" />
                                Meals
                            </label>
                        </div>
                    </div>

                    <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                        <input type="checkbox" @bind="updateRequest.IsActive" />
                        Active
                    </label>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="UpdateFestivalAsync">
                        @(isSaving ? "Saving..." : "Save")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showDeleteModal && selectedFestival is not null)
'@ -NewString @'
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Edit Festival</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="grid grid-cols-1 gap-4 p-5 md:grid-cols-2">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded md:col-span-2 bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Name</label>
                        <input class="form-input" placeholder="Name" @bind="updateRequest.Name" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Slug</label>
                        <input class="form-input" placeholder="Slug" @bind="updateRequest.Slug" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Website</label>
                        <input class="form-input" placeholder="Website" @bind="updateRequest.Website" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Logo URL</label>
                        <input class="form-input" placeholder="Logo URL" @bind="updateRequest.LogoUrl" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Favicon URL</label>
                        <input class="form-input" placeholder="Favicon URL" @bind="updateRequest.FaviconUrl" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Terms & Conditions URL</label>
                        <input class="form-input" placeholder="Terms & Conditions URL" @bind="updateRequest.TermsUrl" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Google Analytics Property ID</label>
                        <input class="form-input" placeholder="Google Analytics Property ID" @bind="updateRequest.GoogleAnalyticsPropertyId" />
                    </div>

                    <div class="md:col-span-2">
                        <label class="block text-sm text-black/60 dark:text-white/60">Description</label>
                        <textarea class="form-input" placeholder="Description" rows="3" @bind="updateRequest.Description"></textarea>
                    </div>

                    <div class="md:col-span-2">
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Modules</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasAccommodation" />
                                Accommodation
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasTransport" />
                                Transport
                            </label>
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasMeals" />
                                Meals
                            </label>
                        </div>
                    </div>

                    <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white md:col-span-2">
                        <input type="checkbox" @bind="updateRequest.IsActive" />
                        Active
                    </label>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="UpdateFestivalAsync">
                        @(isSaving ? "Saving..." : "Save")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showDeleteModal && selectedFestival is not null)
'@

# --- 4. Payment Settings: bajar tambien a max-w-lg (coincide con el resto de la app) ---
$results += Patch-File -Path $razorPath -Description "Payment Settings modal: max-w-lg (coincide con DiscountCodes.razor)" -OldString @'
            <div class="relative w-full max-w-2xl overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Payment Settings — @selectedFestivalForCredentials.Name</h3>
'@ -NewString @'
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Payment Settings — @selectedFestivalForCredentials.Name</h3>
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nLos 3 arreglos aplicados. dotnet build para confirmar, luego un solo commit+push." -ForegroundColor Green