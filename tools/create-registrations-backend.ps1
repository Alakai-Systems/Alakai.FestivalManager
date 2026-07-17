# Fix-Step37-DynamicPageTitles.ps1
#
# Titulo de pestana dinamico en los 3 sitios:
#   1) Admin: usa ActiveFestivalState.Active.Name, se actualiza junto con el
#      favicon (mismo OnChange, mismo mecanismo del Paso 36).
#   2) Formulario publico (Register.razor): usa Availability.EditionName, que
#      ya estaba disponible - no hace falta ningun dato nuevo.
#   3) Panel de usuario (UserPanel.razor): necesitaba anadir EditionName al
#      registro que ya se carga (no existia en el DTO todavia).
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

# ── 1. Admin: actualizar document.title junto con el favicon ───────────────
$layoutPath = "Alakai.FestivalManager.Admin/Components/Layout/MainLayout.razor"

$results += Patch-File -Path $layoutPath -Description "MainLayout: anadir setPageTitle al script existente" -OldString @'
<script>
    window.setFavicon = window.setFavicon || function (url) {
        let link = document.querySelector("link[rel~='icon']");
        if (!link) {
            link = document.createElement('link');
            link.rel = 'icon';
            document.head.appendChild(link);
        }
        link.href = url;
    };
</script>
'@ -NewString @'
<script>
    window.setFavicon = window.setFavicon || function (url) {
        let link = document.querySelector("link[rel~='icon']");
        if (!link) {
            link = document.createElement('link');
            link.rel = 'icon';
            document.head.appendChild(link);
        }
        link.href = url;
    };

    window.setPageTitle = window.setPageTitle || function (title) {
        document.title = title;
    };
</script>
'@

$results += Patch-File -Path $layoutPath -Description "MainLayout: llamar a setPageTitle junto con el favicon" -OldString @'
    private async void UpdateFaviconAsync()
    {
        string? url = ActiveFestivalState.Active?.FaviconUrl;

        if (!string.IsNullOrWhiteSpace(url))
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("setFavicon", url);
            }
            catch
            {
                // Si falla (por ejemplo, durante el prerenderizado antes de tener JS disponible),
                // simplemente se mantiene el favicon actual - no es critico.
            }
        }
    }
'@ -NewString @'
    private async void UpdateFaviconAsync()
    {
        string? url = ActiveFestivalState.Active?.FaviconUrl;
        string? name = ActiveFestivalState.Active?.Name;

        try
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                await JsRuntime.InvokeVoidAsync("setFavicon", url);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                await JsRuntime.InvokeVoidAsync("setPageTitle", $"{name} - Alakai Festival Manager");
            }
        }
        catch
        {
            // Si falla (por ejemplo, durante el prerenderizado antes de tener JS disponible),
            // simplemente se mantiene el favicon/titulo actual - no es critico.
        }
    }
'@

# ── 2. Formulario publico: PageTitle con el nombre de la edicion ────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Register.razor" `
    -Description "Register.razor: anadir PageTitle dinamico" -OldString @'
            @if (!string.IsNullOrWhiteSpace(festivalResponse?.FaviconUrl))
            {
                <HeadContent>
                    <link rel="icon" href="@festivalResponse.FaviconUrl" />
                </HeadContent>
            }
'@ -NewString @'
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
'@

# ── 3. Panel de usuario: anadir EditionName al registro (Application + Admin + Service) ──
$results += Patch-File -Path "Alakai.FestivalManager.Application/Features/UserPanel/Contracts/DTOs/UserPanelRegistrationDto.cs" `
    -Description "Application DTO: anadir EditionName" -OldString @'
    public Guid Id { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
'@ -NewString @'
    public Guid Id { get; set; }
    public string? EditionName { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
'@

$results += Patch-File -Path "Alakai.FestivalManager.Admin/Contracts/UserPanel/DTOs/UserPanelRegistrationDto.cs" `
    -Description "Admin DTO: anadir EditionName" -OldString @'
    public Guid Id { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
'@ -NewString @'
    public Guid Id { get; set; }
    public string? EditionName { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
'@

$results += Patch-File -Path "Alakai.FestivalManager.Application/Features/UserPanel/Services/UserPanelService.cs" `
    -Description "Service: rellenar EditionName al construir el DTO" -OldString @'
            Registration = registration is null ? null : new UserPanelRegistrationDto
            {
                Id = registration.Id,
                RegistrationStatus = registration.Status.ToString(),
'@ -NewString @'
            Registration = registration is null ? null : new UserPanelRegistrationDto
            {
                Id = registration.Id,
                EditionName = registration.Edition?.Name,
                RegistrationStatus = registration.Status.ToString(),
'@

$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor" `
    -Description "UserPanel.razor: anadir PageTitle dinamico" -OldString @'
<PageHeader Title="User Panel" pTitle="Dashboard"></PageHeader>
'@ -NewString @'
@if (!string.IsNullOrWhiteSpace(Dashboard?.Registration?.EditionName))
{
    <PageTitle>@Dashboard.Registration.EditionName</PageTitle>
}

<PageHeader Title="User Panel" pTitle="Dashboard"></PageHeader>
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nTitulos dinamicos anadidos en los 3 sitios. dotnet build para confirmar." -ForegroundColor Green