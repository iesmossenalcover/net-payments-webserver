using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(
                configuration.GetConnectionString("Sqlite"))
            );

            // services.AddDbContext<ApplicationDbContext>(options =>
            //     options.UseSqlite(configuration.GetConnectionString("Sqlite"),
            //     b => b.MigrationsAssembly("WebApp")) // if want some project in a solution
            // );

            services.AddScoped<ApplicationDbContext, ApplicationDbContext>();

            // Better approach
            services.AddScoped<Application.Common.Services.IAuthenticationService, Infrastructure.AuthenticationService>();
            services.AddSingleton<Application.Common.Services.ICsvParser, Infrastructure.CsvParser>();

            return services;
        }
    }
}