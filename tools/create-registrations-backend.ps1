# Fix-Step38-FaviconFixes.ps1
#
# 1) User Panel usa @layout UserPanelLayout, NO MainLayout - por eso el
#    mecanismo de favicon/titulo de los Pasos 36/37 (que vive en MainLayout)
#    nunca se ejecutaba ahi. Ademas ActiveFestivalState es un concepto de
#    staff, no aplica a un participante. Se anade FaviconUrl al registro del
#    dashboard y se usa HeadContent, igual que en Register.razor.
#
# 2) Admin: cambiar solo el href de un <link> de favicon existente no siempre
#    fuerza a algunos navegadores a recargarlo. Se hace mas fiable quitando
#    el link viejo y creando uno nuevo cada vez.
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

# ── 1. Admin: hacer setFavicon mas fiable (quitar+recrear el link) ─────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Layout/MainLayout.razor" `
    -Description "setFavicon: quitar y recrear el link en vez de solo cambiar el href" -OldString @'
    window.setFavicon = window.setFavicon || function (url) {
        let link = document.querySelector("link[rel~='icon']");
        if (!link) {
            link = document.createElement('link');
            link.rel = 'icon';
            document.head.appendChild(link);
        }
        link.href = url;
    };
'@ -NewString @'
    window.setFavicon = window.setFavicon || function (url) {
        document.querySelectorAll("link[rel~='icon']").forEach(el => el.remove());
        const link = document.createElement('link');
        link.rel = 'icon';
        link.href = url;
        document.head.appendChild(link);
    };
'@

# ── 2. Application/Admin: anadir FaviconUrl al registro del dashboard ──────
$results += Patch-File -Path "Alakai.FestivalManager.Application/Features/UserPanel/Contracts/DTOs/UserPanelRegistrationDto.cs" `
    -Description "Application DTO: anadir FaviconUrl" -OldString @'
    public string? EditionName { get; set; }
'@ -NewString @'
    public string? EditionName { get; set; }
    public string? FaviconUrl { get; set; }
'@

$results += Patch-File -Path "Alakai.FestivalManager.Admin/Contracts/UserPanel/DTOs/UserPanelRegistrationDto.cs" `
    -Description "Admin DTO: anadir FaviconUrl" -OldString @'
    public string? EditionName { get; set; }
'@ -NewString @'
    public string? EditionName { get; set; }
    public string? FaviconUrl { get; set; }
'@

$results += Patch-File -Path "Alakai.FestivalManager.Application/Features/UserPanel/Services/UserPanelService.cs" `
    -Description "Service: rellenar FaviconUrl (via Edition.Festival)" -OldString @'
                EditionName = registration.Edition?.Name,
'@ -NewString @'
                EditionName = registration.Edition?.Name,
                FaviconUrl = registration.Edition?.Festival?.FaviconUrl,
'@

# ── 3. UserPanel.razor: HeadContent con el favicon real ────────────────────
$results += Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor" `
    -Description "UserPanel.razor: anadir HeadContent con el favicon" -OldString @'
@if (!string.IsNullOrWhiteSpace(Dashboard?.Registration?.EditionName))
{
    <PageTitle>@Dashboard.Registration.EditionName</PageTitle>
}
'@ -NewString @'
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
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host "IMPORTANTE: verifica que la URL del favicon de Swim Out cargue de verdad pegandola en el navegador -" -ForegroundColor Yellow
Write-Host "si es una URL antigua/rota (de antes de hoy), el remove+recrear el link no la arreglara por si solo." -ForegroundColor Yellow