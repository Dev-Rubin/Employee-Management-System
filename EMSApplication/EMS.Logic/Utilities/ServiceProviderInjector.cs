using Microsoft.Extensions.DependencyInjection;

namespace EMS.Logic.Utilities
{
    public static class ServiceProviderInjector
    {
        private static IServiceProvider? _serviceProvider;

        public static void RegisterServiceScoped<TService, TImplementation>(IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            serviceCollection.AddScoped<TService, TImplementation>();
        }
        public static void RegisterServiceTransient<TService, TImplementation>(IServiceCollection serviceCollection)
             where TService : class
             where TImplementation : class, TService
        {
            serviceCollection.AddTransient<TService, TImplementation>();
        }
        public static void RegisterServiceScopedType(IServiceCollection serviceCollection, Type service, Type implementation)
        {
            serviceCollection.AddScoped(service, implementation);
        }
        public static void RegisterServiceTransientType(IServiceCollection serviceCollection, Type service, Type implementation)
        {
            serviceCollection.AddTransient(service, implementation);
        }
        public static void BuildServiceProvider(IServiceCollection serviceCollection)
        {
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public static TService Resolve<TService>()
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service Provider has not been built yet. Call BuildServiceProvider() before resolving services.");
            }

            return _serviceProvider.GetService<TService>() ?? throw new InvalidOperationException($"Service of type {typeof(TService)} not registered.");
        }

        public interface IAsyncInitializable
        {
            Task InitializeAsync();
        }

        public static async Task<TService> ResolveAsync<TService>()
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service Provider has not been built yet. Call BuildServiceProvider() before resolving services.");
            }

            var service = _serviceProvider.GetService<TService>() ?? throw new InvalidOperationException($"Service of type {typeof(TService)} not registered.");

            if (service is IAsyncInitializable asyncInitializable)
            {
                await asyncInitializable.InitializeAsync().ConfigureAwait(false);
            }

            return service;
        }
    }
}
