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