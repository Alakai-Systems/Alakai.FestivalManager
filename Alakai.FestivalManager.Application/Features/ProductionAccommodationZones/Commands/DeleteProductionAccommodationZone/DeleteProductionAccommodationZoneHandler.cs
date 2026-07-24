namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Commands.DeleteProductionAccommodationZone;

public class DeleteProductionAccommodationZoneHandler
{
    private readonly IProductionAccommodationZoneRepository _zoneRepository;

    public DeleteProductionAccommodationZoneHandler(IProductionAccommodationZoneRepository zoneRepository)
    {
        _zoneRepository = zoneRepository;
    }

    public async Task<Guid> HandleAsync(DeleteProductionAccommodationZoneCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationZone? zone = await _zoneRepository.GetByIdAsync(command.Id, cancellationToken);

        if (zone is null)
        {
            throw new NotFoundException($"Production accommodation zone with id '{command.Id}' was not found.");
        }

        _zoneRepository.Delete(zone);
        await _zoneRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}