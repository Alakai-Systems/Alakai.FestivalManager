# Fix-AdminAuth-Full.ps1
# Consolida los dos fixes de autenticacion del Admin en produccion (Azure App Service Linux):
#   1) Data Protection Keys no persistentes -> cookie ilegible tras reciclado de contenedor.
#   2) Cookie AlakaiAdminAuth > 4096 bytes (JWTs completos como claims) -> el navegador
#      la descarta en silencio.
#
# Este script:
#   - Crea Services/Auth/MemoryCacheTicketStore.cs (si no existe ya)
#   - Parchea Program.cs: using de DataProtection + AddDataProtection().PersistKeysToFileSystem()
#   - Parchea Program.cs: registra IMemoryCache + ITicketStore y engancha options.SessionStore
#
# Ejecutar desde la raiz del repo. Idempotente: se puede correr varias veces sin duplicar cambios.

$ErrorActionPreference = "Stop"
$adminBase = "Alakai.FestivalManager.Admin"

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

    $content = Get-Content -Path $Path -Raw

    if ($content.Contains($NewString)) {
        Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan
        return $true
    }

    if (-not $content.Contains($OldString)) {
        Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow
        return $false
    }

    $updated = $content.Replace($OldString, $NewString)
    Set-Content -Path $Path -Value $updated -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

function New-FileIfMissing {
    param(
        [string]$Path,
        [string]$Content,
        [string]$Description
    )

    if (Test-Path $Path) {
        Write-Host "SKIP (ya existe): $Description" -ForegroundColor Cyan
        return $true
    }

    $directory = Split-Path -Path $Path -Parent
    if (-not (Test-Path $directory)) {
        Write-Host "SKIP (carpeta no encontrada): $directory" -ForegroundColor Yellow
        return $false
    }

    Set-Content -Path $Path -Value $Content -NoNewline
    Write-Host "OK (creado): $Description" -ForegroundColor Green
    return $true
}

$results = @()

# --- 1) Crear MemoryCacheTicketStore.cs ---
$ticketStorePath = Join-Path $adminBase "Services/Auth/MemoryCacheTicketStore.cs"

$ticketStoreContent = @'
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace Alakai.FestivalManager.Admin.Services.Auth;

/// <summary>
/// Guarda el AuthenticationTicket completo (incluyendo access_token/refresh_token) en el servidor
/// en lugar de serializarlo dentro de la cookie. La cookie del navegador solo contiene una clave
/// de sesion, evitando que el ticket cifrado supere el limite de 4096 bytes por cookie.
/// </summary>
public class MemoryCacheTicketStore : ITicketStore
{
    private const string KeyPrefix = "AlakaiAdminAuthTicket-";
    private readonly IMemoryCache _cache;

    public MemoryCacheTicketStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        string key = KeyPrefix + Guid.NewGuid();
        RenewAsync(key, ticket);
        return Task.FromResult(key);
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        MemoryCacheEntryOptions options = new();

        DateTimeOffset? expiresUtc = ticket.Properties.ExpiresUtc;
        if (expiresUtc.HasValue)
        {
            options.SetAbsoluteExpiration(expiresUtc.Value);
        }
        else
        {
            options.SetSlidingExpiration(TimeSpan.FromDays(7));
        }

        _cache.Set(key, ticket, options);
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        _cache.TryGetValue(key, out AuthenticationTicket? ticket);
        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
'@

$results += New-FileIfMissing -Path $ticketStorePath -Content $ticketStoreContent -Description "Crear MemoryCacheTicketStore.cs"

# --- 2) Patches sobre Program.cs ---
$programPath = Join-Path $adminBase "Program.cs"

$patches = @()

$patches += [PSCustomObject]@{
    Old = 'using Microsoft.AspNetCore.HttpOverrides;'
    New = @'
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
'@
    Description = "Agregar using Microsoft.AspNetCore.DataProtection"
}

$patches += [PSCustomObject]@{
    Old = @'
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
'@
    New = @'
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authentication.Cookies.ITicketStore, MemoryCacheTicketStore>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
'@
    Description = "Registrar IMemoryCache + ITicketStore (MemoryCacheTicketStore)"
}

$patches += [PSCustomObject]@{
    Old = @'
builder.Services.AddAuthorization(options =>
'@
    New = @'
builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<Microsoft.AspNetCore.Authentication.Cookies.ITicketStore>((options, store) =>
    {
        options.SessionStore = store;
    });

builder.Services.AddAuthorization(options =>
'@
    Description = "Configurar options.SessionStore = ITicketStore (ticket fuera de la cookie)"
}

$patches += [PSCustomObject]@{
    Old = @'
builder.Services.Configure<ForwardedHeadersOptions>(options =>
'@
    New = @'
string dataProtectionKeyPath = builder.Configuration["DataProtection:KeyRingPath"]
    ?? "/home/DataProtection-Keys";

builder.Services.AddDataProtection()
    .SetApplicationName("Alakai.FestivalManager.Admin")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyPath));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
'@
    Description = "Configurar AddDataProtection().PersistKeysToFileSystem()"
}

foreach ($p in $patches) {
    $results += Patch-File -Path $programPath -OldString $p.Old -NewString $p.New -Description $p.Description
}

# --- Resultado ---
if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores. No se guardaron cambios parciales fuera de lo ya escrito." -ForegroundColor Red
    exit 1
}

Write-Host "`nTodo aplicado correctamente:" -ForegroundColor Green
Write-Host " - MemoryCacheTicketStore.cs creado en Services/Auth/" -ForegroundColor Green
Write-Host " - Data Protection Keys persistentes en /home/DataProtection-Keys (o DataProtection:KeyRingPath)" -ForegroundColor Green
Write-Host " - Cookie AlakaiAdminAuth reducida a una clave de sesion; ticket completo en IMemoryCache" -ForegroundColor Green
Write-Host "`nNOTA: IMemoryCache es por-instancia. Si escalas el App Service Plan a mas de una instancia, migra a cache distribuido (Redis/SQL) o activa ARR affinity." -ForegroundColor Yellow