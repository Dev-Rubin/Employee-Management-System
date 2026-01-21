using EMS.Infrastructure.Persistence.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace EMS.Infrastructure.Persistence.Service
{
    public class Queries : IQueries
    {
        public Queries() { }

        private readonly IServiceProvider _serviceProvider;
        public Queries(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public T New<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
