namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.DeleteCompetition;

public class DeleteCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;

    public DeleteCompetitionHandler(ICompetitionRepository competitionRepository)
    {
        _competitionRepository = competitionRepository;
    }

    public async Task HandleAsync(DeleteCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{command.Id}' was not found.");
        }

        competition.IsActive = false;

        _competitionRepository.Update(competition);
        await _competitionRepository.SaveChangesAsync(cancellationToken);
    }
}
