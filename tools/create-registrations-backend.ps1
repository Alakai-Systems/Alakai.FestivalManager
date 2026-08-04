<#
.SINOPSIS
  Arregla el 401 al inscribirse/cancelar una competicion desde el panel de
  participante (UserPanel.razor).

  DIAGNOSTICO (comprobado leyendo el codigo real, no supuesto):
    - UserPanel.razor (panel del PARTICIPANTE) esta usando
      CompetitionEntryApiClient para crear/editar/borrar inscripciones a
      competiciones. Ese cliente ataca "api/competition-entries" con el
      token de ADMIN (IAdminTokenProvider) - y ese endpoint en la Api
      (CompetitionEntriesController) esta protegido con
      [Authorize(Roles = "SuperAdmin,Admin")]. Un participante normal no
      tiene ese rol -> 401 siempre, para cualquier participante, en
      cualquier momento. No tiene nada que ver con el token caducado ni
      con ningun cambio de hoy - es un bug de cableado preexistente: el
      panel de participante quedo enganchado al cliente de Admin en vez de
      al suyo propio.
    - El cliente correcto para el participante ya existe:
      UserPanelApiClient, que usa el token del propio participante
      (ITokenStorageService) y ataca "api/user-panel/...", protegido solo
      con [Authorize] (cualquier usuario autenticado). Ya tiene
      GetDashboardAsync, UpdateProfileAsync y CreateInvoiceAsync.
    - El problema es que a UserPanelApiClient le faltan los 3 metodos de
      competicion, Y a la Api (UserPanelController) le falta el endpoint
      POST para crear una inscripcion (el PUT de editar y el DELETE de
      cancelar SI existen y estan bien). La logica de negocio para crear
      (IUserPanelService.CreateCompetitionEntryAsync, en
      UserPanelService.cs) YA esta completa e implementada - incluye la
      comprobacion de seguridad de sobreescribir RegistrationId con la
      ultima inscripcion del propio usuario autenticado, para que nadie
      pueda crear una entrada para el registro de otra persona. Solo le
      faltaba el endpoint del controller que la expusiera.

  FIX: se completa el cableado que faltaba, sin tocar la logica de negocio
  (que ya era correcta) ni el CompetitionEntryApiClient (que sigue
  haciendo falta tal cual para las pantallas de Admin reales, como
  Competitions.razor / CompetitionEntries.razor).

  Archivos que modifica:
    - Api\Controllers\UserPanelController.cs                (+POST competition-entries)
    - Admin\Services\Api\UserPanelApiClient.cs               (+3 metodos: Create/Update/Delete competition entry)
    - Admin\Components\Pages\UserPanelDashboard\UserPanel.razor (usa UserPanelApiClient en vez de
                                                                   CompetitionEntryApiClient para las 3 llamadas,
                                                                   quita el @inject que se queda sin uso)

  Idempotente.

.USO
  Ejecutar desde la raiz del repo:
    .\25-fix-userpanel-competition-entries-401.ps1

  Luego: dotnet build (Api y Admin, deben compilar limpio), redeploy de
  AMBOS App Services (app-alakai-swimout-api y app-alakai-swimout-admin).

  Verificacion: como participante (no como Admin), entra al panel de
  usuario, inscribete a una competicion y luego cancelala. Ya no deberia
  salir 401 en ninguno de los dos casos.
#>

function Patch-File {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] [string]$OldString,
        [Parameter(Mandatory = $true)] [string]$NewString,
        [string]$Description = ""
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "SKIP: archivo no encontrado -> $Path" -ForegroundColor Yellow
        return
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $rawContent = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    $usesCrlf = $rawContent.Contains("`r`n")

    $content = $rawContent -replace "`r`n", "`n"
    $oldNormalized = $OldString -replace "`r`n", "`n"
    $newNormalized = $NewString -replace "`r`n", "`n"

    if ($content.Contains($newNormalized)) {
        Write-Host "SKIP: ya aplicado -> $Path ($Description)" -ForegroundColor DarkGray
        return
    }

    $occurrences = ([regex]::Matches($content, [regex]::Escape($oldNormalized))).Count

    if ($occurrences -eq 0) {
        Write-Host "SKIP: anchor no encontrado -> $Path ($Description)" -ForegroundColor Red
        return
    }

    if ($occurrences -gt 1) {
        Write-Host "SKIP: anchor ambiguo, aparece $occurrences veces -> $Path ($Description)" -ForegroundColor Red
        return
    }

    $newContent = $content.Replace($oldNormalized, $newNormalized)

    if ($usesCrlf) {
        $newContent = $newContent -replace "`n", "`r`n"
    }

    [System.IO.File]::WriteAllText($Path, $newContent, $utf8NoBom)
    Write-Host "OK: aplicado -> $Path ($Description)" -ForegroundColor Green
}

$ErrorActionPreference = "Stop"

$ApiControllerPath = ".\Alakai.FestivalManager.Api\Controllers\UserPanelController.cs"
$ApiClientPath      = ".\Alakai.FestivalManager.Admin\Services\Api\UserPanelApiClient.cs"
$RazorPath          = ".\Alakai.FestivalManager.Admin\Components\Pages\UserPanelDashboard\UserPanel.razor"

# ----------------------------------------------------------------------------
# 1. Api: +POST api/user-panel/competition-entries (crear inscripcion).
#    El PUT (editar) y el DELETE (cancelar) ya existian y estaban bien.
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ApiControllerPath `
    -Description "+POST competition-entries" `
    -OldString @'
    [HttpPut("competition-entries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> UpdateCompetitionEntry(Guid id, [FromBody] UpdateCompetitionEntryRequest request, CancellationToken cancellationToken)
'@ `
    -NewString @'
    [HttpPost("competition-entries")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> CreateCompetitionEntry([FromBody] CreateCompetitionEntryRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.CreateCompetitionEntryAsync(userId, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("competition-entries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> UpdateCompetitionEntry(Guid id, [FromBody] UpdateCompetitionEntryRequest request, CancellationToken cancellationToken)
'@

# ----------------------------------------------------------------------------
# 2. Admin: +3 metodos en UserPanelApiClient (usan el token del propio
#    participante, igual que CreateInvoiceAsync que ya funciona).
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ApiClientPath `
    -Description "+Create/Update/DeleteCompetitionEntryAsync" `
    -OldString @'
        if (response?.Success is not true || response.Data?.Dashboard is null)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Invoice could not be created.";
            throw new Exception(message);
        }

        return response.Data.Dashboard;
    }
}
'@ `
    -NewString @'
        if (response?.Success is not true || response.Data?.Dashboard is null)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Invoice could not be created.";
            throw new Exception(message);
        }

        return response.Data.Dashboard;
    }

    public async Task CreateCompetitionEntryAsync(CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/competition-entries", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be created.";
            throw new Exception(message);
        }
    }

    public async Task UpdateCompetitionEntryAsync(Guid id, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/user-panel/competition-entries/{id}", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be updated.";
            throw new Exception(message);
        }
    }

    public async Task DeleteCompetitionEntryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/user-panel/competition-entries/{id}", cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be deleted.";
            throw new Exception(message);
        }
    }
}
'@

# ----------------------------------------------------------------------------
# 3. Admin: UserPanel.razor deja de usar el cliente de Admin y pasa a usar
#    el cliente correcto del propio participante.
# ----------------------------------------------------------------------------
Patch-File `
    -Path $RazorPath `
    -Description "UpdateCompetitionEntry usa UserPanelApiClient" `
    -OldString @'
                await CompetitionEntryApiClient.UpdateAsync(EditingCompetitionEntryId.Value, request);
'@ `
    -NewString @'
                await UserPanelApiClient.UpdateCompetitionEntryAsync(EditingCompetitionEntryId.Value, request);
'@

Patch-File `
    -Path $RazorPath `
    -Description "CreateCompetitionEntry usa UserPanelApiClient" `
    -OldString @'
            await CompetitionEntryApiClient.CreateAsync(createRequest);
'@ `
    -NewString @'
            await UserPanelApiClient.CreateCompetitionEntryAsync(createRequest);
'@

Patch-File `
    -Path $RazorPath `
    -Description "DeleteCompetitionEntry usa UserPanelApiClient" `
    -OldString @'
            await CompetitionEntryApiClient.DeleteAsync(DeletingCompetitionEntryId.Value);
'@ `
    -NewString @'
            await UserPanelApiClient.DeleteCompetitionEntryAsync(DeletingCompetitionEntryId.Value);
'@

Patch-File `
    -Path $RazorPath `
    -Description "quita @inject CompetitionEntryApiClient (ya sin uso en este fichero)" `
    -OldString @'
@inject RegistrationApiClient RegistrationApiClient
@inject CompetitionEntryApiClient CompetitionEntryApiClient
@inject AccommodationApiClient AccommodationApiClient
'@ `
    -NewString @'
@inject RegistrationApiClient RegistrationApiClient
@inject AccommodationApiClient AccommodationApiClient
'@

Write-Host ""
Write-Host "Deberias ver 6 lineas 'OK: aplicado'." -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO:" -ForegroundColor Cyan
Write-Host "  1. dotnet build (Api y Admin, deben compilar limpio)." -ForegroundColor Cyan
Write-Host "  2. Redeploy de AMBOS App Services (app-alakai-swimout-api Y app-alakai-swimout-admin)." -ForegroundColor Cyan
Write-Host "     Si solo redeployas uno de los dos, el otro seguira con el bug hasta que lo redeployes tambien." -ForegroundColor Cyan
Write-Host "  3. Verificacion: como PARTICIPANTE (no Admin), inscribete a una competicion y luego cancelala." -ForegroundColor Cyan
Write-Host "     Ya no deberia dar 401 en ninguno de los dos casos." -ForegroundColor Cyan