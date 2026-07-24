# Fix-AdminBearerTokenHandler.ps1
#
# CAUSA RAIZ del "todas las paginas dan 401" tras aplicar Fix-AuthorizeAudit.ps1:
#
# La mayoria de los ApiClient del Admin (DashboardApiClient, RegistrationApiClient,
# InvoiceApiClient, etc.) NUNCA adjuntaban el token Bearer en sus llamadas HTTP,
# porque sus controllers de la Api estaban abiertos (sin [Authorize]) y nunca hizo
# falta. Los ApiClient de Produccion (ProductionZoneApiClient y hermanos) SI lo
# hacen, a mano, en cada metodo, con:
#
#   string? adminToken = await _adminTokenProvider.GetValidAccessTokenAsync();
#   _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
#
# porque Produccion siempre exigio autenticacion. Al poner [Authorize(Roles=...)]
# en los otros 25 controllers, sus ApiClient correspondientes empezaron a fallar
# con 401 para CUALQUIER usuario, con CUALQUIER rol - no es un problema de roles,
# es que el token nunca se mandaba.
#
# Fix correcto (no tocar 25 archivos de ApiClient a mano, con el riesgo de
# dejarse un metodo): un DelegatingHandler centralizado que adjunta el token a
# TODAS las llamadas HTTP salientes automaticamente, enganchado una sola vez a
# los 33 AddHttpClient<T>() ya existentes.
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

function New-CodeFile {
    param([string]$Path, [string]$Content, [string]$Description)
    if (Test-Path $Path) { Write-Host "SKIP (ya existe): $Path" -ForegroundColor Cyan; return $true }
    $directory = Split-Path -Path $Path -Parent
    if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
    Set-Content -Path $Path -Value $Content -NoNewline
    Write-Host "OK: $Description -> $Path" -ForegroundColor Green
    return $true
}

function Patch-File {
    param([string]$Path, [string]$OldString, [string]$NewString, [string]$Description)
    if (-not (Test-Path $Path)) { Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow; return $false }
    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")
    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"
    if ($normalizedContent.Contains($normalizedNew)) { Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan; return $true }
    if (-not $normalizedContent.Contains($normalizedOld)) { Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow; return $false }
    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

# ---------------------------------------------------------------------------
# 1) El DelegatingHandler nuevo
# ---------------------------------------------------------------------------
$results = @()

$results += New-CodeFile -Path "Alakai.FestivalManager.Admin/Services/Auth/AdminBearerTokenHandler.cs" -Description "AdminBearerTokenHandler.cs" -Content @'
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Auth;

/// <summary>
/// Adjunta automaticamente el token Bearer del admin logueado a toda llamada
/// HTTP saliente hacia la Api, para cualquier HttpClient que lo tenga
/// enganchado via .AddHttpMessageHandler&lt;AdminBearerTokenHandler&gt;().
/// Sin esto, cualquier ApiClient que no adjunte el token a mano (la mayoria,
/// ya que solo Produccion lo hacia manualmente) recibe 401 en cuanto su
/// controller exige autenticacion.
/// </summary>
public class AdminBearerTokenHandler : DelegatingHandler
{
    private readonly IAdminTokenProvider _adminTokenProvider;

    public AdminBearerTokenHandler(IAdminTokenProvider adminTokenProvider)
    {
        _adminTokenProvider = adminTokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? adminToken = await _adminTokenProvider.GetValidAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(adminToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
'@

if ($results -contains $false) { Write-Host "`nFallo (handler)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 2) Registrar el handler en DI (Transient, como es estandar para handlers)
# ---------------------------------------------------------------------------
$extPath = "Alakai.FestivalManager.Admin/Extensions/ApplicationDependencyInjectionExtension.cs"

$results += Patch-File -Path $extPath -Description "Registrar AdminBearerTokenHandler en DI" -OldString @'
        services.AddScoped<IAdminTokenProvider, AdminTokenProvider>();
'@ -NewString @'
        services.AddScoped<IAdminTokenProvider, AdminTokenProvider>();
        services.AddTransient<AdminBearerTokenHandler>();
'@
if ($results -contains $false) { Write-Host "`nFallo (registro DI)." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 3) Enganchar el handler a los 33 AddHttpClient<T>() existentes (reemplazo
#    global - el mismo bloque de cierre se repite identico 33 veces, asi que
#    aqui NO usamos Patch-File (exige coincidencia unica) sino un reemplazo
#    de TODAS las apariciones a la vez).
# ---------------------------------------------------------------------------
$rawContent = Get-Content -Path $extPath -Raw
$usesCrlf = $rawContent.Contains("`r`n")
$normalizedContent = $rawContent -replace "`r`n", "`n"

$oldBlock = "            client.BaseAddress = new Uri(baseUrl);`n        });"
$newBlock = "            client.BaseAddress = new Uri(baseUrl);`n        }).AddHttpMessageHandler<AdminBearerTokenHandler>();"

$occurrences = ([regex]::Matches($normalizedContent, [regex]::Escape($oldBlock))).Count
$alreadyDone = ([regex]::Matches($normalizedContent, [regex]::Escape($newBlock))).Count

if ($alreadyDone -gt 0 -and $occurrences -eq 0) {
    Write-Host "SKIP (ya aplicado): enganchar handler a los AddHttpClient" -ForegroundColor Cyan
}
elseif ($occurrences -eq 0) {
    Write-Host "FALLO (anchor no encontrado): enganchar handler a los AddHttpClient - revisa indentacion/line endings del archivo." -ForegroundColor Red
    exit 1
}
else {
    $updatedNormalized = $normalizedContent.Replace($oldBlock, $newBlock)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $extPath -Value $updatedFinal -NoNewline
    Write-Host "OK: enganchado AdminBearerTokenHandler a los $occurrences AddHttpClient<T>() existentes" -ForegroundColor Green
}

Write-Host "`nA partir de ahora TODAS las llamadas del Admin a la Api llevan el token Bearer automaticamente, sin depender de que cada ApiClient lo haga a mano. Esto arregla los 401 en Dashboard y en cualquier otro controller que hayas restringido con [Authorize]." -ForegroundColor Green
Write-Host "Nota: los ApiClient de Produccion (que ya lo hacian a mano con AttachAuthHeaderAsync) siguen funcionando igual - ahora simplemente ponen el mismo header dos veces, sin conflicto." -ForegroundColor Yellow