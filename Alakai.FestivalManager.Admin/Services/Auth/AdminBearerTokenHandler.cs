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