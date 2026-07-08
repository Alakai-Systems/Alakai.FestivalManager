namespace Alakai.FestivalManager.Application.Features.Meals.Services;

public class MealPreferenceService : IMealPreferenceService
{
    private readonly IMealPreferenceRepository _mealPreferenceRepository;
    private readonly IRegistrationRepository _registrationRepository;

    public MealPreferenceService(IMealPreferenceRepository mealPreferenceRepository, IRegistrationRepository registrationRepository)
    {
        _mealPreferenceRepository = mealPreferenceRepository;
        _registrationRepository = registrationRepository;
    }

    public async Task<ApiResponse<GetMealPreferenceResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        MealPreference? preference = await _mealPreferenceRepository.GetByRegistrationIdAsync(registrationId, cancellationToken);

        return new ApiResponse<GetMealPreferenceResponse>
        {
            Success = true,
            Data = new GetMealPreferenceResponse { Preference = preference is null ? null : ToDto(preference, null) },
            Errors = [],
            Message = preference is null ? "No meal preference set yet." : "Meal preference found."
        };
    }

    public async Task<ApiResponse<GetMealPreferencesResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Registration> registrations = await _registrationRepository.GetByEditionIdAsync(editionId, cancellationToken);
        IReadOnlyList<MealPreference> preferences = await _mealPreferenceRepository.GetByEditionIdAsync(editionId, cancellationToken);

        Dictionary<Guid, MealPreference> preferenceByRegistration = preferences.ToDictionary(p => p.RegistrationId, p => p);

        List<MealPreferenceDto> dtos = registrations.Select(r =>
        {
            preferenceByRegistration.TryGetValue(r.Id, out MealPreference? preference);

            return new MealPreferenceDto
            {
                Id = preference?.Id,
                RegistrationId = r.Id,
                RegistrationName = $"{r.FirstName} {r.LastName}",
                RegistrationEmail = r.Email,
                HasPreference = preference is not null,
                MenuType = preference?.MenuType ?? MenuType.Standard,
                IsCeliacOrGlutenIntolerant = preference?.IsCeliacOrGlutenIntolerant ?? false,
                AllergiesNotes = preference?.AllergiesNotes
            };
        }).ToList();

        return new ApiResponse<GetMealPreferencesResponse>
        {
            Success = true,
            Data = new GetMealPreferencesResponse { Preferences = dtos },
            Errors = [],
            Message = $"There are {dtos.Count} registrations."
        };
    }

    public async Task<ApiResponse<SaveMealPreferenceResponse>> SaveAsync(SaveMealPreferenceCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{command.RegistrationId}' was not found.");
        }

        MealPreference? preference = await _mealPreferenceRepository.GetByRegistrationIdAsync(command.RegistrationId, cancellationToken);

        if (preference is null)
        {
            preference = new MealPreference
            {
                RegistrationId = command.RegistrationId,
                MenuType = command.MenuType,
                IsCeliacOrGlutenIntolerant = command.IsCeliacOrGlutenIntolerant,
                AllergiesNotes = command.AllergiesNotes
            };

            await _mealPreferenceRepository.AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.MenuType = command.MenuType;
            preference.IsCeliacOrGlutenIntolerant = command.IsCeliacOrGlutenIntolerant;
            preference.AllergiesNotes = command.AllergiesNotes;
        }

        await _mealPreferenceRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<SaveMealPreferenceResponse>
        {
            Success = true,
            Data = new SaveMealPreferenceResponse { Preference = ToDto(preference, registration) },
            Errors = [],
            Message = "Meal preference saved successfully."
        };
    }

    private static MealPreferenceDto ToDto(MealPreference preference, Registration? registration)
    {
        return new MealPreferenceDto
        {
            Id = preference.Id,
            RegistrationId = preference.RegistrationId,
            RegistrationName = registration is not null ? $"{registration.FirstName} {registration.LastName}" : null,
            RegistrationEmail = registration?.Email,
            HasPreference = true,
            MenuType = preference.MenuType,
            IsCeliacOrGlutenIntolerant = preference.IsCeliacOrGlutenIntolerant,
            AllergiesNotes = preference.AllergiesNotes
        };
    }
}