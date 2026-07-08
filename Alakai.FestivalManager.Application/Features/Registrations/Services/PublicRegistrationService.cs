namespace Alakai.FestivalManager.Application.Features.Registrations.Services;

public class PublicRegistrationService : IPublicRegistrationService
{
    private readonly IEditionRepository _editionRepository;
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IDiscountCalculationService _discountCalculationService;
    private readonly IAccommodationBuildingRepository _accommodationBuildingRepository;

    public PublicRegistrationService(IEditionRepository editionRepository, IPassTypeRepository passTypeRepository,
        IRegistrationRepository registrationRepository, IDiscountCalculationService discountCalculationService,
        IAccommodationBuildingRepository accommodationBuildingRepository)
    {
        _editionRepository = editionRepository;
        _passTypeRepository = passTypeRepository;
        _registrationRepository = registrationRepository;
        _discountCalculationService = discountCalculationService;
        _accommodationBuildingRepository = accommodationBuildingRepository;
    }

    public async Task<PublicRegistrationAvailabilityDto> GetAvailabilityAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(editionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{editionId}' was not found.");
        }

        IReadOnlyList<PassType> passTypes = await _passTypeRepository.GetActiveByEditionIdWithLevelsAsync(editionId, cancellationToken);

        IReadOnlyList<AccommodationBuilding> buildings = await _accommodationBuildingRepository.GetByEditionIdAsync(editionId, cancellationToken);

        Dictionary<Guid, List<string>> buildingsByPassType = [];

        foreach (AccommodationBuilding building in buildings.Where(b => b.IsActive).OrderBy(b => b.SortOrder).ThenBy(b => b.Name))
        {
            foreach (AccommodationBuildingPassType link in building.AllowedPassTypes)
            {
                if (!buildingsByPassType.TryGetValue(link.PassTypeId, out List<string>? names))
                {
                    names = [];
                    buildingsByPassType[link.PassTypeId] = names;
                }

                names.Add(building.Name);
            }
        }

        PublicRegistrationAvailabilityDto availability = new()
        {
            EditionId = edition.Id,
            EditionName = edition.Name
        };

        foreach (PassType passType in passTypes)
        {
            PublicPassTypeDto passDto = new()
            {
                Id = passType.Id,
                Name = passType.Name,
                Description = passType.Description,
                AllowsMultipleLevels = passType.AllowsMultipleLevels,
                AllLevelsDiscountPercent = passType.AllLevelsDiscountPercent,
                BuildingName = buildingsByPassType.TryGetValue(passType.Id, out List<string>? passBuildings) && passBuildings.Count > 0 ? string.Join(" / ", passBuildings) : null
            };

            foreach (Level level in passType.Levels.Where(l => l.IsActive).OrderBy(l => l.SortOrder).ThenBy(l => l.Name))
            {
                bool requiresRole = level.LeaderCapacity.HasValue || level.FollowerCapacity.HasValue || level.MaxLeaderDifference.HasValue || level.MaxFollowerDifference.HasValue;

                int leaders = 0;
                int followers = 0;
                bool leaderFull = false;
                bool followerFull = false;
                bool leaderSoloAvailable = true;
                bool followerSoloAvailable = true;
                bool isFull;

                if (requiresRole)
                {
                    leaders = await _registrationRepository.CountActiveByLevelAsync(level.Id, DanceRole.Leader, cancellationToken);
                    followers = await _registrationRepository.CountActiveByLevelAsync(level.Id, DanceRole.Follower, cancellationToken);

                    leaderFull = level.LeaderCapacity.HasValue && leaders >= level.LeaderCapacity.Value;
                    followerFull = level.FollowerCapacity.HasValue && followers >= level.FollowerCapacity.Value;

                    leaderSoloAvailable = !leaderFull && (!level.MaxLeaderDifference.HasValue || (leaders + 1) - followers <= level.MaxLeaderDifference.Value);
                    followerSoloAvailable = !followerFull && (!level.MaxFollowerDifference.HasValue || (followers + 1) - leaders <= level.MaxFollowerDifference.Value);

                    isFull = leaderFull && followerFull;
                }
                else
                {
                    int total = await _registrationRepository.CountActiveByLevelAsync(level.Id, null, cancellationToken);
                    isFull = level.IndividualCapacity.HasValue && total >= level.IndividualCapacity.Value;
                }

                passDto.Levels.Add(new PublicLevelDto
                {
                    Id = level.Id,
                    Name = level.Name,
                    Description = level.Description,
                    Price = level.RegularPrice,
                    RequiresRole = requiresRole,
                    IsFull = isFull,
                    LeaderFull = leaderFull,
                    FollowerFull = followerFull,
                    LeaderSoloAvailable = leaderSoloAvailable,
                    FollowerSoloAvailable = followerSoloAvailable
                });
            }

            // A pass type with no active levels has no price/capacity source (levels
            // hold both) and would leave the public form stuck on this step forever.
            // Skip it here instead - add at least one level to this pass type in
            // Admin > Levels to make it selectable on the public form.
            if (passDto.Levels.Count == 0)
            {
                continue;
            }

            availability.PassTypes.Add(passDto);
        }

        return availability;
    }

    public async Task<PublicDiscountCheckDto> CheckDiscountCodeAsync(Guid editionId, string code, decimal basePrice, CancellationToken cancellationToken = default)
    {
        DiscountCalculationResult result = await _discountCalculationService.CalculateAsync(editionId, basePrice, code, cancellationToken);

        return new PublicDiscountCheckDto
        {
            Exists = result.DiscountCodeId.HasValue,
            Applied = result.DiscountStatus == DiscountApplicationStatus.Applied,
            PendingThreshold = result.DiscountStatus == DiscountApplicationStatus.PendingThreshold,
            DiscountAmount = result.DiscountAmount,
            FinalPrice = result.FinalPrice
        };
    }

    public async Task<PublicPartnerLookupDto> LookupPartnerAsync(Guid editionId, string email, CancellationToken cancellationToken = default)
    {
        bool exists = !string.IsNullOrWhiteSpace(email)
            && await _registrationRepository.ExistsByEditionAndEmailAsync(editionId, email.Trim(), cancellationToken);

        return new PublicPartnerLookupDto { Exists = exists };
    }
}
