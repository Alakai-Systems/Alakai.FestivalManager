namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Commands.DeleteProductionAccommodation;

public class DeleteProductionAccommodationHandler
{
    private readonly IProductionAccommodationRepository _accommodationRepository;

    public DeleteProductionAccommodationHandler(IProductionAccommodationRepository accommodationRepository)
    {
        _accommodationRepository = accommodationRepository;
    }

    public async Task<Guid> HandleAsync(DeleteProductionAccommodationCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodation? accommodation = await _accommodationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (accommodation is null)
        {
            throw new NotFoundException($"Production accommodation with id '{command.Id}' was not found.");
        }

        _accommodationRepository.Delete(accommodation);
        await _accommodationRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}