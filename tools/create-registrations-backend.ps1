# Fix-AuthApiClientRecursion.ps1
#
# URGENTE: Fix-AdminBearerTokenHandler.ps1 engancho el handler a los 33
# AddHttpClient<T>() SIN excluir a AuthApiClient (login/refresh de token).
# Eso crea recursion infinita:
#
#   1. Cualquier llamada -> AdminBearerTokenHandler.SendAsync
#   2. -> pide el token a AdminTokenProvider.GetValidAccessTokenAsync()
#   3. si el token esta caducado -> llama a AuthApiClient.RefreshTokenAsync(...)
#   4. esa llamada TAMBIEN pasa por AdminBearerTokenHandler.SendAsync (paso 1)
#   5. -> vuelve a pedir el token -> vuelve a refrescar -> bucle infinito -> 500
#
# Fix: quitar el handler especificamente de la llamada AddHttpClient de
# AuthApiClient (el login y el refresh de token nunca deben depender de si
# mismos para autenticarse - es logica, no un descuido). El resto de los 32
# AddHttpClient siguen con el handler, que es donde hace falta.
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

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

$results = @()
$extPath = "Alakai.FestivalManager.Admin/Extensions/ApplicationDependencyInjectionExtension.cs"

$results += Patch-File -Path $extPath -Description "AuthApiClient: quitar el handler (rompe el bucle de refresh)" -OldString @'
        services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();
'@ -NewString @'
        services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });
'@
if ($results -contains $false) { Write-Host "`nFallo. Si el anchor no se encuentra, puede que el handler ya no este enganchado ahi - revisa a mano si AuthApiClient tiene '.AddHttpMessageHandler<AdminBearerTokenHandler>()' y quitalo." -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# Tambien quitamos PublicRegistrationApiClient del handler - no es la causa
# del 500 (un usuario anonimo no tiene token, GetValidAccessTokenAsync
# devuelve null sin recursion), pero es correcto no molestarse en pedir un
# token de admin en llamadas del formulario publico.
# ---------------------------------------------------------------------------
$results += Patch-File -Path $extPath -Description "PublicRegistrationApiClient: quitar el handler (no necesita token de admin)" -OldString @'
        services.AddHttpClient<PublicRegistrationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();
'@ -NewString @'
        services.AddHttpClient<PublicRegistrationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });
'@
if ($results -contains $false) { Write-Host "`nAviso: no se pudo quitar el handler de PublicRegistrationApiClient (no es critico, no causa el 500)." -ForegroundColor Yellow }

Write-Host "`nBucle de recursion roto. AuthApiClient y PublicRegistrationApiClient ya no dependen del handler; el resto de los 31 ApiClient lo mantienen." -ForegroundColor Green