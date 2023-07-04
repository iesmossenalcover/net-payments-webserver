using Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextPool<AppDbContext>(o =>
                o.UseNpgsql(configuration.GetValue<string>("PostgreSqlConnectionString"))
            );

            services.AddScoped<AppDbContext, AppDbContext>();

            // Domain
            services.AddScoped<Domain.Behaviours.EventPersonProcessingService, Domain.Behaviours.EventPersonProcessingService>();

            // Infrastructure
            services.AddScoped<Domain.Services.IUsersRepository, Repos.UserRepository>();
            services.AddScoped<Domain.Services.IOAuthUsersRepository, Repos.OAuthUserRepository>();
            services.AddScoped<Domain.Services.ICoursesRepository, Repos.CoursesRepository>();
            services.AddScoped<Domain.Services.IPeopleRepository, Repos.PeopleRepository>();
            services.AddScoped<Domain.Services.IPersonGroupCourseRepository, Repos.PeopleGroupCourseRepository>();
            services.AddScoped<Domain.Services.IGroupsRepository, Repos.GroupsRepository>();
            services.AddScoped<Domain.Services.IEventsRespository, Repos.EventsRepository>();
            services.AddScoped<Domain.Services.IEventsPeopleRespository, EventsPeopleRepository>();
            services.AddScoped<Domain.Services.IOrdersRepository, OrdersRepository>();
            services.AddScoped<Domain.Services.IOUGroupRelationsRepository, UoGroupRelationRepository>();
            services.AddScoped<Domain.Services.ITransactionsService, TransactionsService>();
            services.AddSingleton<Domain.Services.ICsvParser, CsvParser>();
            services.AddSingleton<Domain.Services.IGoogleAdminApi, GoogleAdminApi>();
            services.AddSingleton<Domain.Services.IOAuthRepository, OAuthRepository>();
            services.AddSingleton<Domain.Services.IRedsys, Redsys.RedsysApi>();
            services.AddScoped<Domain.Services.IAppConfigRepository, AppConfigRepository>();

            return services;
        }
    }
}