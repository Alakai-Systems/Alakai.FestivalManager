using Alakai.FestivalManager.Infrastructure.BackgroundTasks;
using Azure.Storage.Blobs;

namespace Alakai.FestivalManager.Infrastructure.Extensions;

public static class InfrastructureDependencyInjectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FestivalManagerDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(60);
                }));

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();
        services.AddScoped<IFestivalRepository, FestivalRepository>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IFestivalCredentialsRepository, FestivalCredentialsRepository>();
        services.AddScoped<IEditionRepository, EditionRepository>();
        services.AddScoped<IPassTypeRepository, PassTypeRepository>();
        services.AddScoped<IProductionPersonRepository, ProductionPersonRepository>();
        services.AddScoped<IProductionSupplierRepository, ProductionSupplierRepository>();
        services.AddScoped<IProductionAccommodationBuildingRepository, ProductionAccommodationBuildingRepository>();
        services.AddScoped<IProductionAccommodationZoneRepository, ProductionAccommodationZoneRepository>();
        services.AddScoped<IProductionAccommodationRepository, ProductionAccommodationRepository>();
        services.AddScoped<IProductionReservationRepository, ProductionReservationRepository>();
        services.AddScoped<IProductionTripRepository, ProductionTripRepository>();
        services.AddScoped<IRunnerItineraryRepository, RunnerItineraryRepository>();
        services.AddScoped<ILevelRepository, LevelRepository>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();
        services.AddScoped<ICompetitionEntryRepository, CompetitionEntryRepository>();
        services.AddScoped<ICompetitionCapacityRepository, CompetitionCapacityRepository>();
        services.AddScoped<ICompetitionLevelRepository, CompetitionLevelRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IEmailLogRepository, EmailLogRepository>();
        services.AddScoped<IEmailLayoutRepository, EmailLayoutRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.Configure<GoogleAnalyticsOptions>(configuration.GetSection("GoogleAnalytics"));
        services.AddSingleton<IAnalyticsClient, GoogleAnalyticsClient>();
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection("AzureBlobStorage"));

        string azureBlobConnectionString = configuration["AzureBlobStorage:ConnString"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(azureBlobConnectionString))
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }
        else
        {
            services.AddSingleton(new BlobServiceClient(azureBlobConnectionString));
            services.AddScoped<IFileStorageService, BlobFileStorageService>();
        }
        services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserPanelRepository, UserPanelRepository>();
        services.AddScoped<IAccommodationBuildingRepository, AccommodationBuildingRepository>();
        services.AddScoped<IAccommodationZoneRepository, AccommodationZoneRepository>();
        services.AddScoped<IAccommodationRepository, AccommodationRepository>();
        services.AddScoped<IAccommodationReservationRepository, AccommodationReservationRepository>();
        services.AddScoped<IBusRepository, BusRepository>();
        services.AddScoped<IMealPreferenceRepository, MealPreferenceRepository>();
        services.AddScoped<IBusReservationRepository, BusReservationRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceSettingsRepository, InvoiceSettingsRepository>();
        services.AddScoped<IInvoiceTemplateRepository, InvoiceTemplateRepository>();
        services.AddScoped<IInvoicePdfService, QuestPdfInvoiceService>();
        services.AddScoped<ITicketPdfService, QuestPdfTicketService>();
        services.AddScoped<ITicketTokenService, HmacTicketTokenService>();
        services.Configure<ExternalAuthOptions>(configuration.GetSection("ExternalAuth"));
        services.AddSingleton<IExternalAuthService, ExternalAuthService>();
        services.Configure<RedsysOptions>(configuration.GetSection("Redsys"));
        services.AddSingleton<IRedsysGateway, RedsysGateway>();
        services.Configure<Alakai.FestivalManager.Infrastructure.Email.SystemEmailOptions>(configuration.GetSection("Email"));
        services.Configure<Alakai.FestivalManager.Infrastructure.Email.ApplicationUrlsOptions>(configuration.GetSection("ApplicationUrls"));
        services.Configure<Alakai.FestivalManager.Infrastructure.Email.ApplicationUrlsOptions>(configuration.GetSection("ApplicationUrls"));
        services.Configure<Alakai.FestivalManager.Infrastructure.Email.ApplicationUrlsOptions>(configuration.GetSection("ApplicationUrls"));
        services.Configure<Alakai.FestivalManager.Infrastructure.Email.ApplicationUrlsOptions>(configuration.GetSection("ApplicationUrls"));
        services.AddScoped<IEmailSender, MailKitEmailSender>();

        return services;
    }
}
