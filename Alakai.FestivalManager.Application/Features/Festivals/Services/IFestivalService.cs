namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public interface IFestivalService
{
    Task<FestivalDto> CreateAsync(CreateFestivalCommand command, CancellationToken cancellationToken = default);

    Task<FestivalDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
