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

        return services;
    }
}