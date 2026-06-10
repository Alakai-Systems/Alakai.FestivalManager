namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.DeleteFestival;

public class DeleteFestivalHandler
{
    private readonly IFestivalRepository _festivalRepository;

    public DeleteFestivalHandler(IFestivalRepository festivalRepository)
    {
        _festivalRepository = festivalRepository;
    }

    public async Task<Guid> HandleAsync(DeleteFestivalCommand command, CancellationToken cancellationToken = default)
    {
        Festival? festival = await _festivalRepository.GetByIdAsync(command.Id, cancellationToken);

        if (festival is null)
        {
            throw new NotFoundException(
                $"Festival with id '{command.Id}' was not found.");
        }

        _festivalRepository.Delete(festival);

        await _festivalRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}