<#
.SINOPSIS
  FIX RAIZ: modulos (comida/bus/alojamiento) que desaparecen en el panel de
  usuario para festivales como Swim Out, y en general cualquier accion del
  panel de usuario para participantes con MAS DE UN registro (en distintos
  festivales).

  DIAGNOSTICO (nuevo, distinto de los 401 anteriores):
    GetLatestRegistrationByUserIdAsync(userId, domain, ...) en
    UserPanelRepository resuelve "el registro del usuario": si se le pasa el
    dominio del festival actual, busca el registro de ESE festival; si se le
    pasa null, devuelve el registro mas reciente del usuario EN CUALQUIER
    FESTIVAL. GetDashboardAsync ya hacia esto bien (usa
    new Uri(Navigation.BaseUri).Host como dominio). Pero TODOS los demas
    metodos del panel de usuario (comida, modulos del festival, autobuses,
    alojamiento, competiciones, perfil, facturas) llaman a este metodo con
    domain: null a pelo. Para un participante con un solo registro nunca se
    nota. Para un participante registrado en MAS DE UN festival (como
    aparentemente el caso de Swim Out), "el registro mas reciente" puede ser
    el de OTRO festival, y entonces:
      - GetFestivalModulesAsync devuelve los modulos habilitados del festival
        equivocado -> el panel de Swim Out no muestra comida/bus/alojamiento
        aunque esten activados alli.
      - UpdateBusReservationAsync / DeleteBusReservationAsync /
        UpdateAccommodationReservationAsync / DeleteAccommodationReservationAsync
        rechazan la operacion con "Only the person who made this reservation
        can modify it." aunque el usuario sea el dueno real, porque comparan
        contra el ID de registro equivocado.
      - UpdateProfileAsync y CreateInvoiceAsync (estos dos NO se tocaron en
        ningun script de esta serie, ya tenian este fallo de antes) tambien
        actualizan/facturan sobre el registro equivocado.

  FIX: igual que ya funciona en GetDashboardAsync, se pasa el "domain" (el
  Host actual del participante) a traves de TODA la cadena: Razor ->
  UserPanelApiClient -> query string "?domain=..." -> UserPanelController
  ([FromQuery] string? domain) -> UserPanelService -> repositorio. Ningun
  metodo cambia su logica de negocio, solo dejan de asumir "el registro mas
  reciente sin mas" y usan "el registro de ESTE festival".

  Archivos que modifica:
    - Application\Features\UserPanel\Services\IUserPanelService.cs   (18 firmas +domain)
    - Application\Features\UserPanel\Services\UserPanelService.cs    (18 metodos +domain)
    - Api\Controllers\UserPanelController.cs                         (18 endpoints +domain)
    - Admin\Services\Api\UserPanelApiClient.cs                       (18 metodos +domain, igual que ya hace GetDashboardAsync)
    - Admin\Components\Pages\UserPanelDashboard\UserPanel.razor      (variable local currentDomain -> campo CurrentDomain,
                                                                        19 puntos de llamada pasan CurrentDomain)

  No cambia NADA de la logica de negocio, capacidad, permisos ni mensajes de
  error - solo corrige que registro se resuelve como "el mio".

  Idempotente.

.USO
  Ejecutar desde la raiz del repo:
    .\30-userpanel-domain-scoping-fix.ps1

  Luego: dotnet build (Api, Application y Admin, deben compilar limpio),
  redeploy de AMBOS App Services.

  Verificacion: con un usuario registrado en MAS DE UN festival (como el que
  reporto el problema en Swim Out), entrar al panel de Swim Out y comprobar
  que aparecen los modulos de comida/bus/alojamiento si estan activados alli.
  Tambien: reservar/editar/cancelar bus y alojamiento debe seguir funcionando
  sin el falso "Only the person who made this reservation can modify it.".
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

$IServicePath   = ".\Alakai.FestivalManager.Application\Features\UserPanel\Services\IUserPanelService.cs"
$ServicePath    = ".\Alakai.FestivalManager.Application\Features\UserPanel\Services\UserPanelService.cs"
$ControllerPath = ".\Alakai.FestivalManager.Api\Controllers\UserPanelController.cs"
$ApiClientPath  = ".\Alakai.FestivalManager.Admin\Services\Api\UserPanelApiClient.cs"
$RazorPath      = ".\Alakai.FestivalManager.Admin\Components\Pages\UserPanelDashboard\UserPanel.razor"
# ----------------------------------------------------------------------------
# 1. IUserPanelService: 18 firmas +domain
# ----------------------------------------------------------------------------
Patch-File `
    -Path $IServicePath `
    -Description "+domain en 18 firmas" `
    -OldString @'
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateProfileAsync(Guid userId, UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateCompetitionEntryAsync(Guid userId, CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateCompetitionEntryAsync(Guid userId, Guid competitionEntryId, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> DeleteCompetitionEntryAsync(Guid userId, Guid competitionEntryId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateInvoiceAsync(Guid userId, CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetMealPreferenceResponse>> GetMealPreferenceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SaveMealPreferenceResponse>> SaveMealPreferenceAsync(Guid userId, SaveMealPreferenceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<RegistrationFestivalInfoDto>> GetFestivalModulesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> GetBusReservationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusesResponse>> GetAvailableBusesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> CreateBusReservationsAsync(Guid userId, CreateBusReservationsCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateBusReservationResponse>> UpdateBusReservationAsync(Guid userId, Guid reservationId, UpdateBusReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteBusReservationResponse>> DeleteBusReservationAsync(Guid userId, Guid reservationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationReservationResponse>> GetAccommodationReservationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAvailableAccommodationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingResponse>> GetAccommodationBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> CreateAccommodationReservationAsync(Guid userId, CreateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> UpdateAccommodationReservationAsync(Guid userId, Guid reservationId, UpdateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteAccommodationReservationResponse>> DeleteAccommodationReservationAsync(Guid userId, Guid reservationId, CancellationToken cancellationToken = default);
'@ `
    -NewString @'
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateProfileAsync(Guid userId, string? domain, UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateCompetitionEntryAsync(Guid userId, string? domain, CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateCompetitionEntryAsync(Guid userId, string? domain, Guid competitionEntryId, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> DeleteCompetitionEntryAsync(Guid userId, string? domain, Guid competitionEntryId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateInvoiceAsync(Guid userId, string? domain, CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetMealPreferenceResponse>> GetMealPreferenceAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<SaveMealPreferenceResponse>> SaveMealPreferenceAsync(Guid userId, string? domain, SaveMealPreferenceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<RegistrationFestivalInfoDto>> GetFestivalModulesAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> GetBusReservationsAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusesResponse>> GetAvailableBusesAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> CreateBusReservationsAsync(Guid userId, string? domain, CreateBusReservationsCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateBusReservationResponse>> UpdateBusReservationAsync(Guid userId, string? domain, Guid reservationId, UpdateBusReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteBusReservationResponse>> DeleteBusReservationAsync(Guid userId, string? domain, Guid reservationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationReservationResponse>> GetAccommodationReservationAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAvailableAccommodationsAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingResponse>> GetAccommodationBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> CreateAccommodationReservationAsync(Guid userId, string? domain, CreateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> UpdateAccommodationReservationAsync(Guid userId, string? domain, Guid reservationId, UpdateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteAccommodationReservationResponse>> DeleteAccommodationReservationAsync(Guid userId, string? domain, Guid reservationId, CancellationToken cancellationToken = default);
'@

# ----------------------------------------------------------------------------
# 2. UserPanelService: 18 metodos +domain
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ServicePath `
    -Description "+domain en 18 metodos" `
    -OldString @'
    public async Task<ApiResponse<GetUserPanelDashboardResponse>> CreateCompetitionEntryAsync(Guid userId, CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Competition entry could not be created.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        request.RegistrationId = registration.Id;
        request.InternalNotes = null;

        CreateCompetitionEntryCommand competitionCommand = _mapper.Map<CreateCompetitionEntryCommand>(request);

        await _competitionEntryService.CreateAsync(competitionCommand, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryConfirmed, registration.Id, cancellationToken);

        return await GetDashboardAsync(userId, null, cancellationToken);
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateCompetitionEntryAsync(Guid userId, Guid competitionEntryId, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? existing = await _competitionEntryRepository.GetByIdAsync(competitionEntryId, cancellationToken);

        if (existing is null || existing.Registration.UserId != userId)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Competition entry could not be updated.",
                Data = null,
                Errors = ["Competition entry not found."]
            };
        }

        Guid registrationId = existing.RegistrationId;

        request.RegistrationId = existing.RegistrationId;
        request.InternalNotes = null;

        UpdateCompetitionEntryCommand competitionCommand = _mapper.Map<UpdateCompetitionEntryCommand>(request);

        await _competitionEntryService.UpdateAsync(competitionEntryId, competitionCommand, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryConfirmed, registrationId, cancellationToken);

        return await GetDashboardAsync(userId, null, cancellationToken);
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> DeleteCompetitionEntryAsync(Guid userId, Guid competitionEntryId, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? existing = await _competitionEntryRepository.GetByIdAsync(competitionEntryId, cancellationToken);

        if (existing is null || existing.Registration.UserId != userId)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Competition entry could not be deleted.",
                Data = null,
                Errors = ["Competition entry not found."]
            };
        }

        Guid registrationId = existing.RegistrationId;

        await _competitionEntryService.DeleteAsync(competitionEntryId, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryCancelled, registrationId, cancellationToken);

        return await GetDashboardAsync(userId, null, cancellationToken);
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateProfileAsync(Guid userId, UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _userPanelRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Profile could not be updated.",
                Data = null,
                Errors = ["User not found."]
            };
        }

        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Country = request.Country;
        user.City = request.City;

        if (registration is not null)
        {
            registration.FirstName = request.FirstName;
            registration.LastName = request.LastName;
            registration.Email = request.Email;
            registration.Phone = request.Phone;
            registration.Country = request.Country;
            registration.City = request.City;
            registration.DocumentNumber = request.DocumentNumber;
            registration.DocumentCountry = request.DocumentCountry;
        }

        await _userPanelRepository.SaveChangesAsync(cancellationToken);

        return await GetDashboardAsync(userId, null, cancellationToken);
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> CreateInvoiceAsync(Guid userId, CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Invoice could not be created.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        CreateInvoiceCommand command = new()
        {
            RegistrationId = registration.Id,
            FiscalName = request.FiscalName,
            TaxId = request.TaxId,
            Address = request.Address,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country
        };

        await _invoiceService.CreateAsync(command, cancellationToken);

        return await GetDashboardAsync(userId, null, cancellationToken);
    }

    public async Task<ApiResponse<GetMealPreferenceResponse>> GetMealPreferenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetMealPreferenceResponse>
            {
                Success = false,
                Message = "Meal preference could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _mealPreferenceService.GetByRegistrationIdAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<SaveMealPreferenceResponse>> SaveMealPreferenceAsync(Guid userId, SaveMealPreferenceCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<SaveMealPreferenceResponse>
            {
                Success = false,
                Message = "Meal preference could not be saved.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.RegistrationId = registration.Id;

        return await _mealPreferenceService.SaveAsync(command, cancellationToken);
    }

    public async Task<ApiResponse<RegistrationFestivalInfoDto>> GetFestivalModulesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<RegistrationFestivalInfoDto>
            {
                Success = false,
                Message = "Festival info could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _registrationFestivalInfoService.GetForRegistrationAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetBusReservationsResponse>> GetBusReservationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetBusReservationsResponse>
            {
                Success = false,
                Message = "Bus reservations could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _busReservationService.GetByRegistrationIdAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetBusesResponse>> GetAvailableBusesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetBusesResponse>
            {
                Success = false,
                Message = "Available buses could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _busService.GetAvailableForRegistrationAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetBusReservationsResponse>> CreateBusReservationsAsync(Guid userId, CreateBusReservationsCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetBusReservationsResponse>
            {
                Success = false,
                Message = "Bus reservation could not be created.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.RegistrationId = registration.Id;

        return await _busReservationService.CreateManyAsync(command, cancellationToken);
    }

    public async Task<ApiResponse<CreateBusReservationResponse>> UpdateBusReservationAsync(Guid userId, Guid reservationId, UpdateBusReservationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<CreateBusReservationResponse>
            {
                Success = false,
                Message = "Bus reservation could not be updated.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.ReservationId = reservationId;
        command.RequestingRegistrationId = registration.Id;

        return await _busReservationService.UpdateAsync(command, isAdmin: false, cancellationToken);
    }

    public async Task<ApiResponse<DeleteBusReservationResponse>> DeleteBusReservationAsync(Guid userId, Guid reservationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<DeleteBusReservationResponse>
            {
                Success = false,
                Message = "Bus reservation could not be cancelled.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _busReservationService.DeleteAsync(reservationId, registration.Id, isAdmin: false, cancellationToken);
    }

    public async Task<ApiResponse<GetAccommodationReservationResponse>> GetAccommodationReservationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetAccommodationReservationResponse>
            {
                Success = false,
                Message = "Accommodation reservation could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _accommodationReservationService.GetByResponsibleRegistrationIdAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAvailableAccommodationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetAccommodationBuildingsResponse>
            {
                Success = false,
                Message = "Available accommodations could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _accommodationBuildingService.GetAvailableForRegistrationAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetAccommodationBuildingResponse>> GetAccommodationBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        return await _accommodationBuildingService.GetByIdAsync(buildingId, cancellationToken);
    }

    public async Task<ApiResponse<CreateAccommodationReservationResponse>> CreateAccommodationReservationAsync(Guid userId, CreateAccommodationReservationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<CreateAccommodationReservationResponse>
            {
                Success = false,
                Message = "Accommodation reservation could not be created.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.ResponsibleRegistrationId = registration.Id;

        return await _accommodationReservationService.CreateAsync(command, cancellationToken);
    }

    public async Task<ApiResponse<CreateAccommodationReservationResponse>> UpdateAccommodationReservationAsync(Guid userId, Guid reservationId, UpdateAccommodationReservationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<CreateAccommodationReservationResponse>
            {
                Success = false,
                Message = "Accommodation reservation could not be updated.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.ReservationId = reservationId;
        command.RequestingRegistrationId = registration.Id;

        return await _accommodationReservationService.UpdateAsync(command, isAdmin: false, cancellationToken);
    }

    public async Task<ApiResponse<DeleteAccommodationReservationResponse>> DeleteAccommodationReservationAsync(Guid userId, Guid reservationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, null, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<DeleteAccommodationReservationResponse>
            {
                Success = false,
                Message = "Accommodation reservation could not be cancelled.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _accommodationReservationService.DeleteAsync(reservationId, registration.Id, isAdmin: false, cancellationToken);
    }
}
'@ `
    -NewString @'
    public async Task<ApiResponse<GetUserPanelDashboardResponse>> CreateCompetitionEntryAsync(Guid userId, string? domain, CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Competition entry could not be created.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        request.RegistrationId = registration.Id;
        request.InternalNotes = null;

        CreateCompetitionEntryCommand competitionCommand = _mapper.Map<CreateCompetitionEntryCommand>(request);

        await _competitionEntryService.CreateAsync(competitionCommand, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryConfirmed, registration.Id, cancellationToken);

        return await GetDashboardAsync(userId, domain, cancellationToken);
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateCompetitionEntryAsync(Guid userId, string? domain, Guid competitionEntryId, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? existing = await _competitionEntryRepository.GetByIdAsync(competitionEntryId, cancellationToken);

        if (existing is null || existing.Registration.UserId != userId)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Competition entry could not be updated.",
                Data = null,
                Errors = ["Competition entry not found."]
            };
        }

        Guid registrationId = existing.RegistrationId;

        request.RegistrationId = existing.RegistrationId;
        request.InternalNotes = null;

        UpdateCompetitionEntryCommand competitionCommand = _mapper.Map<UpdateCompetitionEntryCommand>(request);

        await _competitionEntryService.UpdateAsync(competitionEntryId, competitionCommand, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryConfirmed, registrationId, cancellationToken);

        return await GetDashboardAsync(userId, domain, cancellationToken);
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> DeleteCompetitionEntryAsync(Guid userId, string? domain, Guid competitionEntryId, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? existing = await _competitionEntryRepository.GetByIdAsync(competitionEntryId, cancellationToken);

        if (existing is null || existing.Registration.UserId != userId)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Competition entry could not be deleted.",
                Data = null,
                Errors = ["Competition entry not found."]
            };
        }

        Guid registrationId = existing.RegistrationId;

        await _competitionEntryService.DeleteAsync(competitionEntryId, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryCancelled, registrationId, cancellationToken);

        return await GetDashboardAsync(userId, domain, cancellationToken);
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateProfileAsync(Guid userId, string? domain, UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _userPanelRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Profile could not be updated.",
                Data = null,
                Errors = ["User not found."]
            };
        }

        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Country = request.Country;
        user.City = request.City;

        if (registration is not null)
        {
            registration.FirstName = request.FirstName;
            registration.LastName = request.LastName;
            registration.Email = request.Email;
            registration.Phone = request.Phone;
            registration.Country = request.Country;
            registration.City = request.City;
            registration.DocumentNumber = request.DocumentNumber;
            registration.DocumentCountry = request.DocumentCountry;
        }

        await _userPanelRepository.SaveChangesAsync(cancellationToken);

        return await GetDashboardAsync(userId, domain, cancellationToken);
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> CreateInvoiceAsync(Guid userId, string? domain, CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "Invoice could not be created.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        CreateInvoiceCommand command = new()
        {
            RegistrationId = registration.Id,
            FiscalName = request.FiscalName,
            TaxId = request.TaxId,
            Address = request.Address,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country
        };

        await _invoiceService.CreateAsync(command, cancellationToken);

        return await GetDashboardAsync(userId, domain, cancellationToken);
    }

    public async Task<ApiResponse<GetMealPreferenceResponse>> GetMealPreferenceAsync(Guid userId, string? domain, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetMealPreferenceResponse>
            {
                Success = false,
                Message = "Meal preference could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _mealPreferenceService.GetByRegistrationIdAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<SaveMealPreferenceResponse>> SaveMealPreferenceAsync(Guid userId, string? domain, SaveMealPreferenceCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<SaveMealPreferenceResponse>
            {
                Success = false,
                Message = "Meal preference could not be saved.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.RegistrationId = registration.Id;

        return await _mealPreferenceService.SaveAsync(command, cancellationToken);
    }

    public async Task<ApiResponse<RegistrationFestivalInfoDto>> GetFestivalModulesAsync(Guid userId, string? domain, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<RegistrationFestivalInfoDto>
            {
                Success = false,
                Message = "Festival info could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _registrationFestivalInfoService.GetForRegistrationAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetBusReservationsResponse>> GetBusReservationsAsync(Guid userId, string? domain, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetBusReservationsResponse>
            {
                Success = false,
                Message = "Bus reservations could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _busReservationService.GetByRegistrationIdAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetBusesResponse>> GetAvailableBusesAsync(Guid userId, string? domain, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetBusesResponse>
            {
                Success = false,
                Message = "Available buses could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _busService.GetAvailableForRegistrationAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetBusReservationsResponse>> CreateBusReservationsAsync(Guid userId, string? domain, CreateBusReservationsCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetBusReservationsResponse>
            {
                Success = false,
                Message = "Bus reservation could not be created.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.RegistrationId = registration.Id;

        return await _busReservationService.CreateManyAsync(command, cancellationToken);
    }

    public async Task<ApiResponse<CreateBusReservationResponse>> UpdateBusReservationAsync(Guid userId, string? domain, Guid reservationId, UpdateBusReservationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<CreateBusReservationResponse>
            {
                Success = false,
                Message = "Bus reservation could not be updated.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.ReservationId = reservationId;
        command.RequestingRegistrationId = registration.Id;

        return await _busReservationService.UpdateAsync(command, isAdmin: false, cancellationToken);
    }

    public async Task<ApiResponse<DeleteBusReservationResponse>> DeleteBusReservationAsync(Guid userId, string? domain, Guid reservationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<DeleteBusReservationResponse>
            {
                Success = false,
                Message = "Bus reservation could not be cancelled.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _busReservationService.DeleteAsync(reservationId, registration.Id, isAdmin: false, cancellationToken);
    }

    public async Task<ApiResponse<GetAccommodationReservationResponse>> GetAccommodationReservationAsync(Guid userId, string? domain, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetAccommodationReservationResponse>
            {
                Success = false,
                Message = "Accommodation reservation could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _accommodationReservationService.GetByResponsibleRegistrationIdAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAvailableAccommodationsAsync(Guid userId, string? domain, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<GetAccommodationBuildingsResponse>
            {
                Success = false,
                Message = "Available accommodations could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _accommodationBuildingService.GetAvailableForRegistrationAsync(registration.Id, cancellationToken);
    }

    public async Task<ApiResponse<GetAccommodationBuildingResponse>> GetAccommodationBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        return await _accommodationBuildingService.GetByIdAsync(buildingId, cancellationToken);
    }

    public async Task<ApiResponse<CreateAccommodationReservationResponse>> CreateAccommodationReservationAsync(Guid userId, string? domain, CreateAccommodationReservationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<CreateAccommodationReservationResponse>
            {
                Success = false,
                Message = "Accommodation reservation could not be created.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.ResponsibleRegistrationId = registration.Id;

        return await _accommodationReservationService.CreateAsync(command, cancellationToken);
    }

    public async Task<ApiResponse<CreateAccommodationReservationResponse>> UpdateAccommodationReservationAsync(Guid userId, string? domain, Guid reservationId, UpdateAccommodationReservationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<CreateAccommodationReservationResponse>
            {
                Success = false,
                Message = "Accommodation reservation could not be updated.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        command.ReservationId = reservationId;
        command.RequestingRegistrationId = registration.Id;

        return await _accommodationReservationService.UpdateAsync(command, isAdmin: false, cancellationToken);
    }

    public async Task<ApiResponse<DeleteAccommodationReservationResponse>> DeleteAccommodationReservationAsync(Guid userId, string? domain, Guid reservationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<DeleteAccommodationReservationResponse>
            {
                Success = false,
                Message = "Accommodation reservation could not be cancelled.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        return await _accommodationReservationService.DeleteAsync(reservationId, registration.Id, isAdmin: false, cancellationToken);
    }
}
'@

# ----------------------------------------------------------------------------
# 3. UserPanelController: 18 endpoints +domain
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ControllerPath `
    -Description "+domain en 18 endpoints" `
    -OldString @'
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> UpdateProfile([FromBody] UpdateUserPanelProfileRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.UpdateProfileAsync(userId, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

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
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.UpdateCompetitionEntryAsync(userId, id, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("competition-entries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> DeleteCompetitionEntry(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.DeleteCompetitionEntryAsync(userId, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("invoices")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> CreateInvoice([FromBody] CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.CreateInvoiceAsync(userId, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("meal-preference")]
    public async Task<ActionResult<ApiResponse<GetMealPreferenceResponse>>> GetMealPreference(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetMealPreferenceResponse> response = await _userPanelService.GetMealPreferenceAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("meal-preference")]
    public async Task<ActionResult<ApiResponse<SaveMealPreferenceResponse>>> SaveMealPreference([FromBody] SaveMealPreferenceCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<SaveMealPreferenceResponse> response = await _userPanelService.SaveMealPreferenceAsync(userId, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("festival-modules")]
    public async Task<ActionResult<ApiResponse<RegistrationFestivalInfoDto>>> GetFestivalModules(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<RegistrationFestivalInfoDto> response = await _userPanelService.GetFestivalModulesAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("bus-reservations")]
    public async Task<ActionResult<ApiResponse<GetBusReservationsResponse>>> GetBusReservations(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusReservationsResponse> response = await _userPanelService.GetBusReservationsAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("available-buses")]
    public async Task<ActionResult<ApiResponse<GetBusesResponse>>> GetAvailableBuses(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusesResponse> response = await _userPanelService.GetAvailableBusesAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("bus-reservations")]
    public async Task<ActionResult<ApiResponse<GetBusReservationsResponse>>> CreateBusReservations([FromBody] CreateBusReservationsCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusReservationsResponse> response = await _userPanelService.CreateBusReservationsAsync(userId, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("bus-reservations/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CreateBusReservationResponse>>> UpdateBusReservation(Guid id, [FromBody] UpdateBusReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateBusReservationResponse> response = await _userPanelService.UpdateBusReservationAsync(userId, id, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("bus-reservations/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteBusReservationResponse>>> DeleteBusReservation(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<DeleteBusReservationResponse> response = await _userPanelService.DeleteBusReservationAsync(userId, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("accommodation-reservation")]
    public async Task<ActionResult<ApiResponse<GetAccommodationReservationResponse>>> GetAccommodationReservation(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationReservationResponse> response = await _userPanelService.GetAccommodationReservationAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("available-accommodations")]
    public async Task<ActionResult<ApiResponse<GetAccommodationBuildingsResponse>>> GetAvailableAccommodations(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationBuildingsResponse> response = await _userPanelService.GetAvailableAccommodationsAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("accommodation-buildings/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetAccommodationBuildingResponse>>> GetAccommodationBuilding(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationBuildingResponse> response = await _userPanelService.GetAccommodationBuildingAsync(id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("accommodation-reservation")]
    public async Task<ActionResult<ApiResponse<CreateAccommodationReservationResponse>>> CreateAccommodationReservation([FromBody] CreateAccommodationReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateAccommodationReservationResponse> response = await _userPanelService.CreateAccommodationReservationAsync(userId, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("accommodation-reservation/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CreateAccommodationReservationResponse>>> UpdateAccommodationReservation(Guid id, [FromBody] UpdateAccommodationReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateAccommodationReservationResponse> response = await _userPanelService.UpdateAccommodationReservationAsync(userId, id, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("accommodation-reservation/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteAccommodationReservationResponse>>> DeleteAccommodationReservation(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<DeleteAccommodationReservationResponse> response = await _userPanelService.DeleteAccommodationReservationAsync(userId, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
'@ `
    -NewString @'
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> UpdateProfile([FromQuery] string? domain, [FromBody] UpdateUserPanelProfileRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.UpdateProfileAsync(userId, domain, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("competition-entries")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> CreateCompetitionEntry([FromQuery] string? domain, [FromBody] CreateCompetitionEntryRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.CreateCompetitionEntryAsync(userId, domain, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("competition-entries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> UpdateCompetitionEntry([FromQuery] string? domain, Guid id, [FromBody] UpdateCompetitionEntryRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.UpdateCompetitionEntryAsync(userId, domain, id, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("competition-entries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> DeleteCompetitionEntry([FromQuery] string? domain, Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.DeleteCompetitionEntryAsync(userId, domain, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("invoices")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> CreateInvoice([FromQuery] string? domain, [FromBody] CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.CreateInvoiceAsync(userId, domain, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("meal-preference")]
    public async Task<ActionResult<ApiResponse<GetMealPreferenceResponse>>> GetMealPreference([FromQuery] string? domain, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetMealPreferenceResponse> response = await _userPanelService.GetMealPreferenceAsync(userId, domain, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("meal-preference")]
    public async Task<ActionResult<ApiResponse<SaveMealPreferenceResponse>>> SaveMealPreference([FromQuery] string? domain, [FromBody] SaveMealPreferenceCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<SaveMealPreferenceResponse> response = await _userPanelService.SaveMealPreferenceAsync(userId, domain, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("festival-modules")]
    public async Task<ActionResult<ApiResponse<RegistrationFestivalInfoDto>>> GetFestivalModules([FromQuery] string? domain, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<RegistrationFestivalInfoDto> response = await _userPanelService.GetFestivalModulesAsync(userId, domain, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("bus-reservations")]
    public async Task<ActionResult<ApiResponse<GetBusReservationsResponse>>> GetBusReservations([FromQuery] string? domain, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusReservationsResponse> response = await _userPanelService.GetBusReservationsAsync(userId, domain, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("available-buses")]
    public async Task<ActionResult<ApiResponse<GetBusesResponse>>> GetAvailableBuses([FromQuery] string? domain, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusesResponse> response = await _userPanelService.GetAvailableBusesAsync(userId, domain, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("bus-reservations")]
    public async Task<ActionResult<ApiResponse<GetBusReservationsResponse>>> CreateBusReservations([FromQuery] string? domain, [FromBody] CreateBusReservationsCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusReservationsResponse> response = await _userPanelService.CreateBusReservationsAsync(userId, domain, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("bus-reservations/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CreateBusReservationResponse>>> UpdateBusReservation([FromQuery] string? domain, Guid id, [FromBody] UpdateBusReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateBusReservationResponse> response = await _userPanelService.UpdateBusReservationAsync(userId, domain, id, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("bus-reservations/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteBusReservationResponse>>> DeleteBusReservation([FromQuery] string? domain, Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<DeleteBusReservationResponse> response = await _userPanelService.DeleteBusReservationAsync(userId, domain, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("accommodation-reservation")]
    public async Task<ActionResult<ApiResponse<GetAccommodationReservationResponse>>> GetAccommodationReservation([FromQuery] string? domain, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationReservationResponse> response = await _userPanelService.GetAccommodationReservationAsync(userId, domain, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("available-accommodations")]
    public async Task<ActionResult<ApiResponse<GetAccommodationBuildingsResponse>>> GetAvailableAccommodations([FromQuery] string? domain, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationBuildingsResponse> response = await _userPanelService.GetAvailableAccommodationsAsync(userId, domain, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("accommodation-buildings/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetAccommodationBuildingResponse>>> GetAccommodationBuilding(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationBuildingResponse> response = await _userPanelService.GetAccommodationBuildingAsync(id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("accommodation-reservation")]
    public async Task<ActionResult<ApiResponse<CreateAccommodationReservationResponse>>> CreateAccommodationReservation([FromQuery] string? domain, [FromBody] CreateAccommodationReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateAccommodationReservationResponse> response = await _userPanelService.CreateAccommodationReservationAsync(userId, domain, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("accommodation-reservation/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CreateAccommodationReservationResponse>>> UpdateAccommodationReservation([FromQuery] string? domain, Guid id, [FromBody] UpdateAccommodationReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateAccommodationReservationResponse> response = await _userPanelService.UpdateAccommodationReservationAsync(userId, domain, id, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("accommodation-reservation/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteAccommodationReservationResponse>>> DeleteAccommodationReservation([FromQuery] string? domain, Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<DeleteAccommodationReservationResponse> response = await _userPanelService.DeleteAccommodationReservationAsync(userId, domain, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
'@

# ----------------------------------------------------------------------------
# 4. UserPanelApiClient: 18 metodos +domain
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ApiClientPath `
    -Description "+domain en 18 metodos" `
    -OldString @'
    public async Task<UserPanelDashboardDto?> UpdateProfileAsync(UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync("api/user-panel/profile", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Dashboard;
    }

    public async Task<UserPanelDashboardDto> CreateInvoiceAsync(CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/invoices", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

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

    public async Task<MealPreferenceDto?> GetMealPreferenceAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetMealPreferenceResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetMealPreferenceResponse>>("api/user-panel/meal-preference", cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Preference;
    }

    public async Task<MealPreferenceDto?> SaveMealPreferenceAsync(SaveMealPreferenceRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/meal-preference", request, cancellationToken);

        ApiResponse<SaveMealPreferenceResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<SaveMealPreferenceResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Meal preference could not be saved.";
            throw new Exception(message);
        }

        return response.Data?.Preference;
    }

    public async Task<int> GetEnabledFestivalModulesAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return 0;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<RegistrationFestivalInfoDto>? response = await _httpClient.GetFromJsonAsync<ApiResponse<RegistrationFestivalInfoDto>>("api/user-panel/festival-modules", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            return 0;
        }

        return response.Data.EnabledModules;
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetBusReservationsAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetBusReservationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusReservationsResponse>>("api/user-panel/bus-reservations", cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<IReadOnlyList<BusDto>> GetAvailableBusesAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetBusesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusesResponse>>("api/user-panel/available-buses", cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Buses ?? [];
    }

    public async Task<IReadOnlyList<BusReservationDto>> CreateBusReservationsAsync(CreateBusReservationsRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/bus-reservations", request, cancellationToken);

        ApiResponse<GetBusReservationsResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetBusReservationsResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be created.";
            throw new Exception(message);
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<BusReservationDto?> UpdateBusReservationAsync(Guid id, UpdateBusReservationRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/user-panel/bus-reservations/{id}", request, cancellationToken);

        ApiResponse<CreateBusReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateBusReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be updated.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task DeleteBusReservationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/user-panel/bus-reservations/{id}", cancellationToken);

        ApiResponse<DeleteBusReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DeleteBusReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be cancelled.";
            throw new Exception(message);
        }
    }

    public async Task<AccommodationReservationDto?> GetAccommodationReservationAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetAccommodationReservationResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationReservationResponse>>("api/user-panel/accommodation-reservation", cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Reservation;
    }

    public async Task<IReadOnlyList<AccommodationBuildingSummaryDto>> GetAvailableAccommodationsAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetAccommodationBuildingsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingsResponse>>("api/user-panel/available-accommodations", cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Buildings ?? [];
    }

    public async Task<AccommodationBuildingDto?> GetAccommodationBuildingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetAccommodationBuildingResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingResponse>>($"api/user-panel/accommodation-buildings/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Building;
    }

    public async Task<AccommodationReservationDto?> CreateAccommodationReservationAsync(CreateAccommodationReservationRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/accommodation-reservation", request, cancellationToken);

        ApiResponse<CreateAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be created.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task<AccommodationReservationDto?> UpdateAccommodationReservationAsync(Guid id, UpdateAccommodationReservationRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/user-panel/accommodation-reservation/{id}", request, cancellationToken);

        ApiResponse<CreateAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be updated.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task DeleteAccommodationReservationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/user-panel/accommodation-reservation/{id}", cancellationToken);

        ApiResponse<DeleteAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DeleteAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be cancelled.";
            throw new Exception(message);
        }
    }
}
'@ `
    -NewString @'
    public async Task<UserPanelDashboardDto?> UpdateProfileAsync(UpdateUserPanelProfileRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/profile"
            : $"api/user-panel/profile?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Dashboard;
    }

    public async Task<UserPanelDashboardDto> CreateInvoiceAsync(CreateUserPanelInvoiceRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/invoices"
            : $"api/user-panel/invoices?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true || response.Data?.Dashboard is null)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Invoice could not be created.";
            throw new Exception(message);
        }

        return response.Data.Dashboard;
    }

    public async Task CreateCompetitionEntryAsync(CreateCompetitionEntryRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/competition-entries"
            : $"api/user-panel/competition-entries?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be created.";
            throw new Exception(message);
        }
    }

    public async Task UpdateCompetitionEntryAsync(Guid id, UpdateCompetitionEntryRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/competition-entries/{id}"
            : $"api/user-panel/competition-entries/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be updated.";
            throw new Exception(message);
        }
    }

    public async Task DeleteCompetitionEntryAsync(Guid id, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/competition-entries/{id}"
            : $"api/user-panel/competition-entries/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync(url, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Competition entry could not be deleted.";
            throw new Exception(message);
        }
    }

    public async Task<MealPreferenceDto?> GetMealPreferenceAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/meal-preference"
            : $"api/user-panel/meal-preference?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetMealPreferenceResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetMealPreferenceResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Preference;
    }

    public async Task<MealPreferenceDto?> SaveMealPreferenceAsync(SaveMealPreferenceRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/meal-preference"
            : $"api/user-panel/meal-preference?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<SaveMealPreferenceResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<SaveMealPreferenceResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Meal preference could not be saved.";
            throw new Exception(message);
        }

        return response.Data?.Preference;
    }

    public async Task<int> GetEnabledFestivalModulesAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return 0;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/festival-modules"
            : $"api/user-panel/festival-modules?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<RegistrationFestivalInfoDto>? response = await _httpClient.GetFromJsonAsync<ApiResponse<RegistrationFestivalInfoDto>>(url, cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            return 0;
        }

        return response.Data.EnabledModules;
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetBusReservationsAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/bus-reservations"
            : $"api/user-panel/bus-reservations?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetBusReservationsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusReservationsResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<IReadOnlyList<BusDto>> GetAvailableBusesAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/available-buses"
            : $"api/user-panel/available-buses?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetBusesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetBusesResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Buses ?? [];
    }

    public async Task<IReadOnlyList<BusReservationDto>> CreateBusReservationsAsync(CreateBusReservationsRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/bus-reservations"
            : $"api/user-panel/bus-reservations?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<GetBusReservationsResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetBusReservationsResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be created.";
            throw new Exception(message);
        }

        return response.Data?.Reservations ?? [];
    }

    public async Task<BusReservationDto?> UpdateBusReservationAsync(Guid id, UpdateBusReservationRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/bus-reservations/{id}"
            : $"api/user-panel/bus-reservations/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);

        ApiResponse<CreateBusReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateBusReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be updated.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task DeleteBusReservationAsync(Guid id, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/bus-reservations/{id}"
            : $"api/user-panel/bus-reservations/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync(url, cancellationToken);

        ApiResponse<DeleteBusReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DeleteBusReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Bus reservation could not be cancelled.";
            throw new Exception(message);
        }
    }

    public async Task<AccommodationReservationDto?> GetAccommodationReservationAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/accommodation-reservation"
            : $"api/user-panel/accommodation-reservation?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetAccommodationReservationResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationReservationResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Reservation;
    }

    public async Task<IReadOnlyList<AccommodationBuildingSummaryDto>> GetAvailableAccommodationsAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/available-accommodations"
            : $"api/user-panel/available-accommodations?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<GetAccommodationBuildingsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingsResponse>>(url, cancellationToken);

        if (response?.Success is not true)
        {
            return [];
        }

        return response.Data?.Buildings ?? [];
    }

    public async Task<AccommodationBuildingDto?> GetAccommodationBuildingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetAccommodationBuildingResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetAccommodationBuildingResponse>>($"api/user-panel/accommodation-buildings/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Building;
    }

    public async Task<AccommodationReservationDto?> CreateAccommodationReservationAsync(CreateAccommodationReservationRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/accommodation-reservation"
            : $"api/user-panel/accommodation-reservation?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        ApiResponse<CreateAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be created.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task<AccommodationReservationDto?> UpdateAccommodationReservationAsync(Guid id, UpdateAccommodationReservationRequest request, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/accommodation-reservation/{id}"
            : $"api/user-panel/accommodation-reservation/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);

        ApiResponse<CreateAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be updated.";
            throw new Exception(message);
        }

        return response.Data?.Reservation;
    }

    public async Task DeleteAccommodationReservationAsync(Guid id, string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? $"api/user-panel/accommodation-reservation/{id}"
            : $"api/user-panel/accommodation-reservation/{id}?domain={Uri.EscapeDataString(domain)}";

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync(url, cancellationToken);

        ApiResponse<DeleteAccommodationReservationResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DeleteAccommodationReservationResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Accommodation reservation could not be cancelled.";
            throw new Exception(message);
        }
    }
}
'@

# ----------------------------------------------------------------------------
# 5. UserPanel.razor: variable local -> campo CurrentDomain
# ----------------------------------------------------------------------------
Patch-File `
    -Path $RazorPath `
    -Description "campo CurrentDomain (promovido de variable local)" `
    -OldString @'
    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            string? currentDomain = new Uri(Navigation.BaseUri).Host;
            Dashboard = await UserPanelApiClient.GetDashboardAsync(currentDomain);
'@ `
    -NewString @'
    private string? CurrentDomain;

    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            CurrentDomain = new Uri(Navigation.BaseUri).Host;
            Dashboard = await UserPanelApiClient.GetDashboardAsync(CurrentDomain);
'@

# ----------------------------------------------------------------------------
# 6. UserPanel.razor: 18 puntos de llamada pasan CurrentDomain
# ----------------------------------------------------------------------------
Patch-File `
    -Path $RazorPath `
    -Description "meal-preference: cargar" `
    -OldString @'
MealPreferenceDto? preference = await UserPanelApiClient.GetMealPreferenceAsync();
'@ `
    -NewString @'
MealPreferenceDto? preference = await UserPanelApiClient.GetMealPreferenceAsync(CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "meal-preference: guardar" `
    -OldString @'
await UserPanelApiClient.SaveMealPreferenceAsync(request);
'@ `
    -NewString @'
await UserPanelApiClient.SaveMealPreferenceAsync(request, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "bus: cargar reservas" `
    -OldString @'
BusReservations = (await UserPanelApiClient.GetBusReservationsAsync()).ToList();
'@ `
    -NewString @'
BusReservations = (await UserPanelApiClient.GetBusReservationsAsync(CurrentDomain)).ToList();
'@
Patch-File `
    -Path $RazorPath `
    -Description "bus: cargar disponibles" `
    -OldString @'
AvailableBuses = (await UserPanelApiClient.GetAvailableBusesAsync()).ToList();
'@ `
    -NewString @'
AvailableBuses = (await UserPanelApiClient.GetAvailableBusesAsync(CurrentDomain)).ToList();
'@
Patch-File `
    -Path $RazorPath `
    -Description "bus: crear reservas" `
    -OldString @'
await UserPanelApiClient.CreateBusReservationsAsync(request);
'@ `
    -NewString @'
await UserPanelApiClient.CreateBusReservationsAsync(request, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "bus: actualizar reserva" `
    -OldString @'
await UserPanelApiClient.UpdateBusReservationAsync(editingBusReservation.Id, request);
'@ `
    -NewString @'
await UserPanelApiClient.UpdateBusReservationAsync(editingBusReservation.Id, request, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "bus: cancelar reserva" `
    -OldString @'
await UserPanelApiClient.DeleteBusReservationAsync(cancellingBusReservation.Id);
'@ `
    -NewString @'
await UserPanelApiClient.DeleteBusReservationAsync(cancellingBusReservation.Id, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "modulos del festival habilitados" `
    -OldString @'
EnabledFestivalModules = await UserPanelApiClient.GetEnabledFestivalModulesAsync();
'@ `
    -NewString @'
EnabledFestivalModules = await UserPanelApiClient.GetEnabledFestivalModulesAsync(CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "alojamiento: cargar reserva existente" `
    -OldString @'
ExistingReservation = await UserPanelApiClient.GetAccommodationReservationAsync();
'@ `
    -NewString @'
ExistingReservation = await UserPanelApiClient.GetAccommodationReservationAsync(CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "alojamiento: cargar disponibles" `
    -OldString @'
AvailableAccommodationBuildings = (await UserPanelApiClient.GetAvailableAccommodationsAsync()).ToList();
'@ `
    -NewString @'
AvailableAccommodationBuildings = (await UserPanelApiClient.GetAvailableAccommodationsAsync(CurrentDomain)).ToList();
'@
Patch-File `
    -Path $RazorPath `
    -Description "alojamiento: crear reserva" `
    -OldString @'
await UserPanelApiClient.CreateAccommodationReservationAsync(request);
'@ `
    -NewString @'
await UserPanelApiClient.CreateAccommodationReservationAsync(request, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "alojamiento: actualizar reserva" `
    -OldString @'
await UserPanelApiClient.UpdateAccommodationReservationAsync(ExistingReservation.Id, request);
'@ `
    -NewString @'
await UserPanelApiClient.UpdateAccommodationReservationAsync(ExistingReservation.Id, request, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "alojamiento: cancelar reserva" `
    -OldString @'
await UserPanelApiClient.DeleteAccommodationReservationAsync(ExistingReservation.Id);
'@ `
    -NewString @'
await UserPanelApiClient.DeleteAccommodationReservationAsync(ExistingReservation.Id, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "factura: crear" `
    -OldString @'
Dashboard = await UserPanelApiClient.CreateInvoiceAsync(request);
'@ `
    -NewString @'
Dashboard = await UserPanelApiClient.CreateInvoiceAsync(request, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "perfil: actualizar" `
    -OldString @'
UserPanelDashboardDto? updatedDashboard = await UserPanelApiClient.UpdateProfileAsync(request);
'@ `
    -NewString @'
UserPanelDashboardDto? updatedDashboard = await UserPanelApiClient.UpdateProfileAsync(request, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "competicion: actualizar entrada" `
    -OldString @'
await UserPanelApiClient.UpdateCompetitionEntryAsync(EditingCompetitionEntryId.Value, request);
'@ `
    -NewString @'
await UserPanelApiClient.UpdateCompetitionEntryAsync(EditingCompetitionEntryId.Value, request, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "competicion: crear entrada" `
    -OldString @'
await UserPanelApiClient.CreateCompetitionEntryAsync(createRequest);
'@ `
    -NewString @'
await UserPanelApiClient.CreateCompetitionEntryAsync(createRequest, CurrentDomain);
'@
Patch-File `
    -Path $RazorPath `
    -Description "competicion: eliminar entrada" `
    -OldString @'
await UserPanelApiClient.DeleteCompetitionEntryAsync(DeletingCompetitionEntryId.Value);
'@ `
    -NewString @'
await UserPanelApiClient.DeleteCompetitionEntryAsync(DeletingCompetitionEntryId.Value, CurrentDomain);
'@

Write-Host ""
Write-Host "Deberias ver 23 lineas 'OK: aplicado'." -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO:" -ForegroundColor Cyan
Write-Host "  1. dotnet build (Application, Api y Admin, deben compilar limpio)." -ForegroundColor Cyan
Write-Host "  2. Redeploy de AMBOS App Services." -ForegroundColor Cyan
Write-Host "  3. Verificacion: con un usuario registrado en mas de un festival, entrar" -ForegroundColor Cyan
Write-Host "     al panel de Swim Out y comprobar que aparecen los modulos de" -ForegroundColor Cyan
Write-Host "     comida/bus/alojamiento si estan activados alli." -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANTE: UpdateProfileAsync y CreateInvoiceAsync tenian este mismo fallo" -ForegroundColor Cyan
Write-Host "desde antes de esta serie de scripts (no lo introduje yo); tambien quedan" -ForegroundColor Cyan
Write-Host "corregidos aqui." -ForegroundColor Cyan