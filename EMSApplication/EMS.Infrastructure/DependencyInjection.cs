using EMS.Infrastructure.Persistence;
using EMS.Infrastructure.Persistence.Interface;
using EMS.Infrastructure.Persistence.Service;
using EMS.Infrastructure.Persistence.UserContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace EMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<AppDbContext>(options => 
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)),
                ServiceLifetime.Scoped
            );
            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            services.AddScoped<IAppReadDbConnection, AppReadDbConnection>();
            services.AddScoped<IAppWriteDbConnection, AppWriteDbConnection>();

            services.AddScoped<IUserContext, UserContext>();

            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IQueries, Queries>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }

}
