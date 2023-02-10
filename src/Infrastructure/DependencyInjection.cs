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

            services.AddDbContext<ApplicationDbContext>(o => 
                o.UseNpgsql(configuration.GetConnectionString("PostgreSql"))
            );

            services.AddScoped<ApplicationDbContext, ApplicationDbContext>();

            // Better approach
            services.AddScoped<Application.Common.Services.IAuthenticationService, Infrastructure.AuthenticationService>();
            services.AddScoped<Application.Common.Services.IPeopleService, Infrastructure.PeopleService>();
            services.AddSingleton<Application.Common.Services.ICsvParser, Infrastructure.CsvParser>();

            return services;
        }
    }
}