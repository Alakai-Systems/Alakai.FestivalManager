namespace Alakai.FestivalManager.Application.Features.Reports.Services;

public interface IReportService
{
    Task<byte[]> GenerateUsersReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateRegistrationsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCompetitionsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateAccommodationReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateAccommodationGridReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateBusesReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateMealsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
}