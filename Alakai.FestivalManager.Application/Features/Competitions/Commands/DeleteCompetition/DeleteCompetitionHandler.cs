using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

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

        _competitionRepository.Delete(competition);
        await _competitionRepository.SaveChangesAsync(cancellationToken);
    }
}
