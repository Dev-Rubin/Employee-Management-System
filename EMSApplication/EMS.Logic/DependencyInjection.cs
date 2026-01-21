using EMS.Infrastructure.Persistence.Interface;
using EMS.Logic.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EMS.Logic
{
    public static class DependencyInjection
    {
        public static void AddEmsLogic(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterRepository(services, configuration);
            RegisterQuery(services, configuration);
            //services.AddScoped(typeof(IControlNumberSettingProvider), typeof(ControlNumberSettingProvider));
            ServiceProviderInjector.BuildServiceProvider(services);

        }

        public static void RegisterRepository(this IServiceCollection services, IConfiguration configuration)
        {
            var repositoryTypes =
                Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract &&
                t.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IBasicCrudService<,>)))
                .ToList();

            // Register repositories with their corresponding interfaces
            foreach (var repositoryType in repositoryTypes)
            {
                var interfaceName = String.Concat("I", repositoryType.Name);
                var interfaceType = repositoryType.GetInterfaces().FirstOrDefault(x => x.Name == interfaceName);
                ServiceProviderInjector.RegisterServiceScopedType(services, interfaceType, repositoryType);
            }
        }
        public static void RegisterQuery(this IServiceCollection services, IConfiguration configuration)
        {
            var queryTypes =
                Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract &&
                t.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IQueryBuilder<,>)))
                .ToList();

            // Register queries with their corresponding interfaces
            foreach (var queryType in queryTypes)
            {
                var interfaceName = String.Concat("I", queryType.Name);
                var interfaceType = queryType.GetInterfaces().FirstOrDefault(x => x.Name == interfaceName);
                ServiceProviderInjector.RegisterServiceScopedType(services, interfaceType, queryType);
            }
        }
    }
}
