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

            services.AddDbContextPool<Infrastructure.AppDbContext>(o => 
                o.UseNpgsql(configuration.GetConnectionString("PostgreSql"))
            );

            services.AddScoped<Infrastructure.AppDbContext, Infrastructure.AppDbContext>();

            // Better approach
            services.AddScoped<Application.Common.Services.IUsersRepository, Infrastructure.Repos.UserRepository>();
            services.AddScoped<Application.Common.Services.ICoursesRepository, Infrastructure.Repos.CoursesRepository>();
            services.AddScoped<Application.Common.Services.IPeopleRepository, Infrastructure.Repos.PeopleRepository>();
            services.AddScoped<Application.Common.Services.IStudentsRepository, Infrastructure.Repos.StudentsRepository>();
            services.AddScoped<Application.Common.Services.IGroupsRepository, Infrastructure.Repos.GroupsRepository>();
            services.AddScoped<Application.Common.Services.ITransactionsService, Infrastructure.TransactionsService>();
            services.AddSingleton<Application.Common.Services.ICsvParser, Infrastructure.CsvParser>();

            return services;
        }
    }
}