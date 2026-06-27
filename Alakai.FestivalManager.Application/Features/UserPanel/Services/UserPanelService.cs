namespace Alakai.FestivalManager.Application.Features.UserPanel.Services;

public class UserPanelService : IUserPanelService
{
    private readonly IUserPanelRepository _userPanelRepository;
    private readonly ICompetitionEntryService _competitionEntryService;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionCapacityRepository _competitionCapacityRepository;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IMapper _mapper;
    public UserPanelService(IUserPanelRepository userPanelRepository, ICompetitionEntryService competitionEntryService, IMapper mapper, 
        ICompetitionEntryRepository competitionEntryRepository, ICompetitionRepository competitionRepository, 
        ICompetitionCapacityRepository competitionCapacityRepository, IEmailNotificationService emailNotificationService)
    {
        _userPanelRepository = userPanelRepository;
        _competitionEntryService = competitionEntryService;
        _mapper = mapper;
        _competitionEntryRepository = competitionEntryRepository;
        _competitionRepository = competitionRepository;
        _competitionCapacityRepository = competitionCapacityRepository;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<ApiResponse<GetUserPanelDashboardResponse>> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
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

        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, cancellationToken);

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

        IReadOnlyList<Registration> registrations = await _userPanelRepository.GetRegistrationsByUserIdAsync(userId, cancellationToken);

        IReadOnlyList<Guid> registrationIds = registrations
            .Select(r => r.Id)
            .ToList();

        IReadOnlyList<CompetitionEntry> competitionEntries = registrationIds.Count == 0
            ? []
            : await _userPanelRepository.GetCompetitionEntriesByRegistrationIdsAsync(registrationIds, cancellationToken);

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
                RegistrationStatus = registration.Status.ToString(),
                PaymentStatus = registration.PaymentStatus.ToString(),
                PassTypeName = registration.PassType.Name,
                LevelName = registration.Level?.Name,
                DanceRole = registration.DanceRole?.ToString(),
                PartnerEmail = registration.PartnerEmail,
                DiscountCodeValue = registration.DiscountCodeValue,
                FinalPrice = registration.FinalPrice,
                DocumentNumber = registration.DocumentNumber,
                DocumentCountry = registration.DocumentCountry
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
                Capacities = c.Capacities.Select(capacity => new CompetitionCapacityDto
                {
                    Id = capacity.Id,
                    CompetitionId = capacity.CompetitionId,
                    MixAndMatchLevel = capacity.MixAndMatchLevel,
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
            Invoices = []
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
        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, cancellationToken);

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

        return await GetDashboardAsync(userId, cancellationToken);
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

        return await GetDashboardAsync(userId, cancellationToken);
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

        return await GetDashboardAsync(userId, cancellationToken);
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

        Registration? registration = await _userPanelRepository.GetLatestRegistrationByUserIdAsync(userId, cancellationToken);

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

        return await GetDashboardAsync(userId, cancellationToken);
    }
}