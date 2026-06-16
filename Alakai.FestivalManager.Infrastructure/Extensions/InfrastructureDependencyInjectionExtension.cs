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

        return services;
    }
}