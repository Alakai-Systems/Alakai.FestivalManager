namespace Alakai.FestivalManager.Admin.Services.Auth;

// DelegatingHandler compartido por los HttpClient de la API que usan el login de
// Admin (IAdminTokenProvider). Si la API devuelve 401 -aunque
// GetValidAccessTokenAsync ya haya intentado refrescar el token antes de la
// llamada-, es que la sesion ya no se puede recuperar (p.ej. han pasado las 8
// horas de inactividad, o se invalido en el backend). En vez de dejar que cada
// pagina muestre el 401 como un error suelto, aqui se limpia el token en
// memoria y se manda al usuario a /login -- el comportamiento esperado de un
// logout normal.
//
// Solo se engancha (ver ApplicationDependencyInjectionExtension) en los
// HttpClient que usan IAdminTokenProvider. El formulario publico de
// inscripcion, el pago y el User Panel de los propios inscritos usan su
// propia autenticacion y no llevan este handler.
public class AdminAuthDelegatingHandler : DelegatingHandler
{
    private readonly IAdminTokenProvider _adminTokenProvider;
    private readonly NavigationManager _navigationManager;

    public AdminAuthDelegatingHandler(IAdminTokenProvider adminTokenProvider, NavigationManager navigationManager)
    {
        _adminTokenProvider = adminTokenProvider;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _adminTokenProvider.ClearTokens();
            _navigationManager.NavigateTo("/login?reason=expired", forceLoad: true);
        }

        return response;
    }
}