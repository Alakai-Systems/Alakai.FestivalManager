<#
.SINOPSIS
  Paso B del check-in: pantalla /checkin en la Admin. Lee la camara con
  html5-qrcode (cargado bajo demanda desde CDN, no en cada pagina) y tiene
  entrada manual como fallback si la camara falla o el token se lee con el
  movil. Llama al endpoint del Paso A (POST api/tickets/checkin).

  Archivos nuevos:
    - Admin\Contracts\Tickets\DTOs\TicketCheckInResultDto.cs
    - Admin\Contracts\Tickets\Requests\CheckInTicketRequest.cs
    - Admin\Services\Api\TicketsApiClient.cs
    - Admin\wwwroot\js\checkin.js
    - Admin\Components\Pages\CheckIn.razor

  Archivos que modifica:
    - Admin\Global.cs                                    (+ usings Tickets.DTOs/Requests)
    - Admin\Extensions\ApplicationDependencyInjectionExtension.cs  (+ AddHttpClient<TicketsApiClient>)
    - Admin\Components\App.razor                          (+ <script src="js/checkin.js">)
    - Admin\Components\Layout\Sidebar.razor                (+ enlace "Check-in" bajo Registrations)

  Idempotente: cada cambio se detecta y se salta si ya esta aplicado.

.USO
  Ejecutar desde la raiz del repo:
    .\11-checkin-screen.ps1
#>

# ============================================================================
# Patch-File v3 (misma version robusta de los scripts 9 y 10).
# ============================================================================
function Patch-File {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] [string]$OldString,
        [Parameter(Mandatory = $true)] [string]$NewString,
        [string]$Description = ""
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "SKIP: archivo no encontrado -> $Path" -ForegroundColor Yellow
        return
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $rawContent = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    $usesCrlf = $rawContent.Contains("`r`n")

    $content = $rawContent -replace "`r`n", "`n"
    $oldNormalized = $OldString -replace "`r`n", "`n"
    $newNormalized = $NewString -replace "`r`n", "`n"

    if ($content.Contains($newNormalized) -and -not $content.Contains($oldNormalized)) {
        Write-Host "SKIP: ya aplicado -> $Path ($Description)" -ForegroundColor DarkGray
        return
    }

    $occurrences = ([regex]::Matches($content, [regex]::Escape($oldNormalized))).Count

    if ($occurrences -eq 0) {
        Write-Host "SKIP: anchor no encontrado -> $Path ($Description)" -ForegroundColor Red
        return
    }

    if ($occurrences -gt 1) {
        Write-Host "SKIP: anchor ambiguo, aparece $occurrences veces -> $Path ($Description)" -ForegroundColor Red
        return
    }

    $newContent = $content.Replace($oldNormalized, $newNormalized)

    if ($usesCrlf) {
        $newContent = $newContent -replace "`n", "`r`n"
    }

    [System.IO.File]::WriteAllText($Path, $newContent, $utf8NoBom)
    Write-Host "OK: aplicado -> $Path ($Description)" -ForegroundColor Green
}

# ============================================================================
# New-FileIfMissing (misma version de Script 10).
# ============================================================================
function New-FileIfMissing {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] [string]$Content,
        [string]$Description = ""
    )

    if (Test-Path -LiteralPath $Path) {
        Write-Host "SKIP: ya existe -> $Path ($Description)" -ForegroundColor DarkGray
        return
    }

    $directory = Split-Path -Path $Path -Parent
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $normalizedContent = $Content -replace "`n", "`r`n"
    [System.IO.File]::WriteAllText($Path, $normalizedContent, $utf8NoBom)
    Write-Host "OK: creado -> $Path ($Description)" -ForegroundColor Green
}

$ErrorActionPreference = "Stop"

$AdminGlobalPath   = ".\Alakai.FestivalManager.Admin\Global.cs"
$AdminDiPath       = ".\Alakai.FestivalManager.Admin\Extensions\ApplicationDependencyInjectionExtension.cs"
$AppRazorPath      = ".\Alakai.FestivalManager.Admin\Components\App.razor"
$SidebarPath       = ".\Alakai.FestivalManager.Admin\Components\Layout\Sidebar.razor"
$CheckInDtoPath    = ".\Alakai.FestivalManager.Admin\Contracts\Tickets\DTOs\TicketCheckInResultDto.cs"
$CheckInReqPath    = ".\Alakai.FestivalManager.Admin\Contracts\Tickets\Requests\CheckInTicketRequest.cs"
$TicketsClientPath = ".\Alakai.FestivalManager.Admin\Services\Api\TicketsApiClient.cs"
$CheckInJsPath     = ".\Alakai.FestivalManager.Admin\wwwroot\js\checkin.js"
$CheckInRazorPath  = ".\Alakai.FestivalManager.Admin\Components\Pages\CheckIn.razor"

# ----------------------------------------------------------------------------
# 1. DTO y Request espejo (igual que el resto de Contracts de la Admin)
# ----------------------------------------------------------------------------
New-FileIfMissing `
    -Path $CheckInDtoPath `
    -Description "TicketCheckInResultDto" `
    -Content @'
namespace Alakai.FestivalManager.Admin.Contracts.Tickets.DTOs;

public class TicketCheckInResultDto
{
    public Guid RegistrationId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string PassTypeName { get; set; } = string.Empty;
    public string? LevelName { get; set; }
    public bool AlreadyCheckedIn { get; set; }
    public DateTime CheckedInAt { get; set; }
}
'@

New-FileIfMissing `
    -Path $CheckInReqPath `
    -Description "CheckInTicketRequest" `
    -Content @'
namespace Alakai.FestivalManager.Admin.Contracts.Tickets.Requests;

public class CheckInTicketRequest
{
    public string Token { get; set; } = string.Empty;
}
'@

# ----------------------------------------------------------------------------
# 2. TicketsApiClient (mismo patron que EmailNotificationApiClient / DiscountCodeApiClient)
# ----------------------------------------------------------------------------
New-FileIfMissing `
    -Path $TicketsClientPath `
    -Description "TicketsApiClient" `
    -Content @'
using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class TicketsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public TicketsApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
    {
        _httpClient = httpClient;
        _adminTokenProvider = adminTokenProvider;
    }

    private async Task AttachAuthHeaderAsync()
    {
        string? adminToken = await _adminTokenProvider.GetValidAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(adminToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        }
    }

    public async Task<ApiResponse<TicketCheckInResultDto>> CheckInAsync(string token, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/tickets/checkin", new CheckInTicketRequest { Token = token }, cancellationToken);

        ApiResponse<TicketCheckInResultDto>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<TicketCheckInResultDto>>(cancellationToken: cancellationToken);

        return response ?? new ApiResponse<TicketCheckInResultDto>
        {
            Success = false,
            Message = "Unexpected error contacting the server.",
            Data = null,
            Errors = ["No response received."]
        };
    }
}
'@

# ----------------------------------------------------------------------------
# 3. JS interop para html5-qrcode (carga la libreria bajo demanda desde CDN,
#    solo cuando alguien entra en /checkin - no en cada pagina de la Admin)
# ----------------------------------------------------------------------------
New-FileIfMissing `
    -Path $CheckInJsPath `
    -Description "checkin.js" `
    -Content @'
window.ticketCheckIn = (function () {
    let html5QrCode = null;
    let dotNetRef = null;
    let libraryLoadingPromise = null;

    function ensureLibraryLoaded() {
        if (window.Html5Qrcode) {
            return Promise.resolve();
        }

        if (!libraryLoadingPromise) {
            libraryLoadingPromise = new Promise((resolve, reject) => {
                const script = document.createElement('script');
                script.src = 'https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js';
                script.onload = () => resolve();
                script.onerror = () => reject(new Error('Could not load the QR scanning library.'));
                document.head.appendChild(script);
            });
        }

        return libraryLoadingPromise;
    }

    async function start(elementId, dotNetObjectRef) {
        await ensureLibraryLoaded();
        dotNetRef = dotNetObjectRef;

        if (html5QrCode) {
            await stop();
        }

        html5QrCode = new Html5Qrcode(elementId);

        const config = { fps: 10, qrbox: { width: 250, height: 250 } };

        await html5QrCode.start(
            { facingMode: "environment" },
            config,
            (decodedText) => {
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnQrCodeScanned', decodedText);
                }
            },
            () => {
                // Errores de "no hay QR en el encuadre este frame" - esperado, se ignoran.
            }
        );
    }

    function pause() {
        if (html5QrCode) {
            try { html5QrCode.pause(true); } catch (e) { /* ignore */ }
        }
    }

    function resume() {
        if (html5QrCode) {
            try { html5QrCode.resume(); } catch (e) { /* ignore */ }
        }
    }

    async function stop() {
        if (html5QrCode) {
            try {
                await html5QrCode.stop();
                html5QrCode.clear();
            } catch (e) {
                // ignore
            }
            html5QrCode = null;
        }
        dotNetRef = null;
    }

    return { start, pause, resume, stop };
})();
'@

# ----------------------------------------------------------------------------
# 4. Pagina /checkin
# ----------------------------------------------------------------------------
New-FileIfMissing `
    -Path $CheckInRazorPath `
    -Description "CheckIn.razor" `
    -Content @'
@page "/checkin"

@inject TicketsApiClient TicketsApiClient
@inject IJSRuntime JsRuntime
@implements IAsyncDisposable

<PageHeader Title="Operations" pTitle="Check-in"></PageHeader>

<div class="flex flex-col gap-4 min-h-[calc(100vh-212px)]">
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

        <div class="card">
            <div class="p-4">
                <h6 class="mb-3 font-semibold">Escanea el QR de la entrada</h6>
                <div id="qr-reader" class="w-full max-w-sm mx-auto"></div>

                @if (isScannerPaused)
                {
                    <div class="mt-3 text-center">
                        <button type="button" class="btn bg-purple text-white" @onclick="ResumeScannerAsync">
                            Escanear siguiente
                        </button>
                    </div>
                }
            </div>
        </div>

        <div class="card">
            <div class="p-4 flex flex-col gap-4">
                <div>
                    <h6 class="mb-2 font-semibold">O introduce el codigo manualmente</h6>
                    <p class="text-xs text-black/50 dark:text-white/50 mb-2">
                        Si la camara no funciona, escanea el QR con la camara del movil o Google Lens y pega aqui el texto que decodifica.
                    </p>
                    <div class="flex flex-col sm:flex-row gap-2">
                        <input class="form-input flex-1" placeholder="Codigo del ticket..." @bind="manualToken" @bind:event="oninput" />
                        <button type="button" class="btn bg-purple text-white" disabled="@isChecking" @onclick="() => ProcessTokenAsync(manualToken)">
                            Comprobar
                        </button>
                    </div>
                </div>

                @if (isChecking)
                {
                    <div class="text-sm text-black/50 dark:text-white/50">Comprobando...</div>
                }

                @if (lastResult is not null)
                {
                    @if (lastResult.Success && lastResult.Data is not null && !lastResult.Data.AlreadyCheckedIn)
                    {
                        <div class="rounded-lg border border-green-200 bg-green-50 p-4 text-green-800">
                            <div class="font-semibold">Check-in confirmado</div>
                            <div class="mt-1 text-sm">@lastResult.Data.ParticipantName</div>
                            <div class="text-sm">@lastResult.Data.EventName - @(string.IsNullOrWhiteSpace(lastResult.Data.LevelName) ? lastResult.Data.PassTypeName : $"{lastResult.Data.PassTypeName} - {lastResult.Data.LevelName}")</div>
                            <div class="text-xs mt-1">@lastResult.Data.CheckedInAt.ToString("dd/MM/yyyy HH:mm")</div>
                        </div>
                    }
                    else if (lastResult.Success && lastResult.Data is not null && lastResult.Data.AlreadyCheckedIn)
                    {
                        <div class="rounded-lg border border-amber-200 bg-amber-50 p-4 text-amber-800">
                            <div class="font-semibold">Ya se habia hecho check-in</div>
                            <div class="mt-1 text-sm">@lastResult.Data.ParticipantName</div>
                            <div class="text-sm">@lastResult.Data.EventName - @(string.IsNullOrWhiteSpace(lastResult.Data.LevelName) ? lastResult.Data.PassTypeName : $"{lastResult.Data.PassTypeName} - {lastResult.Data.LevelName}")</div>
                            <div class="text-xs mt-1">Check-in original: @lastResult.Data.CheckedInAt.ToString("dd/MM/yyyy HH:mm")</div>
                        </div>
                    }
                    else
                    {
                        <div class="rounded-lg border border-red-200 bg-red-50 p-4 text-red-800">
                            <div class="font-semibold">Codigo no valido</div>
                            <div class="mt-1 text-sm">@(lastResult.Message ?? "El QR no corresponde a ninguna entrada valida.")</div>
                        </div>
                    }
                }
            </div>
        </div>

    </div>
</div>

@code {
    private string manualToken = string.Empty;
    private bool isChecking;
    private bool isScannerPaused;
    private bool scannerStarted;
    private ApiResponse<TicketCheckInResultDto>? lastResult;
    private DotNetObjectReference<CheckIn>? dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetRef = DotNetObjectReference.Create(this);

            try
            {
                await JsRuntime.InvokeVoidAsync("ticketCheckIn.start", "qr-reader", dotNetRef);
                scannerStarted = true;
            }
            catch
            {
                // La camara puede no estar disponible (sin permisos, sin HTTPS, sin camara) -
                // el usuario siempre puede usar la entrada manual de mas abajo.
            }
        }
    }

    [JSInvokable]
    public async Task OnQrCodeScanned(string decodedText)
    {
        isScannerPaused = true;
        await JsRuntime.InvokeVoidAsync("ticketCheckIn.pause");
        await ProcessTokenAsync(decodedText);
    }

    private async Task ProcessTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || isChecking)
        {
            return;
        }

        isChecking = true;
        lastResult = null;
        StateHasChanged();

        try
        {
            lastResult = await TicketsApiClient.CheckInAsync(token.Trim());
        }
        catch (Exception ex)
        {
            lastResult = new ApiResponse<TicketCheckInResultDto>
            {
                Success = false,
                Message = ex.Message,
                Data = null,
                Errors = [ex.Message]
            };
        }
        finally
        {
            isChecking = false;
            StateHasChanged();
        }
    }

    private async Task ResumeScannerAsync()
    {
        lastResult = null;
        manualToken = string.Empty;
        isScannerPaused = false;

        if (scannerStarted)
        {
            await JsRuntime.InvokeVoidAsync("ticketCheckIn.resume");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (scannerStarted)
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("ticketCheckIn.stop");
            }
            catch
            {
                // El circuito puede haberse cerrado ya - no pasa nada.
            }
        }

        dotNetRef?.Dispose();
    }
}
'@

# ----------------------------------------------------------------------------
# 5. Global usings de la Admin
# ----------------------------------------------------------------------------
Patch-File `
    -Path $AdminGlobalPath `
    -Description "Admin Global.cs: Tickets.DTOs / Tickets.Requests" `
    -OldString @'
global using Alakai.FestivalManager.Admin.Contracts.Registrations.DTOs;
global using Alakai.FestivalManager.Admin.Contracts.Registrations.Requests;
global using Alakai.FestivalManager.Admin.Contracts.Registrations.Responses;
global using Alakai.FestivalManager.Admin.Contracts.UserPanel.DTOs;
'@ `
    -NewString @'
global using Alakai.FestivalManager.Admin.Contracts.Registrations.DTOs;
global using Alakai.FestivalManager.Admin.Contracts.Registrations.Requests;
global using Alakai.FestivalManager.Admin.Contracts.Registrations.Responses;
global using Alakai.FestivalManager.Admin.Contracts.Tickets.DTOs;
global using Alakai.FestivalManager.Admin.Contracts.Tickets.Requests;
global using Alakai.FestivalManager.Admin.Contracts.UserPanel.DTOs;
'@

# ----------------------------------------------------------------------------
# 6. Registro del HttpClient para TicketsApiClient
# ----------------------------------------------------------------------------
Patch-File `
    -Path $AdminDiPath `
    -Description "AddHttpClient<TicketsApiClient>" `
    -OldString @'
        services.AddHttpClient<EmailNotificationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });
'@ `
    -NewString @'
        services.AddHttpClient<EmailNotificationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<TicketsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });
'@

# ----------------------------------------------------------------------------
# 7. Carga global de checkin.js (define window.ticketCheckIn - no carga
#    html5-qrcode todavia, eso solo pasa cuando se visita /checkin)
# ----------------------------------------------------------------------------
Patch-File `
    -Path $AppRazorPath `
    -Description "App.razor: script checkin.js" `
    -OldString @'
    <script src="assets/js/pages/festivals-table.js"></script>
    <script src="_framework/blazor.web.js"></script>
'@ `
    -NewString @'
    <script src="assets/js/pages/festivals-table.js"></script>
    <script src="js/checkin.js"></script>
    <script src="_framework/blazor.web.js"></script>
'@

# ----------------------------------------------------------------------------
# 8. Enlace en el menu (grupo Operations, debajo de Registrations)
# ----------------------------------------------------------------------------
Patch-File `
    -Path $SidebarPath `
    -Description "Sidebar.razor: enlace Check-in" `
    -OldString @'
                        <li><NavLink href="/registrations">Registrations</NavLink></li>
'@ `
    -NewString @'
                        <li><NavLink href="/registrations">Registrations</NavLink></li>
                        <li><NavLink href="/checkin">Check-in</NavLink></li>
'@

Write-Host ""
Write-Host "Paso B completado." -ForegroundColor Cyan
Write-Host ""
Write-Host "Deberias ver 9 lineas 'OK' en total (5 archivos nuevos: DTO, Request," -ForegroundColor Yellow
Write-Host "TicketsApiClient, checkin.js, CheckIn.razor; 4 patches: Global.cs," -ForegroundColor Yellow
Write-Host "DI extension, App.razor, Sidebar.razor). Si ves SKIP por anchor no" -ForegroundColor Yellow
Write-Host "encontrado, peguemela literal." -ForegroundColor Yellow
Write-Host ""
Write-Host "VERIFICACION:" -ForegroundColor Cyan
Write-Host "  1. dotnet build debe compilar limpio (Admin y Api)." -ForegroundColor Cyan
Write-Host "  2. Entra en la Admin, menu Operations -> Check-in. El navegador debe" -ForegroundColor Cyan
Write-Host "     pedir permiso de camara (necesita HTTPS o localhost para funcionar" -ForegroundColor Cyan
Write-Host "     - si la app corre en HTTP puro en local puede que el navegador" -ForegroundColor Cyan
Write-Host "     bloquee la camara; en produccion con HTTPS no deberia pasar)." -ForegroundColor Cyan
Write-Host "  3. Escanea un ticket generado antes -> debe salir el aviso verde con" -ForegroundColor Cyan
Write-Host "     el nombre, pase y hora. Escanealo otra vez -> aviso amarillo de" -ForegroundColor Cyan
Write-Host "     'ya se habia hecho check-in'." -ForegroundColor Cyan
Write-Host "  4. Prueba tambien la entrada manual pegando el texto del QR (leelo con" -ForegroundColor Cyan
Write-Host "     la camara del movil o Google Lens) y pulsando Comprobar." -ForegroundColor Cyan
Write-Host "  5. Prueba con un texto inventado en el campo manual -> debe salir el" -ForegroundColor Cyan
Write-Host "     aviso rojo de codigo no valido." -ForegroundColor Cyan