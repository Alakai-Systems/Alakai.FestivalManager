namespace Alakai.FestivalManager.Infrastructure.Extensions;

public static class InfrastructureDependencyInjectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FestivalManagerDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IFestivalRepository, FestivalRepository>();
        services.AddScoped<IEditionRepository, EditionRepository>();
        services.AddScoped<IPassTypeRepository, PassTypeRepository>();
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
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserPanelRepository, UserPanelRepository>();
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddScoped<IEmailSender, MailKitEmailSender>();

        return services;
    }
}
