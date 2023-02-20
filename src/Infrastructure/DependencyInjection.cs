using Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(
            //     configuration.GetConnectionString("Sqlite"))
            // );

            services.AddDbContextPool<AppDbContext>(o => 
                o.UseNpgsql(configuration.GetConnectionString("PostgreSql"))
            );

            services.AddScoped<AppDbContext, AppDbContext>();

            // Better approach
            services.AddScoped<Application.Common.Services.IUsersRepository, Repos.UserRepository>();
            services.AddScoped<Application.Common.Services.ICoursesRepository, Repos.CoursesRepository>();
            services.AddScoped<Application.Common.Services.IPeopleRepository, Repos.PeopleRepository>();
            services.AddScoped<Application.Common.Services.IPersonGroupCourseRepository, Repos.PeopleGroupCourseRepository>();
            services.AddScoped<Application.Common.Services.IStudentsRepository, Repos.StudentsRepository>();
            services.AddScoped<Application.Common.Services.IGroupsRepository, Repos.GroupsRepository>();
            services.AddScoped<Application.Common.Services.IEventsRespository, Repos.EventsRepository>();
            services.AddScoped<Application.Common.Services.IEventsPeopleRespository, EventsPeopleRepository>();
            services.AddScoped<Application.Common.Services.ITransactionsService , TransactionsService>();
            services.AddSingleton<Application.Common.Services.ICsvParser, CsvParser>();

            return services;
        }
    }
}