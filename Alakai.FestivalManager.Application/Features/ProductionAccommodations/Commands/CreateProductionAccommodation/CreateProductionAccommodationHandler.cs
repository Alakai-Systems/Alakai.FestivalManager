namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Commands.CreateProductionAccommodation;

public class CreateProductionAccommodationHandler
{
    private readonly IProductionAccommodationRepository _accommodationRepository;
    private readonly IProductionAccommodationZoneRepository _zoneRepository;
    private readonly IMapper _mapper;

    public CreateProductionAccommodationHandler(IProductionAccommodationRepository accommodationRepository, IProductionAccommodationZoneRepository zoneRepository, IMapper mapper)
    {
        _accommodationRepository = accommodationRepository;
        _zoneRepository = zoneRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationDto> HandleAsync(CreateProductionAccommodationCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationZone? zone = await _zoneRepository.GetByIdAsync(command.ProductionAccommodationZoneId, cancellationToken);

        if (zone is null)
        {
            throw new NotFoundException($"Production accommodation zone with id '{command.ProductionAccommodationZoneId}' was not found.");
        }

        ProductionAccommodation accommodation = _mapper.Map<ProductionAccommodation>(command);

        await _accommodationRepository.AddAsync(accommodation, cancellationToken);
        await _accommodationRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductionAccommodationDto>(accommodation);
    }
}