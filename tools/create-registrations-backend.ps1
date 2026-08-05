<#
.SINOPSIS
  Segundo y ultimo fix pendiente de "comida, autobuses y alojamiento": este
  cubre AUTOBUSES y ALOJAMIENTO (el de comida y modulos del festival era el
  script 28, ya aplicado).

  DIAGNOSTICO (mismo patron ya visto tres veces):
    - BusApiClient y AccommodationApiClient (usados por el panel de
      participante) SIEMPRE mandan el token de Admin (IAdminTokenProvider)
      contra "api/buses", "api/bus-reservations", "api/accommodation-buildings"
      y "api/accommodation-reservations", todos [Authorize(Roles=
      "SuperAdmin,Admin")]. Un participante real recibe 401 siempre que
      intenta reservar autobus o alojamiento, ver los disponibles, editar su
      reserva o cancelarla.

  FIX: mismo patron que competiciones y comida - nuevos endpoints en
  UserPanelController que resuelven el registro del usuario autenticado en
  el SERVIDOR (nunca confiando en lo que mande el cliente) y llaman a los
  servicios que YA EXISTEN y funcionan (IBusReservationService, IBusService,
  IAccommodationReservationService, IAccommodationBuildingService) - toda la
  logica de negocio (capacidad, tipos de pase permitidos, "ya tienes una
  reserva para esta direccion", propiedad de la reserva vía isAdmin /
  RequestingRegistrationId) ya existe y esta probada; solo hay que cablearlo
  de forma segura, exactamente igual que se hizo con competiciones y comida.

  De paso corrige el mismo problema de mensajes de error sin traducir
  (ex.Message mostrado directamente) en los 7 catch de autobuses/alojamiento
  del panel, con las traducciones anadidas a los 4 idiomas - igual que se
  hizo en los scripts 27 y 28.

  Archivos que modifica:
    - Application\Features\UserPanel\Services\IUserPanelService.cs   (+10 firmas)
    - Application\Features\UserPanel\Services\UserPanelService.cs    (+4 dependencias, +10 metodos)
    - Api\Controllers\UserPanelController.cs                         (+11 endpoints)
    - Admin\Services\Api\UserPanelApiClient.cs                       (+11 metodos)
    - Admin\Components\Pages\UserPanelDashboard\UserPanel.razor      (usa UserPanelApiClient,
                                                                        quita los 2 @inject que se quedan sin uso,
                                                                        7 catch usan T.Get() en vez de ex.Message)
    - wwwroot\i18n\en.json / es.json / fr.json / ca.json             (+7 claves nuevas x 4 idiomas)

  Idempotente.

.USO
  Ejecutar desde la raiz del repo:
    .\29-userpanel-bus-accommodation.ps1

  Luego: dotnet build (Api y Admin, deben compilar limpio), redeploy de
  AMBOS App Services.

  Verificacion: como PARTICIPANTE, reserva un autobus, edita esa reserva,
  cancelala; reserva un alojamiento, edita la reserva, cancelala. Antes daba
  401 en todo. Los mensajes de error deben salir traducidos, no en ingles
  en crudo.
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
$EnPath         = ".\Alakai.FestivalManager.Admin\wwwroot\i18n\en.json"
$EsPath         = ".\Alakai.FestivalManager.Admin\wwwroot\i18n\es.json"
$FrPath         = ".\Alakai.FestivalManager.Admin\wwwroot\i18n\fr.json"
$CaPath         = ".\Alakai.FestivalManager.Admin\wwwroot\i18n\ca.json"

# ----------------------------------------------------------------------------
# 1. IUserPanelService: +10 firmas
# ----------------------------------------------------------------------------
Patch-File `
    -Path $IServicePath `
    -Description "+10 firmas (autobuses, alojamiento)" `
    -OldString @'
    Task<ApiResponse<RegistrationFestivalInfoDto>> GetFestivalModulesAsync(Guid userId, CancellationToken cancellationToken = default);
}
'@ `
    -NewString @'
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
}
'@

# ----------------------------------------------------------------------------
# 2. UserPanelService: +4 dependencias en el constructor
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ServicePath `
    -Description "+IBusReservationService/IBusService/IAccommodationReservationService/IAccommodationBuildingService" `
    -OldString @'
    private readonly IMealPreferenceService _mealPreferenceService;
    private readonly IRegistrationFestivalInfoService _registrationFestivalInfoService;
    private readonly IMapper _mapper;
    public UserPanelService(IUserPanelRepository userPanelRepository, ICompetitionEntryService competitionEntryService, IMapper mapper,
        ICompetitionEntryRepository competitionEntryRepository, ICompetitionRepository competitionRepository,
        ICompetitionCapacityRepository competitionCapacityRepository, IEmailNotificationService emailNotificationService,
        IInvoiceService invoiceService, IMealPreferenceService mealPreferenceService, IRegistrationFestivalInfoService registrationFestivalInfoService)
    {
        _userPanelRepository = userPanelRepository;
        _competitionEntryService = competitionEntryService;
        _mapper = mapper;
        _competitionEntryRepository = competitionEntryRepository;
        _competitionRepository = competitionRepository;
        _competitionCapacityRepository = competitionCapacityRepository;
        _emailNotificationService = emailNotificationService;
        _invoiceService = invoiceService;
        _mealPreferenceService = mealPreferenceService;
        _registrationFestivalInfoService = registrationFestivalInfoService;
    }
'@ `
    -NewString @'
    private readonly IMealPreferenceService _mealPreferenceService;
    private readonly IRegistrationFestivalInfoService _registrationFestivalInfoService;
    private readonly IBusReservationService _busReservationService;
    private readonly IBusService _busService;
    private readonly IAccommodationReservationService _accommodationReservationService;
    private readonly IAccommodationBuildingService _accommodationBuildingService;
    private readonly IMapper _mapper;
    public UserPanelService(IUserPanelRepository userPanelRepository, ICompetitionEntryService competitionEntryService, IMapper mapper,
        ICompetitionEntryRepository competitionEntryRepository, ICompetitionRepository competitionRepository,
        ICompetitionCapacityRepository competitionCapacityRepository, IEmailNotificationService emailNotificationService,
        IInvoiceService invoiceService, IMealPreferenceService mealPreferenceService, IRegistrationFestivalInfoService registrationFestivalInfoService,
        IBusReservationService busReservationService, IBusService busService, IAccommodationReservationService accommodationReservationService, IAccommodationBuildingService accommodationBuildingService)
    {
        _userPanelRepository = userPanelRepository;
        _competitionEntryService = competitionEntryService;
        _mapper = mapper;
        _competitionEntryRepository = competitionEntryRepository;
        _competitionRepository = competitionRepository;
        _competitionCapacityRepository = competitionCapacityRepository;
        _emailNotificationService = emailNotificationService;
        _invoiceService = invoiceService;
        _mealPreferenceService = mealPreferenceService;
        _registrationFestivalInfoService = registrationFestivalInfoService;
        _busReservationService = busReservationService;
        _busService = busService;
        _accommodationReservationService = accommodationReservationService;
        _accommodationBuildingService = accommodationBuildingService;
    }
'@

# ----------------------------------------------------------------------------
# 3. UserPanelService: +10 metodos nuevos, al final de la clase.
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ServicePath `
    -Description "+10 metodos (autobuses, alojamiento)" `
    -OldString @'
        return await _registrationFestivalInfoService.GetForRegistrationAsync(registration.Id, cancellationToken);
    }
}
'@ `
    -NewString @'
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
'@

# ----------------------------------------------------------------------------
# 4. UserPanelController: +11 endpoints
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ControllerPath `
    -Description "+11 endpoints (autobuses, alojamiento)" `
    -OldString @'
        ApiResponse<RegistrationFestivalInfoDto> response = await _userPanelService.GetFestivalModulesAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
'@ `
    -NewString @'
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
}
'@

# ----------------------------------------------------------------------------
# 5. UserPanelApiClient: +11 metodos
# ----------------------------------------------------------------------------
Patch-File `
    -Path $ApiClientPath `
    -Description "+11 metodos (autobuses, alojamiento)" `
    -OldString @'
        ApiResponse<RegistrationFestivalInfoDto>? response = await _httpClient.GetFromJsonAsync<ApiResponse<RegistrationFestivalInfoDto>>("api/user-panel/festival-modules", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            return 0;
        }

        return response.Data.EnabledModules;
    }
}
'@ `
    -NewString @'
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
'@

# ----------------------------------------------------------------------------
# 6-12. UserPanel.razor: los 7 catch de autobuses/alojamiento usan T.Get() en
#       vez de ex.Message (mismo problema que ya se corrigio en competiciones
#       y comida). Se hacen ANTES de los cambios de punto 13+ para no
#       depender de ese orden.
# ----------------------------------------------------------------------------
Patch-File `
    -Path $RazorPath `
    -Description "catch de reservar autobus usa T.Get()" `
    -OldString @'
            ShowBusSuccess(T.Get("up_bus_reserved"));
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }
'@ `
    -NewString @'
            ShowBusSuccess(T.Get("up_bus_reserved"));
            await LoadBusDataAsync();
        }
        catch
        {
            ShowBusError(T.Get("up_bus_reservation_error"));
        }
        finally
        {
            IsSavingBus = false;
        }
    }
'@

Patch-File `
    -Path $RazorPath `
    -Description "catch de editar autobus usa T.Get()" `
    -OldString @'
            ShowBusSuccess(T.Get("up_bus_updated"));
            editingBusReservation = null;
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }
'@ `
    -NewString @'
            ShowBusSuccess(T.Get("up_bus_updated"));
            editingBusReservation = null;
            await LoadBusDataAsync();
        }
        catch
        {
            ShowBusError(T.Get("up_bus_update_error"));
        }
        finally
        {
            IsSavingBus = false;
        }
    }
'@

Patch-File `
    -Path $RazorPath `
    -Description "catch de cancelar autobus usa T.Get()" `
    -OldString @'
            ShowBusSuccess(T.Get("up_bus_cancelled"));
            cancellingBusReservation = null;
            await LoadBusDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowBusError(ex.Message);
        }
        finally
        {
            IsSavingBus = false;
        }
    }
'@ `
    -NewString @'
            ShowBusSuccess(T.Get("up_bus_cancelled"));
            cancellingBusReservation = null;
            await LoadBusDataAsync();
        }
        catch
        {
            ShowBusError(T.Get("up_bus_cancel_error"));
        }
        finally
        {
            IsSavingBus = false;
        }
    }
'@

Patch-File `
    -Path $RazorPath `
    -Description "catch de cargar detalle de alojamiento usa T.Get()" `
    -OldString @'
        catch (Exception ex)
        {
            ShowAccommodationError(ex.Message);
        }

        if (SelectedBuildingType == 1)
'@ `
    -NewString @'
        catch
        {
            ShowAccommodationError(T.Get("up_accommodation_load_error"));
        }

        if (SelectedBuildingType == 1)
'@

Patch-File `
    -Path $RazorPath `
    -Description "catch de reservar alojamiento usa T.Get()" `
    -OldString @'
            ShowAccommodationSuccess(T.Get("up_accommodation_reserved"));
            SelectedAccommodationBuildingId = Guid.Empty;
            SelectedBuildingDetail = null;
            OccupantRows = [];
            await LoadAccommodationDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowAccommodationError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowAccommodationError(ex.Message);
        }
        finally
        {
            IsSavingAccommodation = false;
        }
    }
'@ `
    -NewString @'
            ShowAccommodationSuccess(T.Get("up_accommodation_reserved"));
            SelectedAccommodationBuildingId = Guid.Empty;
            SelectedBuildingDetail = null;
            OccupantRows = [];
            await LoadAccommodationDataAsync();
        }
        catch
        {
            ShowAccommodationError(T.Get("up_accommodation_reservation_error"));
        }
        finally
        {
            IsSavingAccommodation = false;
        }
    }
'@

Patch-File `
    -Path $RazorPath `
    -Description "catch de editar alojamiento usa T.Get()" `
    -OldString @'
            ShowAccommodationSuccess(T.Get("up_accommodation_updated"));
            ShowEditAccommodationModal = false;
            await LoadAccommodationDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowAccommodationError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowAccommodationError(ex.Message);
        }
        finally
        {
            IsSavingAccommodation = false;
        }
    }
'@ `
    -NewString @'
            ShowAccommodationSuccess(T.Get("up_accommodation_updated"));
            ShowEditAccommodationModal = false;
            await LoadAccommodationDataAsync();
        }
        catch
        {
            ShowAccommodationError(T.Get("up_accommodation_update_error"));
        }
        finally
        {
            IsSavingAccommodation = false;
        }
    }
'@

Patch-File `
    -Path $RazorPath `
    -Description "catch de cancelar alojamiento usa T.Get()" `
    -OldString @'
            ShowAccommodationSuccess(T.Get("up_reservation_cancelled"));
            ExistingReservation = null;
            ShowCancelAccommodationModal = false;
            await LoadAccommodationDataAsync();
        }
        catch (ApiClientException ex)
        {
            ShowAccommodationError(ex.Message);
        }
        finally
        {
            IsSavingAccommodation = false;
        }
    }
'@ `
    -NewString @'
            ShowAccommodationSuccess(T.Get("up_reservation_cancelled"));
            ExistingReservation = null;
            ShowCancelAccommodationModal = false;
            await LoadAccommodationDataAsync();
        }
        catch
        {
            ShowAccommodationError(T.Get("up_accommodation_cancel_error"));
        }
        finally
        {
            IsSavingAccommodation = false;
        }
    }
'@

# ----------------------------------------------------------------------------
# 13-24. UserPanel.razor: 12 cambios de sitio de llamada, BusApiClient /
#        AccommodationApiClient -> UserPanelApiClient.
# ----------------------------------------------------------------------------
Patch-File `
    -Path $RazorPath `
    -Description "cargar reservas de autobus usa UserPanelApiClient" `
    -OldString @'
            BusReservations = (await BusApiClient.GetReservationsByRegistrationAsync(Dashboard.Registration.Id)).ToList();
'@ `
    -NewString @'
            BusReservations = (await UserPanelApiClient.GetBusReservationsAsync()).ToList();
'@

Patch-File `
    -Path $RazorPath `
    -Description "cargar autobuses disponibles usa UserPanelApiClient" `
    -OldString @'
            AvailableBuses = (await BusApiClient.GetAvailableForRegistrationAsync(Dashboard.Registration.Id)).ToList();
'@ `
    -NewString @'
            AvailableBuses = (await UserPanelApiClient.GetAvailableBusesAsync()).ToList();
'@

Patch-File `
    -Path $RazorPath `
    -Description "reservar autobus usa UserPanelApiClient" `
    -OldString @'
            await BusApiClient.CreateReservationsAsync(request);
'@ `
    -NewString @'
            await UserPanelApiClient.CreateBusReservationsAsync(request);
'@

Patch-File `
    -Path $RazorPath `
    -Description "editar reserva de autobus usa UserPanelApiClient" `
    -OldString @'
            await BusApiClient.UpdateReservationAsync(editingBusReservation.Id, request, isAdmin: false);
'@ `
    -NewString @'
            await UserPanelApiClient.UpdateBusReservationAsync(editingBusReservation.Id, request);
'@

Patch-File `
    -Path $RazorPath `
    -Description "cancelar reserva de autobus usa UserPanelApiClient" `
    -OldString @'
            await BusApiClient.DeleteReservationAsync(cancellingBusReservation.Id, Dashboard.Registration.Id, isAdmin: false);
'@ `
    -NewString @'
            await UserPanelApiClient.DeleteBusReservationAsync(cancellingBusReservation.Id);
'@

Patch-File `
    -Path $RazorPath `
    -Description "cargar reserva de alojamiento usa UserPanelApiClient" `
    -OldString @'
            ExistingReservation = await AccommodationApiClient.GetReservationByRegistrationAsync(Dashboard.Registration.Id);
'@ `
    -NewString @'
            ExistingReservation = await UserPanelApiClient.GetAccommodationReservationAsync();
'@

Patch-File `
    -Path $RazorPath `
    -Description "cargar alojamientos disponibles usa UserPanelApiClient" `
    -OldString @'
                AvailableAccommodationBuildings = (await AccommodationApiClient.GetAvailableForRegistrationAsync(Dashboard.Registration.Id)).ToList();
'@ `
    -NewString @'
                AvailableAccommodationBuildings = (await UserPanelApiClient.GetAvailableAccommodationsAsync()).ToList();
'@

Patch-File `
    -Path $RazorPath `
    -Description "cargar detalle de edificio (reserva existente) usa UserPanelApiClient" `
    -OldString @'
                SelectedBuildingDetail = await AccommodationApiClient.GetBuildingByIdAsync(ExistingReservation.AccommodationBuildingId);
'@ `
    -NewString @'
                SelectedBuildingDetail = await UserPanelApiClient.GetAccommodationBuildingAsync(ExistingReservation.AccommodationBuildingId);
'@

Patch-File `
    -Path $RazorPath `
    -Description "cargar detalle de edificio (seleccion) usa UserPanelApiClient" `
    -OldString @'
            SelectedBuildingDetail = await AccommodationApiClient.GetBuildingByIdAsync(SelectedAccommodationBuildingId);
'@ `
    -NewString @'
            SelectedBuildingDetail = await UserPanelApiClient.GetAccommodationBuildingAsync(SelectedAccommodationBuildingId);
'@

Patch-File `
    -Path $RazorPath `
    -Description "reservar alojamiento usa UserPanelApiClient" `
    -OldString @'
            await AccommodationApiClient.CreateReservationAsync(request);
'@ `
    -NewString @'
            await UserPanelApiClient.CreateAccommodationReservationAsync(request);
'@

Patch-File `
    -Path $RazorPath `
    -Description "editar reserva de alojamiento usa UserPanelApiClient" `
    -OldString @'
            await AccommodationApiClient.UpdateReservationAsync(ExistingReservation.Id, request, isAdmin: false);
'@ `
    -NewString @'
            await UserPanelApiClient.UpdateAccommodationReservationAsync(ExistingReservation.Id, request);
'@

Patch-File `
    -Path $RazorPath `
    -Description "cancelar reserva de alojamiento usa UserPanelApiClient" `
    -OldString @'
            await AccommodationApiClient.DeleteReservationAsync(ExistingReservation.Id, Dashboard.Registration.Id, isAdmin: false);
'@ `
    -NewString @'
            await UserPanelApiClient.DeleteAccommodationReservationAsync(ExistingReservation.Id);
'@

# ----------------------------------------------------------------------------
# 25. UserPanel.razor: quita @inject AccommodationApiClient/BusApiClient
#     (ya sin uso tras los cambios anteriores).
# ----------------------------------------------------------------------------
Patch-File `
    -Path $RazorPath `
    -Description "quita @inject AccommodationApiClient/BusApiClient (ya sin uso)" `
    -OldString @'
@inject RegistrationApiClient RegistrationApiClient
@inject AccommodationApiClient AccommodationApiClient
@inject BusApiClient BusApiClient
@inject PaymentApiClient PaymentApiClient
'@ `
    -NewString @'
@inject RegistrationApiClient RegistrationApiClient
@inject PaymentApiClient PaymentApiClient
'@

# ----------------------------------------------------------------------------
# 26-33. Traducciones: 7 claves nuevas en los 4 idiomas, insertadas
#        alfabeticamente en el mismo bloque donde ya estan up_accommodation_*
#        y up_bus_*.
# ----------------------------------------------------------------------------
Patch-File `
    -Path $EnPath `
    -Description "+3 claves up_accommodation_* (en)" `
    -OldString @'
  "up_accommodation_reserved": "Accommodation reservation created successfully.",
  "up_accommodation_updated": "Accommodation reservation updated successfully.",
'@ `
    -NewString @'
  "up_accommodation_cancel_error": "Accommodation reservation could not be cancelled.",
  "up_accommodation_load_error": "Accommodation details could not be loaded.",
  "up_accommodation_reservation_error": "Accommodation reservation could not be created.",
  "up_accommodation_reserved": "Accommodation reservation created successfully.",
  "up_accommodation_update_error": "Accommodation reservation could not be updated.",
  "up_accommodation_updated": "Accommodation reservation updated successfully.",
'@

Patch-File `
    -Path $EnPath `
    -Description "+3 claves up_bus_* (en)" `
    -OldString @'
  "up_bus_cancelled": "Bus reservation cancelled successfully.",
  "up_bus_reserved": "Bus reservation created successfully.",
  "up_bus_updated": "Bus reservation updated successfully.",
'@ `
    -NewString @'
  "up_bus_cancel_error": "Bus reservation could not be cancelled.",
  "up_bus_cancelled": "Bus reservation cancelled successfully.",
  "up_bus_reservation_error": "Bus reservation could not be created.",
  "up_bus_reserved": "Bus reservation created successfully.",
  "up_bus_update_error": "Bus reservation could not be updated.",
  "up_bus_updated": "Bus reservation updated successfully.",
'@

Patch-File `
    -Path $EsPath `
    -Description "+3 claves up_accommodation_* (es)" `
    -OldString @'
  "up_accommodation_reserved": "Reserva de alojamiento creada correctamente.",
  "up_accommodation_updated": "Reserva de alojamiento actualizada correctamente.",
'@ `
    -NewString @'
  "up_accommodation_cancel_error": "No se pudo cancelar la reserva de alojamiento.",
  "up_accommodation_load_error": "No se pudieron cargar los datos del alojamiento.",
  "up_accommodation_reservation_error": "No se pudo crear la reserva de alojamiento.",
  "up_accommodation_reserved": "Reserva de alojamiento creada correctamente.",
  "up_accommodation_update_error": "No se pudo actualizar la reserva de alojamiento.",
  "up_accommodation_updated": "Reserva de alojamiento actualizada correctamente.",
'@

Patch-File `
    -Path $EsPath `
    -Description "+3 claves up_bus_* (es)" `
    -OldString @'
  "up_bus_cancelled": "Reserva de autobús cancelada correctamente.",
  "up_bus_reserved": "Reserva de autobús creada correctamente.",
  "up_bus_updated": "Reserva de autobús actualizada correctamente.",
'@ `
    -NewString @'
  "up_bus_cancel_error": "No se pudo cancelar la reserva de autobús.",
  "up_bus_cancelled": "Reserva de autobús cancelada correctamente.",
  "up_bus_reservation_error": "No se pudo crear la reserva de autobús.",
  "up_bus_reserved": "Reserva de autobús creada correctamente.",
  "up_bus_update_error": "No se pudo actualizar la reserva de autobús.",
  "up_bus_updated": "Reserva de autobús actualizada correctamente.",
'@

Patch-File `
    -Path $FrPath `
    -Description "+3 claves up_accommodation_* (fr)" `
    -OldString @'
  "up_accommodation_reserved": "Réservation d’hébergement créée avec succès.",
  "up_accommodation_updated": "Réservation d’hébergement mise à jour avec succès.",
'@ `
    -NewString @'
  "up_accommodation_cancel_error": "La réservation d’hébergement n’a pas pu être annulée.",
  "up_accommodation_load_error": "Les détails de l’hébergement n’ont pas pu être chargés.",
  "up_accommodation_reservation_error": "La réservation d’hébergement n’a pas pu être créée.",
  "up_accommodation_reserved": "Réservation d’hébergement créée avec succès.",
  "up_accommodation_update_error": "La réservation d’hébergement n’a pas pu être mise à jour.",
  "up_accommodation_updated": "Réservation d’hébergement mise à jour avec succès.",
'@

Patch-File `
    -Path $FrPath `
    -Description "+3 claves up_bus_* (fr)" `
    -OldString @'
  "up_bus_cancelled": "Réservation de bus annulée avec succès.",
  "up_bus_reserved": "Réservation de bus créée avec succès.",
  "up_bus_updated": "Réservation de bus mise à jour avec succès.",
'@ `
    -NewString @'
  "up_bus_cancel_error": "La réservation de bus n’a pas pu être annulée.",
  "up_bus_cancelled": "Réservation de bus annulée avec succès.",
  "up_bus_reservation_error": "La réservation de bus n’a pas pu être créée.",
  "up_bus_reserved": "Réservation de bus créée avec succès.",
  "up_bus_update_error": "La réservation de bus n’a pas pu être mise à jour.",
  "up_bus_updated": "Réservation de bus mise à jour avec succès.",
'@

Patch-File `
    -Path $CaPath `
    -Description "+3 claves up_accommodation_* (ca)" `
    -OldString @'
  "up_accommodation_reserved": "Reserva d’allotjament creada correctament.",
  "up_accommodation_updated": "Reserva d’allotjament actualitzada correctament.",
'@ `
    -NewString @'
  "up_accommodation_cancel_error": "No s’ha pogut cancel·lar la reserva d’allotjament.",
  "up_accommodation_load_error": "No s’han pogut carregar les dades de l’allotjament.",
  "up_accommodation_reservation_error": "No s’ha pogut crear la reserva d’allotjament.",
  "up_accommodation_reserved": "Reserva d’allotjament creada correctament.",
  "up_accommodation_update_error": "No s’ha pogut actualitzar la reserva d’allotjament.",
  "up_accommodation_updated": "Reserva d’allotjament actualitzada correctament.",
'@

Patch-File `
    -Path $CaPath `
    -Description "+3 claves up_bus_* (ca)" `
    -OldString @'
  "up_bus_cancelled": "Reserva d’autobús cancel·lada correctament.",
  "up_bus_reserved": "Reserva d’autobús creada correctament.",
  "up_bus_updated": "Reserva d’autobús actualitzada correctament.",
'@ `
    -NewString @'
  "up_bus_cancel_error": "No s’ha pogut cancel·lar la reserva d’autobús.",
  "up_bus_cancelled": "Reserva d’autobús cancel·lada correctament.",
  "up_bus_reservation_error": "No s’ha pogut crear la reserva d’autobús.",
  "up_bus_reserved": "Reserva d’autobús creada correctament.",
  "up_bus_update_error": "No s’ha pogut actualitzar la reserva d’autobús.",
  "up_bus_updated": "Reserva d’autobús actualitzada correctament.",
'@

Write-Host ""
Write-Host "Deberias ver 33 lineas 'OK: aplicado'." -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO:" -ForegroundColor Cyan
Write-Host "  1. dotnet build (Api y Admin, deben compilar limpio)." -ForegroundColor Cyan
Write-Host "  2. Redeploy de AMBOS App Services." -ForegroundColor Cyan
Write-Host "  3. Verificacion: como PARTICIPANTE, reserva/edita/cancela un autobus" -ForegroundColor Cyan
Write-Host "     y un alojamiento. Antes daba 401 en todo." -ForegroundColor Cyan
Write-Host ""
Write-Host "Con esto quedan cubiertos los 4 puntos de la auditoria de Controllers.zip:" -ForegroundColor Cyan
Write-Host "  competiciones (script 25/27), comida y modulos (script 28), autobuses y alojamiento (este)." -ForegroundColor Cyan