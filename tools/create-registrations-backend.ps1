# Fix-Step56-FormHeaderFooter.ps1
#
# Reutiliza el Header/Footer de Email (Comunicacion -> Email Layout Settings)
# tambien en el formulario publico de inscripcion. Mismo contenido en los dos
# sitios, sin duplicar nada - tal como se decidio.
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

# ── 1. Api: inyectar IEmailLayoutRepository + nuevo endpoint publico ────────
$results += Patch-File -Path "Alakai.FestivalManager.Api/Controllers/PublicFestivalsController.cs" `
    -Description "Inyectar IEmailLayoutRepository" -OldString @'
    private readonly IFestivalRepository _festivalRepository;
    private readonly IEditionRepository _editionRepository;

    public PublicFestivalsController(IFestivalRepository festivalRepository, IEditionRepository editionRepository)
    {
        _festivalRepository = festivalRepository;
        _editionRepository = editionRepository;
    }
'@ -NewString @'
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

    [HttpGet("email-layout/{editionId:guid}")]
    public async Task<IActionResult> GetEmailLayout(Guid editionId, CancellationToken cancellationToken)
    {
        EmailLayout? layout = await _emailLayoutRepository.GetForEditionAsync(editionId, cancellationToken);

        return Ok(new
        {
            headerHtml = layout?.HeaderHtml ?? string.Empty,
            footerHtml = layout?.FooterHtml ?? string.Empty
        });
    }
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (Api). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 2. Admin: cliente + DTO ──────────────────────────────────────────────────
$results2 = Patch-File -Path "Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs" `
    -Description "Cliente: anadir GetEmailLayoutAsync" -OldString @'
    public async Task<PublicFestivalBrandingDto?> GetFestivalByDomainAsync(string domain, CancellationToken cancellationToken = default)
'@ -NewString @'
    public async Task<PublicEmailLayoutDto?> GetEmailLayoutAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PublicEmailLayoutDto>($"api/public/festivals/email-layout/{editionId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PublicFestivalBrandingDto?> GetFestivalByDomainAsync(string domain, CancellationToken cancellationToken = default)
'@

if (-not $results2) {
    Write-Host "`nNo se pudo aplicar el patch del cliente (metodo)." -ForegroundColor Red
    exit 1
}

$results3 = Patch-File -Path "Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs" `
    -Description "Anadir el record PublicEmailLayoutDto" -OldString @'
public record PublicFestivalBrandingDto(string Name, string? FaviconUrl);
'@ -NewString @'
public record PublicFestivalBrandingDto(string Name, string? FaviconUrl);

public record PublicEmailLayoutDto(string HeaderHtml, string FooterHtml);
'@

if (-not $results3) {
    Write-Host "`nNo se pudo aplicar el patch del cliente (DTO)." -ForegroundColor Red
    exit 1
}

# ── 3. Register.razor: cargar y mostrar Header/Footer ───────────────────────
$results4 = Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Register.razor" `
    -Description "Cargar el layout tras conocer la edicion" -OldString @'
            Form.EditionId = festivalResponse.ActiveEditionId.Value;
            HasAccommodation = festivalResponse.HasAccommodation;
'@ -NewString @'
            Form.EditionId = festivalResponse.ActiveEditionId.Value;
            HasAccommodation = festivalResponse.HasAccommodation;

            try
            {
                PublicEmailLayoutDto? layout = await PublicApi.GetEmailLayoutAsync(festivalResponse.ActiveEditionId.Value);
                HeaderHtml = layout?.HeaderHtml;
                FooterHtml = layout?.FooterHtml;
            }
            catch
            {
                // Sin header/footer configurado para esta edicion - el formulario sigue funcionando sin ellos.
            }
'@

if (-not $results4) {
    Write-Host "`nNo se pudo aplicar el patch de carga del layout en Register.razor." -ForegroundColor Red
    exit 1
}

$results5 = Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Register.razor" `
    -Description "Anadir los campos HeaderHtml/FooterHtml" -OldString @'
    private PublicFestivalSlugDto? festivalResponse;
'@ -NewString @'
    private PublicFestivalSlugDto? festivalResponse;
    private string? HeaderHtml;
    private string? FooterHtml;
'@

if (-not $results5) {
    Write-Host "`nNo se pudo aplicar el patch de los campos en Register.razor." -ForegroundColor Red
    exit 1
}

# ── 4. Mostrar el Header antes del formulario ───────────────────────────────
$results6 = Patch-File -Path "Alakai.FestivalManager.Admin/Components/Pages/Register.razor" `
    -Description "Mostrar el Header antes del formulario" -OldString @'
            <!-- Festival header -->
'@ -NewString @'
            @if (!string.IsNullOrWhiteSpace(HeaderHtml))
            {
                <div class="mb-6 overflow-hidden rounded-lg">
                    @((MarkupString)HeaderHtml)
                </div>
            }

            <!-- Festival header -->
'@

if (-not $results6) {
    Write-Host "`nNo se pudo aplicar el patch de mostrar el Header." -ForegroundColor Red
    exit 1
}

Write-Host "`nBackend + carga del Header completos." -ForegroundColor Green
Write-Host "IMPORTANTE: falta anadir el Footer al final del formulario - pegame el final del archivo" -ForegroundColor Yellow
Write-Host "Register.razor (donde termina el @if principal, antes del cierre) para colocarlo con precision." -ForegroundColor Yellow
Write-Host "dotnet build para confirmar esta parte mientras tanto." -ForegroundColor Cyan