using Alakai.FestivalManager.Application.Interfaces.Repositories;

namespace Alakai.FestivalManager.Application.Features.EmailLogs.Commands.DeleteEmailLog;

public class DeleteEmailLogHandler
{
    private readonly IEmailLogRepository _emailLogRepository;

    public DeleteEmailLogHandler(IEmailLogRepository emailLogRepository)
    {
        _emailLogRepository = emailLogRepository;
    }

    public async Task<Guid> HandleAsync(DeleteEmailLogCommand command, CancellationToken cancellationToken = default)
    {
        EmailLog? emailLog = await _emailLogRepository.GetByIdAsync(command.Id, cancellationToken);

        if (emailLog is null) 
        { 
            throw new NotFoundException($"Email log with id '{command.Id}' was not found."); 
        }


        _emailLogRepository.Delete(emailLog);

        await _emailLogRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
