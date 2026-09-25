<#
    Fix-DynamicCurrency.ps1
    -----------------------------------------------

    Quita todos los simbolos "€" hardcodeados de la app y los sustituye por
    la divisa configurada en Payment Settings de cada festival (EUR/USD/CAD).

    Cambios en backend / infraestructura:
      - FestivalRepository: GetAllAsync/GetBySlugAsync ahora incluyen
        Credentials (antes festival.Credentials siempre era null en esos
        sitios, asi que Currency nunca se podia leer).
      - UserPanelRepository: GetLatestRegistrationByUserIdAsync tambien
        incluye Credentials.
      - FestivalDto (Application y Admin) y UserPanelRegistrationDto
        (Application y Admin): nuevo campo Currency (string, "EUR" por
        defecto). Solo se expone la divisa, nunca ninguna clave secreta de
        FestivalCredentials.
      - FestivalMappingProfile: mapeo explicito de Currency.
      - UserPanelService y PublicFestivalsController: calculan la Currency
        del festival y la incluyen en la respuesta.
      - PublicRegistrationApiClient: PublicFestivalSlugDto gana un campo
        Currency.
      - Nuevo Alakai.FestivalManager.Admin/Contracts/Common/CurrencyHelper.cs
        con CurrencyHelper.Symbol(currencyCode) -> "€" / "$" / "CA$".

    Cambios en las paginas (Register.razor, UserPanel.razor,
    Registrations.razor, DiscountCodes.razor, Buses.razor): cada "€" fijo se
    sustituye por un CurrencySymbol calculado a partir del festival
    correspondiente (el festival de la inscripcion, o el festival
    seleccionado en el filtro de arriba en las paginas de Admin). Se
    respeta exactamente la posicion/espaciado que ya tenia cada "€".

    Ademas: en el modal "Payment Settings", el campo Currency estaba metido
    dentro de la card de Redsys, dando a entender que era una configuracion
    solo de Redsys -- pero es una propiedad de FestivalCredentials que
    afecta por igual a Redsys y a Stripe. Se saca a su propia fila, a todo
    el ancho, por encima de las 3 columnas, con la etiqueta "Currency
    (applies to Redsys & Stripe)".

    Limitacion conocida (asumida a proposito): en las paginas de listado del
    Admin (Registrations, DiscountCodes, Buses) la divisa que se usa es la
    del festival actualmente seleccionado en el filtro de arriba, no una
    divisa por fila/registro individual (los DTOs de esas filas no llevan
    su propia Currency). Es razonable porque esas paginas normalmente se
    miran festival a festival, pero si algun dia se listan varios festivales
    a la vez con distintas divisas en la misma tabla, esto habria que
    revisarlo.

    Verificado contra los archivos reales del repo, con las 15 correcciones
    anteriores ya aplicadas. Anchors unicos, balance de llaves/parentesis
    limpio, diff revisado.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-DynamicCurrency.ps1

    Idempotente y todo-o-nada: si algun anchor no encaja porque algun archivo
    local difiere de lo esperado, no escribe nada y lista el problema.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath 'Alakai.FestivalManager.sln')) {
    Write-Error "No se encuentra Alakai.FestivalManager.sln en el directorio actual. Ejecuta este script desde la raiz del repo (Alakai.FestivalManager/)."
    exit 1
}

# ----------------------------------------------------------------------------
# Helpers (identicos a los scripts anteriores)
# ----------------------------------------------------------------------------

function Get-NormalizedContent {
    param([string]$Path)

    $raw = [System.IO.File]::ReadAllText($Path)
    $usesCrlf = $raw.Contains("`r`n")
    $normalized = $raw.Replace("`r`n", "`n")

    return [PSCustomObject]@{
        Raw        = $raw
        Normalized = $normalized
        UsesCrlf   = $usesCrlf
    }
}

function Set-NormalizedContent {
    param(
        [string]$Path,
        [string]$NormalizedContent,
        [bool]$UsesCrlf
    )

    $final = if ($UsesCrlf) { $NormalizedContent.Replace("`n", "`r`n") } else { $NormalizedContent }
    [System.IO.File]::WriteAllText($Path, $final, [System.Text.UTF8Encoding]::new($false))
}

function Convert-ToLf {
    param([string]$Text)
    return $Text.Replace("`r`n", "`n")
}

$script:PlanErrors = @()
$script:Plan = @()

function Add-PatchOperation {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Anchor,
        [Parameter(Mandatory)][string]$Replacement,
        [Parameter(Mandatory)][string]$Description
    )

    $script:Plan += [PSCustomObject]@{
        Type        = 'Patch'
        Path        = $Path
        Anchor      = Convert-ToLf $Anchor
        Replacement = Convert-ToLf $Replacement
        Description = $Description
    }
}

function Add-CreateOperation {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Content,
        [Parameter(Mandatory)][string]$Description
    )

    $script:Plan += [PSCustomObject]@{
        Type        = 'Create'
        Path        = $Path
        Content     = Convert-ToLf $Content
        Description = $Description
    }
}

function Test-PatchOperation {
    param($Op)

    if (-not (Test-Path -LiteralPath $Op.Path)) {
        return "No existe el archivo: $($Op.Path)"
    }

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 1) {
        return $null
    }

    if ($anchorCount -eq 0) {
        $replacementCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Replacement))).Count
        if ($replacementCount -ge 1) {
            return $null  # ya aplicado -> idempotente
        }
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado -- revisalo a mano). Descripcion: $($Op.Description)"
    }

    return "Anchor encontrado $anchorCount veces en $($Op.Path) (deberia ser unico). Descripcion: $($Op.Description)"
}

function Test-CreateOperation {
    param($Op)

    if (-not (Test-Path -LiteralPath $Op.Path)) {
        return $null
    }

    $existing = Get-NormalizedContent -Path $Op.Path

    if ($existing.Normalized.TrimEnd() -eq $Op.Content.TrimEnd()) {
        return $null  # ya existe con el contenido esperado -> idempotente
    }

    return "Ya existe $($Op.Path) con un contenido distinto al esperado; revisalo a mano antes de reintentar."
}

function Invoke-PatchOperation {
    param($Op)

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 0) {
        Write-Host "  = ya aplicado: $($Op.Path) -- $($Op.Description)" -ForegroundColor DarkGray
        return
    }

    $newNormalized = $file.Normalized.Replace($Op.Anchor, $Op.Replacement)
    Set-NormalizedContent -Path $Op.Path -NormalizedContent $newNormalized -UsesCrlf $file.UsesCrlf

    Write-Host "  + patched: $($Op.Path) -- $($Op.Description)" -ForegroundColor Green
}

function Invoke-CreateOperation {
    param($Op)

    if (Test-Path -LiteralPath $Op.Path) {
        Write-Host "  = ya existe: $($Op.Path) -- $($Op.Description)" -ForegroundColor DarkGray
        return
    }

    $dir = Split-Path -Parent $Op.Path
    if ($dir -and -not (Test-Path -LiteralPath $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }

    Set-NormalizedContent -Path $Op.Path -NormalizedContent $Op.Content -UsesCrlf $false

    Write-Host "  + created: $($Op.Path) -- $($Op.Description)" -ForegroundColor Green
}

# 1. Alakai.FestivalManager.Infrastructure/Repositories/FestivalRepository.cs -- FestivalRepository.cs: GetAllAsync incluye Credentials (hace falta para exponer Currency en FestivalDto)
Add-PatchOperation -Path 'Alakai.FestivalManager.Infrastructure/Repositories/FestivalRepository.cs' `
    -Description 'FestivalRepository.cs: GetAllAsync incluye Credentials (hace falta para exponer Currency en FestivalDto)' `
    -Anchor @'
    public async Task<IReadOnlyList<Festival>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }
'@ `
    -Replacement @'
    public async Task<IReadOnlyList<Festival>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .Include(f => f.Credentials)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }
'@

# 2. Alakai.FestivalManager.Infrastructure/Repositories/FestivalRepository.cs -- FestivalRepository.cs: GetBySlugAsync incluye Credentials (hace falta para exponer Currency al formulario publico)
Add-PatchOperation -Path 'Alakai.FestivalManager.Infrastructure/Repositories/FestivalRepository.cs' `
    -Description 'FestivalRepository.cs: GetBySlugAsync incluye Credentials (hace falta para exponer Currency al formulario publico)' `
    -Anchor @'
    public async Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Slug == slug, cancellationToken);
    }
'@ `
    -Replacement @'
    public async Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .Include(f => f.Credentials)
            .FirstOrDefaultAsync(f => f.Slug == slug, cancellationToken);
    }
'@

# 3. Alakai.FestivalManager.Infrastructure/Repositories/UserPanelRepository.cs -- UserPanelRepository.cs: GetLatestRegistrationByUserIdAsync incluye Festival.Credentials (hace falta para Currency en el panel del usuario)
Add-PatchOperation -Path 'Alakai.FestivalManager.Infrastructure/Repositories/UserPanelRepository.cs' `
    -Description 'UserPanelRepository.cs: GetLatestRegistrationByUserIdAsync incluye Festival.Credentials (hace falta para Currency en el panel del usuario)' `
    -Anchor @'
        IQueryable<Registration> baseQuery = _context.Registrations
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.Edition).ThenInclude(e => e.Festival)
            .Where(r => r.UserId == userId && r.IsActive);
'@ `
    -Replacement @'
        IQueryable<Registration> baseQuery = _context.Registrations
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.Edition).ThenInclude(e => e.Festival).ThenInclude(f => f.Credentials)
            .Where(r => r.UserId == userId && r.IsActive);
'@

# 4. Alakai.FestivalManager.Application/Features/Festivals/Contracts/DTOs/FestivalDto.cs -- FestivalDto.cs (Application): anade Currency, sin exponer ningun otro campo de FestivalCredentials
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Festivals/Contracts/DTOs/FestivalDto.cs' `
    -Description 'FestivalDto.cs (Application): anade Currency, sin exponer ningun otro campo de FestivalCredentials' `
    -Anchor @'
    public bool IsActive { get; set; }
    public FestivalModule EnabledModules { get; set; }
    public EnabledPaymentPlan EnabledPaymentPlans { get; set; }
    public EnabledPaymentPlatform EnabledPaymentPlatforms { get; set; }
}
'@ `
    -Replacement @'
    public bool IsActive { get; set; }
    public FestivalModule EnabledModules { get; set; }
    public EnabledPaymentPlan EnabledPaymentPlans { get; set; }
    public EnabledPaymentPlatform EnabledPaymentPlatforms { get; set; }

    /// <summary>Solo la divisa (EUR/USD/CAD) -- nunca las claves de FestivalCredentials.</summary>
    public string Currency { get; set; } = "EUR";
}
'@

# 5. Alakai.FestivalManager.Application/Features/Festivals/Mappings/FestivalMappingProfile.cs -- FestivalMappingProfile.cs: Currency se saca de Credentials.Currency (o EUR si no hay credenciales todavia)
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Festivals/Mappings/FestivalMappingProfile.cs' `
    -Description 'FestivalMappingProfile.cs: Currency se saca de Credentials.Currency (o EUR si no hay credenciales todavia)' `
    -Anchor @'
        //Generics y Gets
        CreateMap<Festival, FestivalDto>();
'@ `
    -Replacement @'
        //Generics y Gets
        CreateMap<Festival, FestivalDto>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src =>
                src.Credentials != null && !string.IsNullOrWhiteSpace(src.Credentials.Currency) ? src.Credentials.Currency : "EUR"));
'@

# 6. Alakai.FestivalManager.Application/Features/UserPanel/Contracts/DTOs/UserPanelRegistrationDto.cs -- UserPanelRegistrationDto.cs (Application): anade Currency
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/UserPanel/Contracts/DTOs/UserPanelRegistrationDto.cs' `
    -Description 'UserPanelRegistrationDto.cs (Application): anade Currency' `
    -Anchor @'
    public string Language { get; set; } = "en";
    public bool AllowRedsysPayment { get; set; } = true;
    public bool AllowStripePayment { get; set; }
}
'@ `
    -Replacement @'
    public string Language { get; set; } = "en";
    public bool AllowRedsysPayment { get; set; } = true;
    public bool AllowStripePayment { get; set; }
    public string Currency { get; set; } = "EUR";
}
'@

# 7. Alakai.FestivalManager.Application/Features/UserPanel/Services/UserPanelService.cs -- UserPanelService.cs: calcula la divisa del festival (o EUR si no hay credenciales configuradas)
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/UserPanel/Services/UserPanelService.cs' `
    -Description 'UserPanelService.cs: calcula la divisa del festival (o EUR si no hay credenciales configuradas)' `
    -Anchor @'
        EnabledPaymentPlatform enabledPaymentPlatforms = registration.Edition?.Festival?.EnabledPaymentPlatforms ?? EnabledPaymentPlatform.Redsys;
        bool allowRedsysPayment = (enabledPaymentPlatforms & EnabledPaymentPlatform.Redsys) != 0;
        bool allowStripePayment = (enabledPaymentPlatforms & EnabledPaymentPlatform.Stripe) != 0;
'@ `
    -Replacement @'
        EnabledPaymentPlatform enabledPaymentPlatforms = registration.Edition?.Festival?.EnabledPaymentPlatforms ?? EnabledPaymentPlatform.Redsys;
        bool allowRedsysPayment = (enabledPaymentPlatforms & EnabledPaymentPlatform.Redsys) != 0;
        bool allowStripePayment = (enabledPaymentPlatforms & EnabledPaymentPlatform.Stripe) != 0;
        string currency = registration.Edition?.Festival?.Credentials?.Currency;
        currency = string.IsNullOrWhiteSpace(currency) ? "EUR" : currency;
'@

# 8. Alakai.FestivalManager.Application/Features/UserPanel/Services/UserPanelService.cs -- UserPanelService.cs: pasa la divisa calculada al DTO del panel del usuario
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/UserPanel/Services/UserPanelService.cs' `
    -Description 'UserPanelService.cs: pasa la divisa calculada al DTO del panel del usuario' `
    -Anchor @'
                PaymentDueAt = registration.PaymentDueAt,
                AllowRedsysPayment = allowRedsysPayment,
                AllowStripePayment = allowStripePayment
            },
'@ `
    -Replacement @'
                PaymentDueAt = registration.PaymentDueAt,
                AllowRedsysPayment = allowRedsysPayment,
                AllowStripePayment = allowStripePayment,
                Currency = currency
            },
'@

# 9. Alakai.FestivalManager.Api/Controllers/PublicFestivalsController.cs -- PublicFestivalsController.cs: GetBySlug incluye la divisa del festival (GetBySlugAsync ya trae Credentials incluido)
Add-PatchOperation -Path 'Alakai.FestivalManager.Api/Controllers/PublicFestivalsController.cs' `
    -Description 'PublicFestivalsController.cs: GetBySlug incluye la divisa del festival (GetBySlugAsync ya trae Credentials incluido)' `
    -Anchor @'
        return Ok(new
        {
            ActiveEditionId = active?.Id,
            HasAccommodation = hasAccommodation,
            TermsUrl = festival.TermsUrl,
            FaviconUrl = festival.FaviconUrl,
            AllowFullOnline = allowFullOnline,
            AllowSplitFiftyFifty = allowSplitFiftyFifty,
            AllowDeferredTenDays = allowDeferredTenDays,
            AllowRedsysPayment = allowRedsysPayment,
            AllowStripePayment = allowStripePayment
        });
'@ `
    -Replacement @'
        string currency = festival.Credentials?.Currency;
        currency = string.IsNullOrWhiteSpace(currency) ? "EUR" : currency;

        return Ok(new
        {
            ActiveEditionId = active?.Id,
            HasAccommodation = hasAccommodation,
            TermsUrl = festival.TermsUrl,
            FaviconUrl = festival.FaviconUrl,
            AllowFullOnline = allowFullOnline,
            AllowSplitFiftyFifty = allowSplitFiftyFifty,
            AllowDeferredTenDays = allowDeferredTenDays,
            AllowRedsysPayment = allowRedsysPayment,
            AllowStripePayment = allowStripePayment,
            Currency = currency
        });
'@

# 10. Alakai.FestivalManager.Admin/Contracts/Festivals/DTOs/FestivalDto.cs -- FestivalDto.cs (Admin): anade Currency
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Contracts/Festivals/DTOs/FestivalDto.cs' `
    -Description 'FestivalDto.cs (Admin): anade Currency' `
    -Anchor @'
    public int EnabledModules { get; set; }
    public int EnabledPaymentPlans { get; set; }
    public int EnabledPaymentPlatforms { get; set; }
}
'@ `
    -Replacement @'
    public int EnabledModules { get; set; }
    public int EnabledPaymentPlans { get; set; }
    public int EnabledPaymentPlatforms { get; set; }
    public string Currency { get; set; } = "EUR";
}
'@

# 11. Alakai.FestivalManager.Admin/Contracts/UserPanel/DTOs/UserPanelRegistrationDto.cs -- UserPanelRegistrationDto.cs (Admin): anade Currency
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Contracts/UserPanel/DTOs/UserPanelRegistrationDto.cs' `
    -Description 'UserPanelRegistrationDto.cs (Admin): anade Currency' `
    -Anchor @'
    public string Language { get; set; } = "en";
    public bool AllowRedsysPayment { get; set; } = true;
    public bool AllowStripePayment { get; set; }
}
'@ `
    -Replacement @'
    public string Language { get; set; } = "en";
    public bool AllowRedsysPayment { get; set; } = true;
    public bool AllowStripePayment { get; set; }
    public string Currency { get; set; } = "EUR";
}
'@

# 12. Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs -- PublicRegistrationApiClient.cs: PublicFestivalSlugDto incluye Currency
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Services/Api/PublicRegistrationApiClient.cs' `
    -Description 'PublicRegistrationApiClient.cs: PublicFestivalSlugDto incluye Currency' `
    -Anchor @'
public record PublicFestivalSlugDto(Guid? ActiveEditionId, bool HasAccommodation, string? TermsUrl, string? FaviconUrl, bool AllowFullOnline = true, bool AllowSplitFiftyFifty = true, bool AllowDeferredTenDays = true, bool AllowRedsysPayment = true, bool AllowStripePayment = false);
'@ `
    -Replacement @'
public record PublicFestivalSlugDto(Guid? ActiveEditionId, bool HasAccommodation, string? TermsUrl, string? FaviconUrl, bool AllowFullOnline = true, bool AllowSplitFiftyFifty = true, bool AllowDeferredTenDays = true, bool AllowRedsysPayment = true, bool AllowStripePayment = false, string Currency = "EUR");
'@

# 13. Alakai.FestivalManager.Admin/Contracts/Common/CurrencyHelper.cs -- CurrencyHelper.cs (nuevo): simbolo de divisa segun el codigo (EUR/USD/CAD), reutilizable en todo el Admin
Add-CreateOperation -Path 'Alakai.FestivalManager.Admin/Contracts/Common/CurrencyHelper.cs' `
    -Description 'CurrencyHelper.cs (nuevo): simbolo de divisa segun el codigo (EUR/USD/CAD), reutilizable en todo el Admin' `
    -Content @'
namespace Alakai.FestivalManager.Admin.Contracts.Common;

/// <summary>
/// Simbolo a mostrar para cada divisa soportada (ver el selector de Currency
/// en Payment Settings: EUR/USD/CAD). Se usa en vez de "€" fijo en cualquier
/// importe mostrado al usuario, para que festivales en otra divisa no
/// muestren el simbolo equivocado.
/// </summary>
public static class CurrencyHelper
{
    public static string Symbol(string? currencyCode) => currencyCode?.Trim().ToUpperInvariant() switch
    {
        "USD" => "$",
        "CAD" => "CA$",
        _ => "€"
    };
}
'@

# 14. Alakai.FestivalManager.Admin/Components/Pages/Register.razor -- Register.razor: anade CurrencySymbol (segun la divisa del festival) en vez de € fijo
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Register.razor' `
    -Description 'Register.razor: anade CurrencySymbol (segun la divisa del festival) en vez de € fijo' `
    -Anchor @'
    private decimal ManagementFee => SelectedPaymentPlatform == "Stripe"
        ? Math.Round(AmountNow * 0.015m + 0.25m, 2, MidpointRounding.AwayFromZero)
        : Math.Round(AmountNow * 0.01m, 2, MidpointRounding.AwayFromZero);
    private decimal AmountNow => SelectedPlan switch
'@ `
    -Replacement @'
    private decimal ManagementFee => SelectedPaymentPlatform == "Stripe"
        ? Math.Round(AmountNow * 0.015m + 0.25m, 2, MidpointRounding.AwayFromZero)
        : Math.Round(AmountNow * 0.01m, 2, MidpointRounding.AwayFromZero);

    // "€" fijo sustituido por esto en todo el formulario -- refleja la divisa
    // configurada para este festival en Payment Settings (EUR/USD/CAD).
    private string CurrencySymbol => CurrencyHelper.Symbol(festivalResponse?.Currency);
    private decimal AmountNow => SelectedPlan switch
'@

# 15. Alakai.FestivalManager.Admin/Components/Pages/Register.razor -- Register.razor: precio del nivel usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Register.razor' `
    -Description 'Register.razor: precio del nivel usa la divisa del festival' `
    -Anchor @'
                                            <span class="text-muted text-xs">@lvl.Price.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)€</span>
'@ `
    -Replacement @'
                                            <span class="text-muted text-xs">@lvl.Price.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)@CurrencySymbol</span>
'@

# 16. Alakai.FestivalManager.Admin/Components/Pages/Register.razor -- Register.razor: importe de descuento usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Register.razor' `
    -Description 'Register.razor: importe de descuento usa la divisa del festival' `
    -Anchor @'
                                                ✓ @T.Get("reg_discount_applied") (-@DiscountInfo.DiscountAmount.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)€)
'@ `
    -Replacement @'
                                                ✓ @T.Get("reg_discount_applied") (-@DiscountInfo.DiscountAmount.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)@CurrencySymbol)
'@

# 17. Alakai.FestivalManager.Admin/Components/Pages/Register.razor -- Register.razor: precio base usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Register.razor' `
    -Description 'Register.razor: precio base usa la divisa del festival' `
    -Anchor @'
                                    <div class="text-sm text-muted">@T.Get("price") <span class="font-semibold text-black">@BasePrice.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)€</span></div>
'@ `
    -Replacement @'
                                    <div class="text-sm text-muted">@T.Get("price") <span class="font-semibold text-black">@BasePrice.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)@CurrencySymbol</span></div>
'@

# 18. Alakai.FestivalManager.Admin/Components/Pages/Register.razor -- Register.razor: importe a pagar ahora usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Register.razor' `
    -Description 'Register.razor: importe a pagar ahora usa la divisa del festival' `
    -Anchor @'
                                        <div class="text-sm text-muted mt-1">@T.Get("pay_now") <span class="font-semibold text-black">@AmountNow.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)€</span></div>
'@ `
    -Replacement @'
                                        <div class="text-sm text-muted mt-1">@T.Get("pay_now") <span class="font-semibold text-black">@AmountNow.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)@CurrencySymbol</span></div>
'@

# 19. Alakai.FestivalManager.Admin/Components/Pages/Register.razor -- Register.razor: comision de gestion usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Register.razor' `
    -Description 'Register.razor: comision de gestion usa la divisa del festival' `
    -Anchor @'
                                    <div class="text-xs text-muted mt-1">+ @ManagementFee.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)€ @T.Get("reg_management_fee") (@(SelectedPlan == "SplitFiftyFifty" ? T.Get("reg_of_50") : T.Get("reg_of_total"))).</div>
'@ `
    -Replacement @'
                                    <div class="text-xs text-muted mt-1">+ @ManagementFee.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)@CurrencySymbol @T.Get("reg_management_fee") (@(SelectedPlan == "SplitFiftyFifty" ? T.Get("reg_of_50") : T.Get("reg_of_total"))).</div>
'@

# 20. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: anade CurrencySymbol (segun la divisa del festival) en vez de € fijo
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: anade CurrencySymbol (segun la divisa del festival) en vez de € fijo' `
    -Anchor @'
    private decimal FinalPrice => Dashboard?.Registration?.FinalPrice ?? 0;
'@ `
    -Replacement @'
    private decimal FinalPrice => Dashboard?.Registration?.FinalPrice ?? 0;

    // "€" fijo sustituido por esto en todo el panel -- refleja la divisa
    // configurada para este festival en Payment Settings (EUR/USD/CAD).
    private string CurrencySymbol => CurrencyHelper.Symbol(Dashboard?.Registration?.Currency);
'@

# 21. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: precio total (cabecera) usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: precio total (cabecera) usa la divisa del festival' `
    -Anchor @'
                <div class="p-4 rounded bg-light/20 dark:bg-white/5">
                    <MudText Class="text-muted dark:text-darkmuted">@T.Get("up_final_price")</MudText>
                    <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@FinalPrice.ToString("0.00") €</MudText>
                </div>
'@ `
    -Replacement @'
                <div class="p-4 rounded bg-light/20 dark:bg-white/5">
                    <MudText Class="text-muted dark:text-darkmuted">@T.Get("up_final_price")</MudText>
                    <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@FinalPrice.ToString("0.00") @CurrencySymbol</MudText>
                </div>
'@

# 22. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: pagado/pendiente/reembolsado usan la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: pagado/pendiente/reembolsado usan la divisa del festival' `
    -Anchor @'
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@Dashboard.Registration.AmountPaid.ToString("0.00") € @T.Get("up_paid")</MudText>
                        <MudText Class="text-warning">@((FinalPrice - Dashboard.Registration.AmountPaid + Dashboard.Registration.RefundedAmount).ToString("0.00")) € @T.Get("up_pending")</MudText>
                        @if (Dashboard.Registration.RefundedAmount > 0)
                        {
                            <MudText Class="text-xs text-danger">@Dashboard.Registration.RefundedAmount.ToString("0.00") € refunded</MudText>
                        }
'@ `
    -Replacement @'
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@Dashboard.Registration.AmountPaid.ToString("0.00") @CurrencySymbol @T.Get("up_paid")</MudText>
                        <MudText Class="text-warning">@((FinalPrice - Dashboard.Registration.AmountPaid + Dashboard.Registration.RefundedAmount).ToString("0.00")) @CurrencySymbol @T.Get("up_pending")</MudText>
                        @if (Dashboard.Registration.RefundedAmount > 0)
                        {
                            <MudText Class="text-xs text-danger">@Dashboard.Registration.RefundedAmount.ToString("0.00") @CurrencySymbol refunded</MudText>
                        }
'@

# 23. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: precio total (pagado por completo) usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: precio total (pagado por completo) usa la divisa del festival' `
    -Anchor @'
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@FinalPrice.ToString("0.00") €</MudText>
                        <MudText Class="text-warning">@PaymentStatus</MudText>
'@ `
    -Replacement @'
                        <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white">@FinalPrice.ToString("0.00") @CurrencySymbol</MudText>
                        <MudText Class="text-warning">@PaymentStatus</MudText>
'@

# 24. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: importe de factura usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: importe de factura usa la divisa del festival' `
    -Anchor @'
                                    <td class="px-4 py-3">@invoice.Amount.ToString("0.00") €</td>
'@ `
    -Replacement @'
                                    <td class="px-4 py-3">@invoice.Amount.ToString("0.00") @CurrencySymbol</td>
'@

# 25. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: mini-card del dashboard (PaymentCardSuffix) usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: mini-card del dashboard (PaymentCardSuffix) usa la divisa del festival' `
    -Anchor @'
                string suffix = $"{paid:0.00} € paid · {(FinalPrice - paid + refunded):0.00} € pending";

                if (refunded > 0)
                {
                    suffix += $" · {refunded:0.00} € refunded";
                }
'@ `
    -Replacement @'
                string suffix = $"{paid:0.00} {CurrencySymbol} paid · {(FinalPrice - paid + refunded):0.00} {CurrencySymbol} pending";

                if (refunded > 0)
                {
                    suffix += $" · {refunded:0.00} {CurrencySymbol} refunded";
                }
'@

# 26. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: mini-card del dashboard (importe total) usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: mini-card del dashboard (importe total) usa la divisa del festival' `
    -Anchor @'
            return $"{FinalPrice:0.00} €";
        }
    }

    private string? CompetitionsCardSuffix
'@ `
    -Replacement @'
            return $"{FinalPrice:0.00} {CurrencySymbol}";
        }
    }

    private string? CompetitionsCardSuffix
'@

# 27. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: mini-card de la ultima factura usa la divisa del festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: mini-card de la ultima factura usa la divisa del festival' `
    -Anchor @'
            return latest is null ? null : $"Last invoice {latest.Amount:0.00} €";
'@ `
    -Replacement @'
            return latest is null ? null : $"Last invoice {latest.Amount:0.00} {CurrencySymbol}";
'@

# 28. Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor -- Registrations.razor: anade CurrencySymbol (segun la divisa del festival filtrado) en vez de € fijo
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor' `
    -Description 'Registrations.razor: anade CurrencySymbol (segun la divisa del festival filtrado) en vez de € fijo' `
    -Anchor @'
    private Guid festivalFilter = Guid.Empty;
    private Guid editionFilter = Guid.Empty;

    private List<EditionDto> EditionsForSelectedFestival =>
'@ `
    -Replacement @'
    private Guid festivalFilter = Guid.Empty;
    private Guid editionFilter = Guid.Empty;

    // "€" fijo sustituido por esto en toda la pagina -- refleja la divisa del
    // festival seleccionado en el filtro de arriba (EUR por defecto si no hay
    // ninguno seleccionado todavia).
    private string CurrencySymbol => CurrencyHelper.Symbol(festivals.FirstOrDefault(f => f.Id == festivalFilter)?.Currency);

    private List<EditionDto> EditionsForSelectedFestival =>
'@

# 29. Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor -- Registrations.razor: precio final/base de la tabla usa la divisa del festival filtrado
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor' `
    -Description 'Registrations.razor: precio final/base de la tabla usa la divisa del festival filtrado' `
    -Anchor @'
                                        <div>@registration.FinalPrice.ToString("0.##") €</div>
                                        @if (registration.FinalPrice != registration.BasePrice)
                                        {
                                            <div class="text-xs text-black/50 dark:text-white/60">Base @registration.BasePrice.ToString("0.##") €</div>
                                        }
'@ `
    -Replacement @'
                                        <div>@registration.FinalPrice.ToString("0.##") @CurrencySymbol</div>
                                        @if (registration.FinalPrice != registration.BasePrice)
                                        {
                                            <div class="text-xs text-black/50 dark:text-white/60">Base @registration.BasePrice.ToString("0.##") @CurrencySymbol</div>
                                        }
'@

# 30. Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor -- Registrations.razor: importe reembolsado de la tabla usa la divisa del festival filtrado
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor' `
    -Description 'Registrations.razor: importe reembolsado de la tabla usa la divisa del festival filtrado' `
    -Anchor @'
                                        @if (registration.RefundedAmount > 0)
                                        {
                                            <div class="text-xs text-danger mt-1">Refunded @registration.RefundedAmount.ToString("0.##") €</div>
                                        }
'@ `
    -Replacement @'
                                        @if (registration.RefundedAmount > 0)
                                        {
                                            <div class="text-xs text-danger mt-1">Refunded @registration.RefundedAmount.ToString("0.##") @CurrencySymbol</div>
                                        }
'@

# 31. Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor -- Registrations.razor: modal de reembolso usa la divisa del festival filtrado
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor' `
    -Description 'Registrations.razor: modal de reembolso usa la divisa del festival filtrado' `
    -Anchor @'
                        Refund <strong>@refundRegistration.FullName</strong>'s payment via @((refundRegistration.PaymentPlatformUsed ?? PaymentPlatform.Redsys)). Paid @refundRegistration.AmountPaid.ToString("0.00") €@(refundRegistration.RefundedAmount > 0 ? $", already refunded {refundRegistration.RefundedAmount:0.00} €" : "").
                    </p>
                    <div class="mt-4">
                        <label class="form-label">Amount to refund (€)</label>
'@ `
    -Replacement @'
                        Refund <strong>@refundRegistration.FullName</strong>'s payment via @((refundRegistration.PaymentPlatformUsed ?? PaymentPlatform.Redsys)). Paid @refundRegistration.AmountPaid.ToString("0.00") @CurrencySymbol@(refundRegistration.RefundedAmount > 0 ? $", already refunded {refundRegistration.RefundedAmount:0.00} {CurrencySymbol}" : "").
                    </p>
                    <div class="mt-4">
                        <label class="form-label">Amount to refund (@CurrencySymbol)</label>
'@

# 32. Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor -- Registrations.razor: mensaje de error del reembolso usa la divisa del festival filtrado
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor' `
    -Description 'Registrations.razor: mensaje de error del reembolso usa la divisa del festival filtrado' `
    -Anchor @'
            ShowError($"Enter an amount between 0.01 and {refundMaxAmount:0.00} €.");
'@ `
    -Replacement @'
            ShowError($"Enter an amount between 0.01 and {refundMaxAmount:0.00} {CurrencySymbol}.");
'@

# 33. Alakai.FestivalManager.Admin/Components/Pages/DiscountCodes.razor -- DiscountCodes.razor: anade CurrencySymbol (segun la divisa del festival filtrado) en vez de € fijo
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/DiscountCodes.razor' `
    -Description 'DiscountCodes.razor: anade CurrencySymbol (segun la divisa del festival filtrado) en vez de € fijo' `
    -Anchor @'
    private string festivalFilter = string.Empty;
    private string editionFilter = string.Empty;

    private List<EditionDto> EditionsForSelectedFestival =>
'@ `
    -Replacement @'
    private string festivalFilter = string.Empty;
    private string editionFilter = string.Empty;

    // "€" fijo sustituido por esto en toda la pagina -- refleja la divisa del
    // festival seleccionado en el filtro de arriba (EUR por defecto si no hay
    // ninguno seleccionado todavia).
    private string CurrencySymbol =>
        CurrencyHelper.Symbol(Guid.TryParse(festivalFilter, out Guid fid) ? festivals.FirstOrDefault(f => f.Id == fid)?.Currency : null);

    private List<EditionDto> EditionsForSelectedFestival =>
'@

# 34. Alakai.FestivalManager.Admin/Components/Pages/DiscountCodes.razor -- DiscountCodes.razor: pasa la divisa del festival filtrado al formatear el valor del descuento
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/DiscountCodes.razor' `
    -Description 'DiscountCodes.razor: pasa la divisa del festival filtrado al formatear el valor del descuento' `
    -Anchor @'
                                <td class="px-4 py-3">@FormatDiscountValue(item)</td>
'@ `
    -Replacement @'
                                <td class="px-4 py-3">@FormatDiscountValue(item, CurrencySymbol)</td>
'@

# 35. Alakai.FestivalManager.Admin/Components/Pages/DiscountCodes.razor -- DiscountCodes.razor: FormatDiscountValue recibe la divisa en vez de usar € fijo
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/DiscountCodes.razor' `
    -Description 'DiscountCodes.razor: FormatDiscountValue recibe la divisa en vez de usar € fijo' `
    -Anchor @'
    private static string FormatDiscountValue(DiscountCodeDto code)
    {
        return code.DiscountType == DiscountType.Percentage ? $"{code.DiscountValue:0.##}%" : $"{code.DiscountValue:0.##} €";
    }
'@ `
    -Replacement @'
    private static string FormatDiscountValue(DiscountCodeDto code, string currencySymbol)
    {
        return code.DiscountType == DiscountType.Percentage ? $"{code.DiscountValue:0.##}%" : $"{code.DiscountValue:0.##} {currencySymbol}";
    }
'@

# 36. Alakai.FestivalManager.Admin/Components/Pages/Buses.razor -- Buses.razor: anade CurrencySymbol (segun la divisa del festival seleccionado) en vez de € fijo
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Buses.razor' `
    -Description 'Buses.razor: anade CurrencySymbol (segun la divisa del festival seleccionado) en vez de € fijo' `
    -Anchor @'
    private List<EditionDto> editionsForSelectedFestival =>
        editions.Where(e => e.FestivalId == selectedFestivalId).OrderByDescending(e => e.Year).ToList();

    protected override async Task OnInitializedAsync()
'@ `
    -Replacement @'
    private List<EditionDto> editionsForSelectedFestival =>
        editions.Where(e => e.FestivalId == selectedFestivalId).OrderByDescending(e => e.Year).ToList();

    // "€" fijo sustituido por esto en toda la pagina -- refleja la divisa del
    // festival seleccionado arriba (EUR por defecto si no hay ninguno
    // seleccionado todavia).
    private string CurrencySymbol => CurrencyHelper.Symbol(festivals.FirstOrDefault(f => f.Id == selectedFestivalId)?.Currency);

    protected override async Task OnInitializedAsync()
'@

# 37. Alakai.FestivalManager.Admin/Components/Pages/Buses.razor -- Buses.razor: precio del bus usa la divisa del festival seleccionado
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Buses.razor' `
    -Description 'Buses.razor: precio del bus usa la divisa del festival seleccionado' `
    -Anchor @'
                                <td class="px-3 py-3">@(bus.Price > 0 ? bus.Price.ToString("0.00") + " €" : "-")</td>
'@ `
    -Replacement @'
                                <td class="px-3 py-3">@(bus.Price > 0 ? bus.Price.ToString("0.00") + " " + CurrencySymbol : "-")</td>
'@

# 38. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor: Currency pasa a ser una fila propia (aplica a Redsys y Stripe), ya no dentro de la card de Redsys
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor: Currency pasa a ser una fila propia (aplica a Redsys y Stripe), ya no dentro de la card de Redsys' `
    -Anchor @'
                    @if (isLoadingCredentials)
                    {
                        <p class="text-sm md:col-span-3 text-black/50 dark:text-white/60">Loading payment settings...</p>
                    }
                    else
                    {
                        <div class="p-4 space-y-4 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5">
                            <p class="text-xs font-semibold tracking-wide uppercase text-black dark:text-white">Redsys</p>
'@ `
    -Replacement @'
                    @if (isLoadingCredentials)
                    {
                        <p class="text-sm md:col-span-3 text-black/50 dark:text-white/60">Loading payment settings...</p>
                    }
                    else
                    {
                        <div class="p-4 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5 md:col-span-3">
                            <div class="max-w-xs">
                                <label class="block text-sm text-black/60 dark:text-white/60">Currency (applies to Redsys &amp; Stripe)</label>
                                <select class="form-select" @bind="credentialsRequest.Currency">
                                    <option value="EUR">EUR (€)</option>
                                    <option value="USD">USD ($)</option>
                                    <option value="CAD">CAD (CA$)</option>
                                </select>
                            </div>
                        </div>

                        <div class="p-4 space-y-4 rounded-lg border border-black/10 dark:border-darkborder bg-black/[0.02] dark:bg-white/5">
                            <p class="text-xs font-semibold tracking-wide uppercase text-black dark:text-white">Redsys</p>
'@

# 39. Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor -- Festivals.razor: elimina el campo Currency duplicado dentro de la card de Redsys
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Festivals.razor' `
    -Description 'Festivals.razor: elimina el campo Currency duplicado dentro de la card de Redsys' `
    -Anchor @'
                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Secret Key</label>
                                <input type="password" class="form-input" placeholder="@(hasRedsysSecretKey ? "Already configured - leave blank to keep" : "Secret Key")" @bind="credentialsRequest.RedsysSecretKey" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Currency</label>
                                <select class="form-select" @bind="credentialsRequest.Currency">
                                    <option value="EUR">EUR (€)</option>
                                    <option value="USD">USD ($)</option>
                                    <option value="CAD">CAD (CA$)</option>
                                </select>
                            </div>
                        </div>
'@ `
    -Replacement @'
                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Secret Key</label>
                                <input type="password" class="form-input" placeholder="@(hasRedsysSecretKey ? "Already configured - leave blank to keep" : "Secret Key")" @bind="credentialsRequest.RedsysSecretKey" />
                            </div>
                        </div>
'@


# ============================================================================
# EJECUCION: validar TODO primero (todo o nada), luego aplicar
# ============================================================================

Write-Host ""
Write-Host "Validando $($script:Plan.Count) cambios contra los archivos locales..." -ForegroundColor Cyan

foreach ($op in $script:Plan) {
    $err = if ($op.Type -eq 'Patch') { Test-PatchOperation -Op $op } else { Test-CreateOperation -Op $op }
    if ($err) {
        $script:PlanErrors += $err
    }
}

if ($script:PlanErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "NO se ha escrito nada. $($script:PlanErrors.Count) problema(s) encontrados:" -ForegroundColor Red
    foreach ($e in $script:PlanErrors) {
        Write-Host "  - $e" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Revisa esos archivos a mano, o dime que ha cambiado para regenerar el script." -ForegroundColor Yellow
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    if ($op.Type -eq 'Patch') {
        Invoke-PatchOperation -Op $op
    }
    else {
        Invoke-CreateOperation -Op $op
    }
}

Write-Host ""
Write-Host "Listo. Recompila y despliega para ver los importes con la divisa" -ForegroundColor Cyan
Write-Host "de cada festival (EUR/USD/CAD) en vez de € fijo en todas partes," -ForegroundColor Cyan
Write-Host "y el campo Currency de Payment Settings ya no esta dentro de la" -ForegroundColor Cyan
Write-Host "card de Redsys." -ForegroundColor Cyan
Write-Host ""