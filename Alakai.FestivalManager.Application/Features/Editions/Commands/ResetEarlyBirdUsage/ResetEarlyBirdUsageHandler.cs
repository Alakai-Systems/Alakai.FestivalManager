namespace Alakai.FestivalManager.Application.Features.Editions.Commands.ResetEarlyBirdUsage;

public class ResetEarlyBirdUsageHandler
{
    private readonly IEditionRepository _editionRepository;

    public ResetEarlyBirdUsageHandler(IEditionRepository editionRepository)
    {
        _editionRepository = editionRepository;
    }

    public async Task<int> HandleAsync(ResetEarlyBirdUsageCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        edition.EarlyBirdUsedCount = 0;
        edition.SetUpdated();

        await _editionRepository.SaveChangesAsync(cancellationToken);

        return edition.EarlyBirdUsedCount;
    }
}