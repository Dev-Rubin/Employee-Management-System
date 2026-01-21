using EMS.Infrastructure.Utilities;

namespace EMS.Infrastructure.Common
{
    public static class Injector
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            if (ServiceProvider == null)
                ServiceProvider = serviceProvider;
        }

        public static bool IsReady
        {
            get { return ServiceProvider != null; }
        }

        public static T? Resolve<T>()
        {
            return ServiceProviderInjector.Resolve<T>();
        }
    }
}
