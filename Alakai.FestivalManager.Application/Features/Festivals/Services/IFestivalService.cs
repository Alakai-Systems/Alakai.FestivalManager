namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public interface IFestivalService
{
    Task<FestivalDto> CreateAsync(
        CreateFestivalCommand command,
        CancellationToken cancellationToken = default);
}
