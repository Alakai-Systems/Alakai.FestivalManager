# Fix-Step42-DomainBasedBranding.ps1
#
# Arreglo de fondo: el favicon/titulo dependia de ActiveFestivalState (un
# concepto de DESPUES de iniciar sesion) y de datos por-pagina (Register,
# UserPanel) - tres mecanismos distintos, sin coordinarse entre si, con
# resultados inconsistentes segun la pagina y el momento.
#
# Se sustituye TODO por una unica resolucion, a nivel raiz (App.razor),
# basada en el DOMINIO de la peticion (HttpContext.Request.Host) - funciona
# igual en el login, en el Admin, en el formulario publico y en el panel de
# usuario, desde el primer render, sin depender de haber iniciado sesion.
#
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

    if ($normalizedContent.Contains($normalizedNew) -and -not $normalizedContent.Contains($normalizedOld)) {
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

# ── 1. Application: interfaz del repositorio ────────────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Application/Interfaces/Repositories/IFestivalRepository.cs" `
    -Description "Anadir GetByCustomDomainAsync a la interfaz" -OldString @'
    Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
'@ -NewString @'
    Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Festival?> GetByCustomDomainAsync(string domain, CancellationToken cancellationToken = default);
'@

# ── 2. Infrastructure: implementacion ────────────────────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Infrastructure/Repositories/FestivalRepository.cs" `
    -Description "Implementar GetByCustomDomainAsync" -OldString @'
    public async Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Slug == slug, cancellationToken);
    }
'@ -NewString @'
    public async Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Slug == slug, cancellationToken);
    }

    public async Task<Festival?> GetByCustomDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.CustomDomain == domain, cancellationToken);
    }
'@

# ── 3. Api: nuevo endpoint publico ───────────────────────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Api/Controllers/PublicFestivalsController.cs" `
    -Description "Anadir endpoint by-domain" -OldString @'
        return Ok(new
        {
            ActiveEditionId = active?.Id,
            HasAccommodation = hasAccommodation,
            TermsUrl = festival.TermsUrl,
            FaviconUrl = festival.FaviconUrl
        });
    }
}
'@ -NewString @'
        return Ok(new
        {
            ActiveEditionId = active?.Id,
            HasAccommodation = hasAccommodation,
            TermsUrl = festival.TermsUrl,
            FaviconUrl = festival.FaviconUrl
        });
    }

    [HttpGet("by-domain/{domain}")]
    public async Task<IActionResult> GetByDomain(string domain, CancellationToken cancellationToken)
    {
        Festival? festival = await _festivalRepository.GetByCustomDomainAsync(domain, cancellationToken);

        if (festival is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            Name = festival.Name,
            FaviconUrl = festival.FaviconUrl
        });
    }
}
'@

# ── 4. Admin: cliente + DTO ───────────────────────────────────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs" `
    -Description "Cliente: anadir GetByDomainAsync + PublicFestivalBrandingDto" -OldString @'
    public async Task<PublicFestivalSlugDto?> GetFestivalBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<PublicFestivalSlugDto>($"api/public/festivals/by-slug/{slug}", cancellationToken);
    }
'@ -NewString @'
    public async Task<PublicFestivalSlugDto?> GetFestivalBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<PublicFestivalSlugDto>($"api/public/festivals/by-slug/{slug}", cancellationToken);
    }

    public async Task<PublicFestivalBrandingDto?> GetFestivalByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PublicFestivalBrandingDto>($"api/public/festivals/by-domain/{domain}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }
'@

$results += Patch-File -Path "Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs" `
    -Description "Anadir el record PublicFestivalBrandingDto" -OldString @'
public record PublicFestivalSlugDto(Guid? ActiveEditionId, bool HasAccommodation, string? TermsUrl, string? FaviconUrl);
'@ -NewString @'
public record PublicFestivalSlugDto(Guid? ActiveEditionId, bool HasAccommodation, string? TermsUrl, string? FaviconUrl);

public record PublicFestivalBrandingDto(string Name, string? FaviconUrl);
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (backend). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nBackend listo. Aplicando App.razor (sobreescritura completa) y limpiando MainLayout/Register/UserPanel..." -ForegroundColor Green

# ── 5. Program.cs: registrar IHttpContextAccessor ───────────────────────────
$programResult = Patch-File -Path "Alakai.FestivalManager.Admin/Program.cs" `
    -Description "Registrar IHttpContextAccessor" -OldString @'
var builder = WebApplication.CreateBuilder(args);
'@ -NewString @'
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
'@

if (-not $programResult) {
    Write-Host "`nNo se pudo registrar IHttpContextAccessor en Program.cs - anadelo tu a mano:" -ForegroundColor Red
    Write-Host "  builder.Services.AddHttpContextAccessor();" -ForegroundColor Cyan
}

# ── 6. App.razor: sobreescritura completa con resolucion por dominio ───────
$appRazorPath = "Alakai.FestivalManager.Admin/Components/App.razor"
$appRazorContent = @'
@using Radzen.Blazor
@inject IHttpContextAccessor HttpContextAccessor
@inject Alakai.FestivalManager.Admin.Services.Api.PublicRegistrationApiClient PublicApi
<!DOCTYPE html>
<html lang="en" x-data="{ direction: $store.app.direction }" x-bind:dir="$store.app.direction" class="group/item" x-bind:data-mode="$store.app.mode" x-bind:data-sidebar="$store.app.sidebarMode">

<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />

    <title>@(Branding?.Name is not null ? $"{Branding.Name} - Alakai Festival Manager" : "Alakai Festival Manager")</title>

    @if (!string.IsNullOrWhiteSpace(Branding?.FaviconUrl))
    {
        <link rel="icon" href="@Branding.FaviconUrl">
    }
    else
    {
        <link rel="shortcut icon" href="assets/images/favicon.ico">
    }

    <link rel="stylesheet" href="assets/css/plugins.css">
    <link rel="stylesheet" href="assets/css/remixicon.css">
    <link rel="stylesheet" href="assets/css/tailwind.css">
    <link rel="stylesheet" href="assets/css/style.css">
    <link rel="stylesheet" href="assets/css/site.css">
    <link rel="stylesheet" href="assets/libs/tippy.js/tippy.css">
    <link rel="stylesheet" href="assets/libs/tippy.js/border.css">
    <link rel="stylesheet" href="assets/libs/fancybox/css/jquery.fancybox.css">
    <link rel="stylesheet" href="assets/libs/magnific-popup/magnific-popup.css">
    <link rel="stylesheet" href="_content/MudBlazor/MudBlazor.min.css" />
    <RadzenTheme Theme="standard" @rendermode="InteractiveServer" />
    <script src="_content/Radzen.Blazor/Radzen.Blazor.js"></script>
    <link rel="stylesheet" href="https://cdn.datatables.net/1.11.5/css/jquery.dataTables.min.css">
    <link rel="stylesheet" href="https://cdn.datatables.net/responsive/2.2.9/css/responsive.dataTables.min.css">

    <ImportMap />
    <HeadOutlet @rendermode="InteractiveServer" />
</head>

<body x-data="main"
      x-init="$store.app.hasCreative = window.location.href.includes('/creative'); $store.app.hasdetached = window.location.href.includes('/detached')"
      x-bind:class="[ $store.app.sidebar ? 'toggle-sidebar' : '', $store.app.fullscreen ? 'full' : '', $store.app.hasCreative ? 'detached ' : '', $store.app.hasdetached ? 'detached detached-simple ' : '', $store.app.layout ]"
      class="relative overflow-x-hidden text-sm antialiased font-normal text-black font-cerebri dark:text-white vertical">

    <Routes @rendermode="InteractiveServer" />

    <script src="assets/js/pages/alpine-collaspe.min.js"></script>
    <script src="assets/js/pages/alpine-persist.min.js"></script>
    <script src="assets/js/pages/alpine.min.js" defer></script>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="assets/libs/tippy.js/popper.min.js"></script>
    <script src="assets/libs/tippy.js/tippy-bundle.umd.min.js"></script>
    <script src="assets/js/pages/tooltips.init.js"></script>

    <script>
        window.toggleFullScreen = function () {
            if (document.fullscreenElement) {
                document.exitFullscreen();
            } else {
                document.documentElement.requestFullscreen();
            }
        };

        window.toggleSidebar = function () {
            document.body.classList.toggle('toggle-sidebar');
        };

        window.removeToggleSidebarClass = function () {
            document.body.classList.remove('toggle-sidebar');
        };

        window.getActiveFestivalId = function () {
            return localStorage.getItem('activeFestivalId');
        };

        window.setActiveFestivalId = function (id) {
            localStorage.setItem('activeFestivalId', id);
        };

        window.changeDirection = function (direction) {
            document.documentElement.setAttribute('dir', direction);
        };

        window.setBodyClass = function (className) {
            document.body.classList.remove('light', 'dark');
            document.body.classList.add(className);
        };

        window.addClickListener = function (dotNetObject) {
            document.body.addEventListener("click", function () {
                dotNetObject.invokeMethodAsync("HandleClickEvent");
            });
        };
    </script>

    <script src="assets/js/pages/index.global.min.js"></script>
    <script src="assets/js/main.js"></script>
    <script src="assets/js/pages/form-editor.js"></script>
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
    <script src="https://cdn.datatables.net/1.11.5/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/responsive/2.2.9/js/dataTables.responsive.min.js"></script>
    <script src="assets/js/pages/festivals-table.js"></script>
    <script src="_framework/blazor.web.js"></script>
</body>

</html>

@code {
    private Alakai.FestivalManager.Admin.Services.Api.PublicFestivalBrandingDto? Branding;

    protected override async Task OnInitializedAsync()
    {
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
}
'@

if (Test-Path $appRazorPath) {
    $rawContent = Get-Content -Path $appRazorPath -Raw
    if ($rawContent.Replace("`r`n","`n") -eq $appRazorContent.Replace("`r`n","`n")) {
        Write-Host "SKIP (ya aplicado): App.razor" -ForegroundColor Cyan
    } else {
        Set-Content -Path $appRazorPath -Value $appRazorContent -NoNewline
        Write-Host "OK: App.razor reescrito con resolucion por dominio" -ForegroundColor Green
    }
} else {
    Write-Host "SKIP (archivo no encontrado): $appRazorPath" -ForegroundColor Yellow
    exit 1
}

# ── 7. MainLayout.razor: quitar el mecanismo de ActiveFestivalState (ya no hace falta) ──
$mainLayoutPath = "Alakai.FestivalManager.Admin/Components/Layout/MainLayout.razor"
$mainLayoutContent = @'
@using Radzen.Blazor
@inherits LayoutComponentBase

<AuthorizeView Policy="AdminAccess">
    <Authorized>
        <div class="bg-[#f9fbfd] dark:bg-dark">
            <div class="bg-black min-h-[220px] sm:min-h-[250px] bg-bottom fixed hidden w-full -z-50 detached-img" style="background-image: url('assets/images/bg-main.png');"></div>

            <div x-cloak class="fixed inset-0 bg-black/60 dark:bg-dark/90 z-[999] lg:hidden" x-bind:class="{ 'hidden': !$store.app.sidebar }" x-on:click="$store.app.toggleSidebar()"></div>

            <div class="flex mx-auto main-container">
                <Sidebar />

                <div class="flex-1 main-content min-w-0">
                    <Topbar />

                    <div class="h-[calc(100vh-60px)] relative overflow-y-auto overflow-x-hidden p-4 space-y-4 detached-content min-w-0">
                        @Body
                        <Footer />
                    </div>
                </div>
            </div>
        </div>

        <RadzenComponents />
    </Authorized>
    <NotAuthorized>
        <RedirectToLogin />
    </NotAuthorized>
</AuthorizeView>
'@

if (Test-Path $mainLayoutPath) {
    $rawContent = Get-Content -Path $mainLayoutPath -Raw
    if ($rawContent.Replace("`r`n","`n") -eq $mainLayoutContent.Replace("`r`n","`n")) {
        Write-Host "SKIP (ya aplicado): MainLayout.razor" -ForegroundColor Cyan
    } else {
        Set-Content -Path $mainLayoutPath -Value $mainLayoutContent -NoNewline
        Write-Host "OK: MainLayout.razor limpiado (favicon/titulo ya no van aqui)" -ForegroundColor Green
    }
} else {
    Write-Host "SKIP (archivo no encontrado): $mainLayoutPath" -ForegroundColor Yellow
}

# ── 8. Register.razor: quitar el HeadContent de favicon (redundante ahora) ──
$results2 = Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Register.razor" `
    -Description "Register.razor: quitar HeadContent de favicon (App.razor ya lo cubre)" -OldString @'
            @if (!string.IsNullOrWhiteSpace(Availability?.EditionName))
            {
                <PageTitle>@Availability.EditionName</PageTitle>
            }

            @if (!string.IsNullOrWhiteSpace(festivalResponse?.FaviconUrl))
            {
                <HeadContent>
                    <link rel="icon" href="@festivalResponse.FaviconUrl" />
                </HeadContent>
            }

            <!-- Festival header -->
'@ -NewString @'
            @if (!string.IsNullOrWhiteSpace(Availability?.EditionName))
            {
                <PageTitle>@Availability.EditionName</PageTitle>
            }

            <!-- Festival header -->
'@

if (-not $results2) {
    Write-Host "SKIP (anchor de Register.razor no encontrado - puede que ya este limpio)" -ForegroundColor Yellow
}

# ── 9. UserPanel.razor: quitar el HeadContent de favicon (redundante ahora) ──
$results3 = Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor" `
    -Description "UserPanel.razor: quitar HeadContent de favicon (App.razor ya lo cubre)" -OldString @'
@if (!string.IsNullOrWhiteSpace(Dashboard?.Registration?.EditionName))
{
    <PageTitle>@Dashboard.Registration.EditionName</PageTitle>
}

@if (!string.IsNullOrWhiteSpace(Dashboard?.Registration?.FaviconUrl))
{
    <HeadContent>
        <link rel="icon" href="@Dashboard.Registration.FaviconUrl" />
    </HeadContent>
}

<PageHeader Title="User Panel" pTitle="Dashboard"></PageHeader>
'@ -NewString @'
@if (!string.IsNullOrWhiteSpace(Dashboard?.Registration?.EditionName))
{
    <PageTitle>@Dashboard.Registration.EditionName</PageTitle>
}

<PageHeader Title="User Panel" pTitle="Dashboard"></PageHeader>
'@

if (-not $results3) {
    Write-Host "SKIP (anchor de UserPanel.razor no encontrado - puede que ya este limpio)" -ForegroundColor Yellow
}

Write-Host "`nHecho. dotnet build para confirmar." -ForegroundColor Green
Write-Host "IMPORTANTE: el titulo de pestana (PageTitle) de Register/UserPanel se sigue mostrando (informativo," -ForegroundColor Yellow
Write-Host "el nombre de la edicion concreta), pero ahora el FAVICON viene siempre de App.razor, resuelto por el" -ForegroundColor Yellow
Write-Host "dominio de la peticion - funciona igual antes y despues de iniciar sesion, en las 3 zonas de la app." -ForegroundColor Yellow