namespace Alakai.FestivalManager.Application.Features.Reports.Services;

public interface IReportService
{
    Task<byte[]> GenerateUsersReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateRegistrationsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCheckInReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCompetitionsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateAccommodationReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateAccommodationGridReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateBusesReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateMealsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionTeamReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionSuppliersReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionTripsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionItinerariesReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionAccommodationReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateProductionAccommodationGridReportAsync(Guid editionId, CancellationToken cancellationToken = default);

    Task<byte[]> GenerateInvoicesReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateFinancialSummaryReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateEarlyBirdReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateRefundsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateFinancePaymentsReportAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateOutstandingBalancesReportAsync(Guid editionId, CancellationToken cancellationToken = default);
}