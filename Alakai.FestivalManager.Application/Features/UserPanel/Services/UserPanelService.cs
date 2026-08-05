using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;
using Alakai.FestivalManager.Application.Features.Invoices.Services;

namespace Alakai.FestivalManager.Application.Features.UserPanel.Services;

public class UserPanelService : IUserPanelService
{
    private readonly IUserPanelRepository _userPanelRepository;
    private readonly ICompetitionEntryService _competitionEntryService;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionCapacityRepository _competitionCapacityRepository;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IInvoiceService _invoiceService;
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

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> GetDashboardAsync(Guid userId, string? domain, CancellationToken cancellationToken = default)
    {
        User? user = await _userPanelRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "User panel dashboard could not be loaded.",
                Data = null,
                Errors = ["User not found."]
            };
        }

        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, domain, cancellationToken);

        if(registration is null)
        {
            return new ApiResponse<GetUserPanelDashboardResponse>
            {
                Success = false,
                Message = "User panel dashboard could not be loaded.",
                Data = null,
                Errors = ["Registration not found."]
            };
        }

        IReadOnlyList<Guid> registrationIds = [registration.Id];

        IReadOnlyList<CompetitionEntry> competitionEntries = registrationIds.Count == 0
            ? []
            : await _userPanelRepository.GetCompetitionEntriesByRegistrationIdsAsync(registrationIds, cancellationToken);

        IReadOnlyList<Invoice> invoices = registrationIds.Count == 0
            ? []
            : await _userPanelRepository.GetInvoicesByRegistrationIdsAsync(registrationIds, cancellationToken);

        IReadOnlyList<Competition> availableCompetitions = await _competitionRepository.GetByEditionIdAsync(registration.EditionId, cancellationToken);

        IReadOnlyList<Guid> competitionIds = availableCompetitions.Select(c => c.Id).ToList();

        IReadOnlyList<CompetitionCapacity> competitionCapacities = await _competitionCapacityRepository.GetByCompetitionIdsAsync(competitionIds, cancellationToken);

        UserPanelDashboardDto dashboard = new()
        {
            User = new UserPanelUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Country = user.Country,
                City = user.City
            },
            Registration = registration is null ? null : new UserPanelRegistrationDto
            {
                Id = registration.Id,
                EditionName = registration.Edition?.Name,
                FaviconUrl = registration.Edition?.Festival?.FaviconUrl,
                RegistrationStatus = registration.Status.ToString(),
                PaymentStatus = registration.PaymentStatus.ToString(),
                PassTypeName = registration.PassType.Name,
                LevelName = registration.Level?.Name,
                DanceRole = registration.DanceRole?.ToString(),
                PartnerEmail = registration.PartnerEmail,
                DiscountCodeValue = registration.DiscountCodeValue,
                FinalPrice = registration.FinalPrice,
                DocumentNumber = registration.DocumentNumber,
                DocumentCountry = registration.DocumentCountry,
                PaymentPlan = registration.PaymentPlan.ToString(),
                AmountPaid = registration.AmountPaid,
                PaymentDueAt = registration.PaymentDueAt
            },
            Competitions = competitionEntries.Select(c => new CompetitionEntryDto
            {
                Id = c.Id,
                CompetitionId = c.CompetitionId,
                RegistrationId = c.RegistrationId,
                PartnerRegistrationId = c.PartnerRegistrationId,
                CompetitionCapacityId = c.CompetitionCapacityId,
                DanceRole = c.DanceRole,
                Status = c.Status,
                Notes = c.Notes,
                InternalNotes = c.InternalNotes,
                IsActive = c.IsActive
            }).ToList(),
            AvailableCompetitions = availableCompetitions.Select(c => new CompetitionDto
            {
                Id = c.Id,
                Name = c.Name,
                EditionId = c.EditionId,
                Description = c.Description,
                Format = c.Format,
                Levels = c.Levels
                                .Where(l => l.IsActive)
                                .OrderBy(l => l.SortOrder)
                                .Select(l => new CompetitionLevelDto
                                {
                                    Id = l.Id,
                                    Name = l.Name,
                                    SortOrder = l.SortOrder,
                                    IsActive = l.IsActive
                                }).ToList(),
                Capacities = c.Capacities.Select(capacity => new CompetitionCapacityDto
                {
                    Id = capacity.Id,
                    CompetitionId = capacity.CompetitionId,
                    CompetitionLevelId = capacity.CompetitionLevelId,
                    DanceRole = capacity.DanceRole,
                    Capacity = capacity.Capacity,
                    SortOrder = capacity.SortOrder,
                    IsActive = capacity.IsActive
                }).ToList(),
                RequiresPartner = c.RequiresPartner,
                RequiresRole = c.RequiresRole,
                Price = c.Price,
                RegistrationOpenAt = c.RegistrationOpenAt,
                RegistrationCloseAt = c.RegistrationCloseAt,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            Invoices = invoices.Select(i => new UserPanelInvoiceDto
            {
                Id = i.Id,
                Number = i.Number,
                Date = i.IssuedAt,
                Amount = i.Amount,
                PdfUrl = i.PdfUrl
            }).ToList()
        };

        return new ApiResponse<GetUserPanelDashboardResponse>
        {
            Success = true,
            Message = "User panel dashboard loaded successfully.",
            Data = new GetUserPanelDashboardResponse
            {
                Dashboard = dashboard
            },
            Errors = []
        };
    }

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