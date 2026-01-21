using EMS.Infrastructure.Persistence;
using EMS.Infrastructure.Persistence.Interface;
using EMS.Infrastructure.Persistence.Service;
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
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)),
                ServiceLifetime.Scoped);

            services.AddScoped(typeof(IAppDbContext), typeof(AppDbContext));
            services.AddScoped(typeof(IAppWriteDbConnection), typeof(AppWriteDbConnection));
            services.AddScoped(typeof(IAppReadDbConnection), typeof(AppReadDbConnection));
            services.AddScoped(typeof(IRepository), typeof(Repository));
            services.AddScoped(typeof(IQueries), typeof(Queries));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
        }
    }

}
