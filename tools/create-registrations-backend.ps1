# Fix-Step69-PlatformDefaultBranding.ps1
#
# Anade una marca "por defecto" de Alakai Systems, usada cuando el dominio de
# la peticion NO coincide con el CustomDomain de ningun festival (por ejemplo,
# vuestro propio dominio neutro app.alakai-systems.com, o la URL generica de
# Azure). Como App.razor y las 2 paginas de Login ya llaman todas al MISMO
# endpoint (by-domain), basta con que ESE endpoint tenga un fallback sensato
# en vez de devolver "no encontrado" - no hace falta tocar nada mas en ningun
# otro sitio.
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

# ── 1. appsettings.json: seccion PlatformBranding (rellena las URLs cuando las tengas) ──
$results += Patch-File -Path "Alakai.FestivalManager.Api/appsettings.json" `
    -Description "Anadir la seccion PlatformBranding" -OldString @'
  "GoogleAnalytics": {
'@ -NewString @'
  "PlatformBranding": {
    "Name": "Alakai Systems",
    "LogoUrl": "",
    "FaviconUrl": ""
  },
  "GoogleAnalytics": {
'@

if ($results -contains $false) {
    Write-Host "`nNo se pudo aplicar el patch de appsettings.json. Revisa el mensaje anterior." -ForegroundColor Red
    exit 1
}

# ── 2. PublicFestivalsController: usar la config como fallback ─────────────
$results2 = @()

$results2 += Patch-File -Path "Alakai.FestivalManager.Api/Controllers/PublicFestivalsController.cs" `
    -Description "Inyectar IConfiguration" -OldString @'
    private readonly IFestivalRepository _festivalRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IEmailLayoutRepository _emailLayoutRepository;

    public PublicFestivalsController(IFestivalRepository festivalRepository, IEditionRepository editionRepository,
        IEmailLayoutRepository emailLayoutRepository)
    {
        _festivalRepository = festivalRepository;
        _editionRepository = editionRepository;
        _emailLayoutRepository = emailLayoutRepository;
    }
'@ -NewString @'
    private readonly IFestivalRepository _festivalRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IEmailLayoutRepository _emailLayoutRepository;
    private readonly IConfiguration _configuration;

    public PublicFestivalsController(IFestivalRepository festivalRepository, IEditionRepository editionRepository,
        IEmailLayoutRepository emailLayoutRepository, IConfiguration configuration)
    {
        _festivalRepository = festivalRepository;
        _editionRepository = editionRepository;
        _emailLayoutRepository = emailLayoutRepository;
        _configuration = configuration;
    }
'@

$results2 += Patch-File -Path "Alakai.FestivalManager.Api/Controllers/PublicFestivalsController.cs" `
    -Description "GetByDomain: usar PlatformBranding como fallback en vez de NotFound" -OldString @'
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
            FaviconUrl = festival.FaviconUrl,
            LogoUrl = festival.LogoUrl
        });
    }
'@ -NewString @'
    [HttpGet("by-domain/{domain}")]
    public async Task<IActionResult> GetByDomain(string domain, CancellationToken cancellationToken)
    {
        Festival? festival = await _festivalRepository.GetByCustomDomainAsync(domain, cancellationToken);

        if (festival is not null)
        {
            return Ok(new
            {
                Name = festival.Name,
                FaviconUrl = festival.FaviconUrl,
                LogoUrl = festival.LogoUrl
            });
        }

        string platformName = _configuration["PlatformBranding:Name"] ?? "Alakai Festival Manager";
        string? platformLogoUrl = _configuration["PlatformBranding:LogoUrl"];
        string? platformFaviconUrl = _configuration["PlatformBranding:FaviconUrl"];

        return Ok(new
        {
            Name = platformName,
            FaviconUrl = string.IsNullOrWhiteSpace(platformFaviconUrl) ? null : platformFaviconUrl,
            LogoUrl = string.IsNullOrWhiteSpace(platformLogoUrl) ? null : platformLogoUrl
        });
    }
'@

if ($results2 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (controlador). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nHecho. dotnet build para confirmar." -ForegroundColor Green
Write-Host ""
Write-Host "Para que aparezca de verdad el logo/favicon de Alakai Systems, rellena" -ForegroundColor Yellow
Write-Host "LogoUrl y FaviconUrl en la seccion PlatformBranding de appsettings.json" -ForegroundColor Yellow
Write-Host "(o en la variable de entorno equivalente en Azure) con las URLs reales." -ForegroundColor Yellow
Write-Host "Mientras esten vacias, se usara el favicon/logo por defecto del sistema." -ForegroundColor Cyan