# Fix-Step4h-PaymentModalTwoColumns.ps1
#
# Modal de Payment Settings a dos columnas: Redsys en la columna izquierda,
# Email (SMTP) en la derecha - division natural del propio contenido.
# El checkbox "Use SSL/TLS" ocupa las 2 columnas al final.
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

$result = Patch-File -Path $razorPath -Description "Payment Settings modal: dos columnas (Redsys | Email)" -OldString @'
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
'@ -NewString @'
            <div class="relative w-full max-w-2xl overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Payment Settings — @selectedFestivalForCredentials.Name</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="grid grid-cols-1 gap-4 p-5 md:grid-cols-2">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded md:col-span-2 bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    @if (isLoadingCredentials)
                    {
                        <p class="text-sm md:col-span-2 text-black/50 dark:text-white/60">Loading payment settings...</p>
                    }
                    else
                    {
                        <div class="space-y-4">
                            <p class="text-xs font-semibold tracking-wide uppercase text-black/40 dark:text-white/40">Redsys</p>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Merchant Code</label>
                                <input class="form-input" placeholder="Merchant Code" @bind="credentialsRequest.RedsysMerchantCode" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Terminal</label>
                                <input class="form-input" placeholder="Terminal" @bind="credentialsRequest.RedsysTerminal" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Merchant Name</label>
                                <input class="form-input" placeholder="Merchant Name" @bind="credentialsRequest.RedsysMerchantName" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Secret Key</label>
                                <input type="password" class="form-input" placeholder="@(hasRedsysSecretKey ? "Already configured - leave blank to keep" : "Secret Key")" @bind="credentialsRequest.RedsysSecretKey" />
                            </div>
                        </div>

                        <div class="space-y-4">
                            <p class="text-xs font-semibold tracking-wide uppercase text-black/40 dark:text-white/40">Email (SMTP)</p>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Host</label>
                                <input class="form-input" placeholder="smtp.example.com" @bind="credentialsRequest.EmailHost" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Port</label>
                                <input type="number" class="form-input" placeholder="587" @bind="credentialsRequest.EmailPort" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Username</label>
                                <input class="form-input" placeholder="Username" @bind="credentialsRequest.EmailUsername" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Password</label>
                                <input type="password" class="form-input" placeholder="@(hasEmailPassword ? "Already configured - leave blank to keep" : "Password")" @bind="credentialsRequest.EmailPassword" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">From Email</label>
                                <input class="form-input" placeholder="info@example.com" @bind="credentialsRequest.EmailFromEmail" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">From Name</label>
                                <input class="form-input" placeholder="Festival Name" @bind="credentialsRequest.EmailFromName" />
                            </div>
                        </div>

                        <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white md:col-span-2">
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

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual del modal de Payment Settings." -ForegroundColor Red
    exit 1
}

Write-Host "`nPayment Settings modal ahora en dos columnas (Redsys | Email). dotnet build para confirmar." -ForegroundColor Green