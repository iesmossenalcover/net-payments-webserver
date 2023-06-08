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

            // Better approach
            services.AddScoped<Application.Common.Services.IUsersRepository, Repos.UserRepository>();
            services.AddScoped<Application.Common.Services.IOAuthUsersRepository, Repos.OAuthUserRepository>();
            services.AddScoped<Application.Common.Services.ICoursesRepository, Repos.CoursesRepository>();
            services.AddScoped<Application.Common.Services.IPeopleRepository, Repos.PeopleRepository>();
            services.AddScoped<Application.Common.Services.IPersonGroupCourseRepository, Repos.PeopleGroupCourseRepository>();
            services.AddScoped<Application.Common.Services.IGroupsRepository, Repos.GroupsRepository>();
            services.AddScoped<Application.Common.Services.IEventsRespository, Repos.EventsRepository>();
            services.AddScoped<Application.Common.Services.IEventsPeopleRespository, EventsPeopleRepository>();
            services.AddScoped<Application.Common.Services.IOrdersRepository, OrdersRepository>();
            services.AddScoped<Application.Common.Services.ITransactionsService, TransactionsService>();
            services.AddScoped<Application.Common.Services.IRepository<Domain.Entities.Authentication.GoogleGroupClaimRelation>, Repository<Domain.Entities.Authentication.GoogleGroupClaimRelation>>();
            services.AddSingleton<Application.Common.Services.ICsvParser, CsvParser>();
            services.AddSingleton<Application.Common.Services.IGoogleAdminApi, GoogleAdminApi>();
            services.AddSingleton<Application.Common.Services.IOAuthRepository, OAuthRepository>();
            services.AddSingleton<Application.Common.Services.IRedsys, Redsys.RedsysApi>();
            services.AddScoped<Application.Common.Services.IAppConfigRepository, AppConfigRepository>();
            return services;
        }
    }
}