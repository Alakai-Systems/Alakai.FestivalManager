# Fix-Step63-LoginPageImprovements.ps1
#
# Tres cambios en las paginas de Login (Admin y User Panel):
#   1) Divisor minimalista "o" en vez del texto "Or with Email".
#   2) Borde negro solido + redondeado, y boton de mostrar/ocultar contrasena.
#   3) Logo del festival encima del formulario, resuelto por el dominio de la
#      URL (reutiliza la resolucion del Paso 42, via CascadingValue desde
#      App.razor - sin llamadas de red adicionales).
#
# Ejecutar DESPUES de Fix-Step42.
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

# ── 1. Api: anadir LogoUrl al endpoint by-domain ─────────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Api/Controllers/PublicFestivalsController.cs" `
    -Description "Anadir LogoUrl a la respuesta de by-domain" -OldString @'
        return Ok(new
        {
            Name = festival.Name,
            FaviconUrl = festival.FaviconUrl
        });
    }
}
'@ -NewString @'
        return Ok(new
        {
            Name = festival.Name,
            FaviconUrl = festival.FaviconUrl,
            LogoUrl = festival.LogoUrl
        });
    }
}
'@

# ── 2. Admin: anadir LogoUrl al DTO ──────────────────────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs" `
    -Description "Anadir LogoUrl al record PublicFestivalBrandingDto" -OldString @'
public record PublicFestivalBrandingDto(string Name, string? FaviconUrl);
'@ -NewString @'
public record PublicFestivalBrandingDto(string Name, string? FaviconUrl, string? LogoUrl);
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (backend). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 3. App.razor: exponer Branding a los hijos via CascadingValue ───────────
$appRazorResult = Patch-File -Path "Alakai.FestivalManager.Admin/Components/App.razor" `
    -Description "Envolver Routes en CascadingValue para compartir Branding" -OldString @'
    <Routes @rendermode="InteractiveServer" />
'@ -NewString @'
    <CascadingValue Value="Branding">
        <Routes @rendermode="InteractiveServer" />
    </CascadingValue>
'@

if (-not $appRazorResult) {
    Write-Host "`nNo se pudo aplicar el patch de App.razor." -ForegroundColor Red
    exit 1
}

# ── 4. Login de participantes (User Panel) ───────────────────────────────────
$authLoginPath = "Alakai.FestivalManager.Admin/Components/Pages/Auth/Login.razor"

$results2 = @()

$results2 += Patch-File -Path $authLoginPath -Description "Recibir Branding en cascada" -OldString @'
@implements IAsyncDisposable
'@ -NewString @'
@implements IAsyncDisposable

@code {
    [CascadingParameter]
    public Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? Branding { get; set; }
}
'@

$results2 += Patch-File -Path $authLoginPath -Description "Borde negro solido + logo encima del titulo" -OldString @'
    <div class="max-w-[550px] flex-none w-full bg-white border border-black/10 p-6 sm:p-10 lg:px-10 lg:py-14 rounded-2xl loginform shadow-3xl shadow-black/10 dark:bg-darklight dark:border-darkborder">
        <MudText Typo="Typo.h1" Class="mb-2 text-2xl font-semibold text-center dark:text-white">Sign In</MudText>
'@ -NewString @'
    <div class="max-w-[550px] flex-none w-full bg-white border-2 border-black p-6 sm:p-10 lg:px-10 lg:py-14 rounded-2xl loginform shadow-3xl shadow-black/10 dark:bg-darklight dark:border-white">
        @if (!string.IsNullOrWhiteSpace(Branding?.LogoUrl))
        {
            <img src="@Branding.LogoUrl" alt="@Branding.Name" class="max-h-16 mx-auto mb-6 object-contain" />
        }
        <MudText Typo="Typo.h1" Class="mb-2 text-2xl font-semibold text-center dark:text-white">Sign In</MudText>
'@

$results2 += Patch-File -Path $authLoginPath -Description "Divisor minimalista o en vez del texto" -OldString @'
            <div class="flex items-center mb-7">
                <div class="w-full h-[2px] bg-black/10 dark:bg-darkborder"></div>
                <div class="px-5 capitalize text-muted whitespace-nowrap dark:text-darkmuted">Or with Email</div>
                <div class="w-full h-[2px] bg-black/10 dark:bg-darkborder"></div>
            </div>
'@ -NewString @'
            <div class="flex items-center mb-7">
                <div class="w-full h-[2px] bg-black/10 dark:bg-darkborder"></div>
                <div class="px-4 text-muted whitespace-nowrap dark:text-darkmuted">o</div>
                <div class="w-full h-[2px] bg-black/10 dark:bg-darkborder"></div>
            </div>
'@

$results2 += Patch-File -Path $authLoginPath -Description "Boton de mostrar/ocultar contrasena" -OldString @'
                <div>
                    <InputText @bind-Value="LoginModel.Password" placeholder="Password" type="password" class="form-input" />
                </div>
'@ -NewString @'
                <div class="relative">
                    <InputText id="userPanelPassword" @bind-Value="LoginModel.Password" placeholder="Password" type="password" class="form-input pr-11" />
                    <button type="button" onclick="const i=document.getElementById('userPanelPassword'); const isPw=i.type==='password'; i.type=isPw?'text':'password'; this.querySelector('i').className=isPw?'ri-eye-line':'ri-eye-off-line';" class="absolute -translate-y-1/2 right-3 top-1/2 text-black/50 dark:text-white/60">
                        <i class="ri-eye-off-line"></i>
                    </button>
                </div>
'@

if ($results2 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (login de participantes). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 5. Login del Admin (equipo organizador) ──────────────────────────────────
$adminLoginPath = "Alakai.FestivalManager.Admin/Components/Pages/AdminAuth/Login.razor"

$results3 = @()

$results3 += Patch-File -Path $adminLoginPath -Description "Borde negro solido + logo encima del titulo" -OldString @'
    <div class="max-w-[550px] flex-none w-full bg-white border border-black/10 p-6 sm:p-10 lg:px-10 lg:py-14 rounded-2xl loginform shadow-3xl shadow-black/10 dark:bg-darklight dark:border-darkborder">
        <MudText Typo="Typo.h1" Class="mb-2 text-2xl font-semibold text-center dark:text-white">Admin Sign In</MudText>
'@ -NewString @'
    <div class="max-w-[550px] flex-none w-full bg-white border-2 border-black p-6 sm:p-10 lg:px-10 lg:py-14 rounded-2xl loginform shadow-3xl shadow-black/10 dark:bg-darklight dark:border-white">
        @if (!string.IsNullOrWhiteSpace(Branding?.LogoUrl))
        {
            <img src="@Branding.LogoUrl" alt="@Branding.Name" class="max-h-16 mx-auto mb-6 object-contain" />
        }
        <MudText Typo="Typo.h1" Class="mb-2 text-2xl font-semibold text-center dark:text-white">Admin Sign In</MudText>
'@

$results3 += Patch-File -Path $adminLoginPath -Description "Boton de mostrar/ocultar contrasena" -OldString @'
            <div>
                <input name="password" type="password" placeholder="Password" class="form-input" required />
            </div>
'@ -NewString @'
            <div class="relative">
                <input id="adminPassword" name="password" type="password" placeholder="Password" class="form-input pr-11" required />
                <button type="button" onclick="const i=document.getElementById('adminPassword'); const isPw=i.type==='password'; i.type=isPw?'text':'password'; this.querySelector('i').className=isPw?'ri-eye-line':'ri-eye-off-line';" class="absolute -translate-y-1/2 right-3 top-1/2 text-black/50 dark:text-white/60">
                    <i class="ri-eye-off-line"></i>
                </button>
            </div>
'@

$results3 += Patch-File -Path $adminLoginPath -Description "Recibir Branding en cascada" -OldString @'
@code {
    [SupplyParameterFromQuery(Name = "error")]
    public string? Error { get; set; }
'@ -NewString @'
@code {
    [CascadingParameter]
    public Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? Branding { get; set; }

    [SupplyParameterFromQuery(Name = "error")]
    public string? Error { get; set; }
'@

if ($results3 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (login de admin). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nTodo aplicado. dotnet build para confirmar." -ForegroundColor Green
Write-Host "El logo solo aparecera si el festival tiene un dominio propio configurado" -ForegroundColor Yellow
Write-Host "(Custom Domain) y ese festival tiene un Logo URL puesto en sus ajustes." -ForegroundColor Yellow