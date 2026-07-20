# Fix-Step67-FinalLoginFix.ps1
#
# Arreglo definitivo de las 3 cosas pendientes, todas con la misma causa raiz
# en 2 de los 3 casos (clases de Tailwind sin regla CSS real compilada):
#
#   1) Boton de Google sin borde: "border-2" no se usaba en ningun otro sitio
#      del proyecto -> sin regla CSS real. Se sustituye por CSS en linea.
#   2) Logo grande y sin margen: "max-h-10" tampoco tenia regla CSS real
#      (era la primera vez que se usaba, en mi propio cambio anterior).
#      Se sustituye por CSS en linea con tamano y margen explicitos.
#   3) Logo que desaparece: se simplifica quitando la dependencia entre
#      App.razor y las paginas de Login via CascadingValue (mas fragil) -
#      cada pagina de Login resuelve y persiste SU PROPIO logo de forma
#      autonoma, con el mismo mecanismo de PersistentComponentState pero
#      sin pasar por ningun componente intermedio.
#
# Ejecutar DESPUES de Fix-Step42, Fix-Step63, Fix-Step64, Fix-Step65, Fix-Step66.
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

$authLoginPath = "Alakai.FestivalManager.Admin/Components/Pages/Auth/Login.razor"
$adminLoginPath = "Alakai.FestivalManager.Admin/Components/Pages/AdminAuth/Login.razor"

$results = @()

# ── 1. Boton de Google: borde con CSS en linea ──────────────────────────────
$results += Patch-File -Path $authLoginPath -Description "Boton de Google: borde negro con CSS en linea" -OldString @'
                    <button type="button"
                            disabled="@IsLoading"
                            @onclick="SignInWithGoogleAsync"
                            class="flex items-center justify-center w-full gap-3 px-4 py-3
                       bg-white border-2 border-black rounded-full
                       hover:bg-slate-50
                       transition-all duration-200">
'@ -NewString @'
                    <button type="button"
                            disabled="@IsLoading"
                            @onclick="SignInWithGoogleAsync"
                            style="border: 2px solid #000; border-radius: 9999px;"
                            class="flex items-center justify-center w-full gap-3 px-4 py-3
                       bg-white hover:bg-slate-50
                       transition-all duration-200">
'@

# ── 2. Logo: tamano y margen con CSS en linea (Auth) ────────────────────────
$results += Patch-File -Path $authLoginPath -Description "Auth: logo con tamano/margen en CSS en linea" -OldString @'
        @if (!string.IsNullOrWhiteSpace(Branding?.LogoUrl))
        {
            <div class="flex items-center justify-center h-10 mb-6">
                <img src="@Branding.LogoUrl" alt="@Branding.Name" class="object-contain max-w-full max-h-10" />
            </div>
        }
        <MudText Typo="Typo.h1" Class="mb-2 text-2xl font-semibold text-center dark:text-white">Sign In</MudText>
'@ -NewString @'
        @if (!string.IsNullOrWhiteSpace(Branding?.LogoUrl))
        {
            <div style="display:flex; align-items:center; justify-content:center; height:40px; margin-bottom:24px;">
                <img src="@Branding.LogoUrl" alt="@Branding.Name" style="max-width:100%; max-height:40px; object-fit:contain;" />
            </div>
        }
        <MudText Typo="Typo.h1" Class="mb-2 text-2xl font-semibold text-center dark:text-white">Sign In</MudText>
'@

# ── 3. Logo: tamano y margen con CSS en linea (AdminAuth) ───────────────────
$results += Patch-File -Path $adminLoginPath -Description "AdminAuth: logo con tamano/margen en CSS en linea" -OldString @'
        @if (!string.IsNullOrWhiteSpace(Branding?.LogoUrl))
        {
            <div class="flex items-center justify-center h-10 mb-6">
                <img src="@Branding.LogoUrl" alt="@Branding.Name" class="object-contain max-w-full max-h-10" />
            </div>
        }
        <MudText Typo="Typo.h1" Class="mb-2 text-2xl font-semibold text-center dark:text-white">Admin Sign In</MudText>
'@ -NewString @'
        @if (!string.IsNullOrWhiteSpace(Branding?.LogoUrl))
        {
            <div style="display:flex; align-items:center; justify-content:center; height:40px; margin-bottom:24px;">
                <img src="@Branding.LogoUrl" alt="@Branding.Name" style="max-width:100%; max-height:40px; object-fit:contain;" />
            </div>
        }
        <MudText Typo="Typo.h1" Class="mb-2 text-2xl font-semibold text-center dark:text-white">Admin Sign In</MudText>
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (CSS). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 4. Simplificar: cada pagina de Login resuelve su propio logo, sin CascadingValue ──
# 4a. App.razor: revertir el CascadingValue (ya no hace falta)
$appPath = "Alakai.FestivalManager.Admin/Components/App.razor"
$resultApp = Patch-File -Path $appPath -Description "App.razor: quitar el CascadingValue (cada pagina resuelve lo suyo)" -OldString @'
    <CascadingValue Value="Branding">
        <Routes @rendermode="InteractiveServer" />
    </CascadingValue>
'@ -NewString @'
    <Routes @rendermode="InteractiveServer" />
'@

if (-not $resultApp) {
    Write-Host "`nNo se pudo revertir el CascadingValue en App.razor." -ForegroundColor Red
    exit 1
}

# 4b. Auth/Login.razor: resolver su propio Branding, con persistencia propia
$resultAuthCode = Patch-File -Path $authLoginPath -Description "Auth: resolver Branding de forma autonoma con persistencia propia" -OldString @'
@implements IAsyncDisposable

@code {
    [CascadingParameter]
    public Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? Branding { get; set; }
}
'@ -NewString @'
@implements IAsyncDisposable
@inject Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor
@inject Alakai.FestivalManager.Admin.Services.Api.PublicRegistrationApiClient PublicApi
@inject PersistentComponentState ApplicationState
'@

if (-not $resultAuthCode) {
    Write-Host "`nNo se pudo aplicar el patch de Auth/Login.razor (inyecciones)." -ForegroundColor Red
    exit 1
}

Write-Host "`nParte 1 aplicada. Aplicando la logica de resolucion de Branding en cada pagina..." -ForegroundColor Green

# 4c. Auth/Login.razor: anadir la propiedad Branding + OnInitializedAsync + persistencia
$resultAuthLogic = Patch-File -Path $authLoginPath -Description "Auth: anadir OnInitializedAsync con resolucion y persistencia de Branding" -OldString @'
    private DotNetObjectReference<Login>? _dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
'@ -NewString @'
    private DotNetObjectReference<Login>? _dotNetRef;

    private Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? Branding;
    private PersistingComponentStateSubscription _persistingSubscription;

    protected override async Task OnInitializedAsync()
    {
        _persistingSubscription = ApplicationState.RegisterOnPersisting(PersistBranding);

        if (ApplicationState.TryTakeFromJson("branding", out Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? restored))
        {
            Branding = restored;
            return;
        }

        try
        {
            string? host = HttpContextAccessor.HttpContext?.Request.Host.Host;

            if (!string.IsNullOrWhiteSpace(host))
            {
                Branding = await PublicApi.GetFestivalByDomainAsync(host);
            }
        }
        catch
        {
            // Sin marca especifica para este dominio - se usan los valores por defecto.
        }
    }

    private Task PersistBranding()
    {
        ApplicationState.PersistAsJson("branding", Branding);
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
'@

if (-not $resultAuthLogic) {
    Write-Host "`nNo se pudo aplicar el patch de Auth/Login.razor (OnInitializedAsync)." -ForegroundColor Red
    exit 1
}

$resultAuthDispose = Patch-File -Path $authLoginPath -Description "Auth: liberar la suscripcion de persistencia en Dispose" -OldString @'
    public ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();

        return ValueTask.CompletedTask;
    }
}
'@ -NewString @'
    public ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        _persistingSubscription.Dispose();

        return ValueTask.CompletedTask;
    }
}
'@

if (-not $resultAuthDispose) {
    Write-Host "`nNo se pudo aplicar el patch de Auth/Login.razor (Dispose)." -ForegroundColor Red
    exit 1
}

Write-Host "Auth/Login.razor completo." -ForegroundColor Green

# ── 5. AdminAuth/Login.razor: lo mismo, de forma autonoma ───────────────────
$resultAdminInject = Patch-File -Path $adminLoginPath -Description "AdminAuth: anadir inyecciones necesarias" -OldString @'
@page "/login"
@layout AuthLayout
@attribute [AllowAnonymous]
'@ -NewString @'
@page "/login"
@layout AuthLayout
@attribute [AllowAnonymous]
@implements IDisposable
@inject Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor
@inject Alakai.FestivalManager.Admin.Services.Api.PublicRegistrationApiClient PublicApi
@inject PersistentComponentState ApplicationState
'@

if (-not $resultAdminInject) {
    Write-Host "`nNo se pudo aplicar el patch de AdminAuth/Login.razor (inyecciones)." -ForegroundColor Red
    exit 1
}

$resultAdminCode = Patch-File -Path $adminLoginPath -Description "AdminAuth: reemplazar CascadingParameter por resolucion propia" -OldString @'
@code {
    [CascadingParameter]
    public Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? Branding { get; set; }

    [SupplyParameterFromQuery(Name = "error")]
    public string? Error { get; set; }
'@ -NewString @'
@code {
    private Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? Branding;
    private PersistingComponentStateSubscription _persistingSubscription;

    protected override async Task OnInitializedAsync()
    {
        _persistingSubscription = ApplicationState.RegisterOnPersisting(PersistBranding);

        if (ApplicationState.TryTakeFromJson("branding", out Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? restored))
        {
            Branding = restored;
            return;
        }

        try
        {
            string? host = HttpContextAccessor.HttpContext?.Request.Host.Host;

            if (!string.IsNullOrWhiteSpace(host))
            {
                Branding = await PublicApi.GetFestivalByDomainAsync(host);
            }
        }
        catch
        {
            // Sin marca especifica para este dominio - se usan los valores por defecto.
        }
    }

    private Task PersistBranding()
    {
        ApplicationState.PersistAsJson("branding", Branding);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _persistingSubscription.Dispose();
    }

    [SupplyParameterFromQuery(Name = "error")]
    public string? Error { get; set; }
'@

if (-not $resultAdminCode) {
    Write-Host "`nNo se pudo aplicar el patch de AdminAuth/Login.razor (codigo)." -ForegroundColor Red
    exit 1
}

Write-Host "AdminAuth/Login.razor completo." -ForegroundColor Green
Write-Host "" 
Write-Host "TODO aplicado. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Nota: el Paso 65 (que anadia esto mismo en App.razor) queda sin efecto -" -ForegroundColor Yellow
Write-Host "este script ya revierte esa parte y lo hace de forma independiente en cada pagina." -ForegroundColor Yellow