using Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;
using Alakai.FestivalManager.Application.Features.Users.Commands.DeleteUser;
using Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;
using Alakai.FestivalManager.Application.Features.Users.Queries.GetUserByEmail;
using Alakai.FestivalManager.Application.Features.Users.Queries.GetUserById;
using Alakai.FestivalManager.Application.Features.Users.Queries.GetUsers;
using Alakai.FestivalManager.Application.Features.Users.Services;

namespace Alakai.FestivalManager.Application.Extensions;

public static class ApplicationDependencyInjectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjectionExtension).Assembly);
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(ApplicationDependencyInjectionExtension).Assembly);
        });

        //Festivals
        services.AddScoped<CreateFestivalHandler>();
        services.AddScoped<IFestivalService, FestivalService>();
        services.AddScoped<GetFestivalByIdHandler>();
        services.AddScoped<GetFestivalsHandler>();
        services.AddScoped<UpdateFestivalHandler>();
        services.AddScoped<DeleteFestivalHandler>();

        //Editions
        services.AddScoped<CreateEditionHandler>();
        services.AddScoped<GetEditionByIdHandler>();
        services.AddScoped<GetEditionsByFestivalIdHandler>();
        services.AddScoped<GetEditionsHandler>();
        services.AddScoped<UpdateEditionHandler>();
        services.AddScoped<DeleteEditionHandler>();
        services.AddScoped<IEditionService, EditionService>();

        //PassTypes
        services.AddScoped<CreatePassTypeHandler>();
        services.AddScoped<GetPassTypeByIdHandler>();
        services.AddScoped<GetPassTypesHandler>();
        services.AddScoped<GetPassTypesByEditionIdHandler>();
        services.AddScoped<UpdatePassTypeHandler>();
        services.AddScoped<DeletePassTypeHandler>();
        services.AddScoped<IPassTypeService, PassTypeService>();

        //Levels 
        services.AddScoped<CreateLevelHandler>();
        services.AddScoped<GetLevelByIdHandler>();
        services.AddScoped<GetLevelsHandler>();
        services.AddScoped<GetLevelsByPassTypeIdHandler>();
        services.AddScoped<UpdateLevelHandler>();
        services.AddScoped<DeleteLevelHandler>();
        services.AddScoped<ILevelService, LevelService>();

        //Registrations
        services.AddScoped<CreateRegistrationHandler>();
        services.AddScoped<GetRegistrationByIdHandler>();
        services.AddScoped<GetRegistrationsHandler>();
        services.AddScoped<GetRegistrationsByEditionIdHandler>();
        services.AddScoped<UpdateRegistrationHandler>();
        services.AddScoped<DeleteRegistrationHandler>();
        services.AddScoped<IRegistrationService, RegistrationService>();

        // Users
        //Users
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserByEmailHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
