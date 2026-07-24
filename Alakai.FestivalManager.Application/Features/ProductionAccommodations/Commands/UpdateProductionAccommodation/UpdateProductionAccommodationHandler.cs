namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Commands.UpdateProductionAccommodation;

public class UpdateProductionAccommodationHandler
{
    private readonly IProductionAccommodationRepository _accommodationRepository;
    private readonly IProductionAccommodationZoneRepository _zoneRepository;
    private readonly IMapper _mapper;

    public UpdateProductionAccommodationHandler(IProductionAccommodationRepository accommodationRepository, IProductionAccommodationZoneRepository zoneRepository, IMapper mapper)
    {
        _accommodationRepository = accommodationRepository;
        _zoneRepository = zoneRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationDto> HandleAsync(UpdateProductionAccommodationCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodation? accommodation = await _accommodationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (accommodation is null)
        {
            throw new NotFoundException($"Production accommodation with id '{command.Id}' was not found.");
        }

        ProductionAccommodationZone? zone = await _zoneRepository.GetByIdAsync(command.ProductionAccommodationZoneId, cancellationToken);

        if (zone is null)
        {
            throw new NotFoundException($"Production accommodation zone with id '{command.ProductionAccommodationZoneId}' was not found.");
        }

        _mapper.Map(command, accommodation);
        accommodation.SetUpdated();

        await _accommodationRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductionAccommodationDto>(accommodation);
    }
}