using Alakai.FestivalManager.Application.Features.UserPanel.Contracts.Requests;

namespace Alakai.FestivalManager.Application.Features.UserPanel.Services;

public class UserPanelService : IUserPanelService
{
    private readonly IUserPanelRepository _userPanelRepository;

    public UserPanelService(IUserPanelRepository userPanelRepository)
    {
        _userPanelRepository = userPanelRepository;
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

        IReadOnlyList<Registration> registrations = await _userPanelRepository.GetRegistrationsByUserIdAsync(userId, cancellationToken);

        IReadOnlyList<Guid> registrationIds = registrations
            .Select(r => r.Id)
            .ToList();

        IReadOnlyList<CompetitionEntry> competitionEntries = registrationIds.Count == 0
            ? []
            : await _userPanelRepository.GetCompetitionEntriesByRegistrationIdsAsync(registrationIds, cancellationToken);

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
            Competitions = competitionEntries.Select(c => new UserPanelCompetitionDto
            {
                Id = c.Id,
                CompetitionName = c.Competition.Name,
                Role = c.DanceRole?.ToString(),
                Status = c.Status.ToString()
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