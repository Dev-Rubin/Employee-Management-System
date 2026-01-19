using EMS.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using EMS.Application;

namespace EMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static void RegisterHandler(this IServiceCollection services, IConfiguration configuration) 
        { 
            var assembly = Assembly.GetExecutingAssembly(); 
            Type handlerInterFaceOpen = typeof(MediatR.IRequestHandler<,>); 
            IEnumerable<Assembly> assembliesWithHandlers;
            try 
            {
                assembliesWithHandlers = assembly.DefinedTypes.Where(t => t.ImplementedInterfaces.Any(i => i.IsGenericType 
                    && i.GetGenericTypeDefinition() == handlerInterFaceOpen))
                    .Select(t => t.Assembly).
                    Distinct(
                    ).ToArray(); 
            }
            catch (ReflectionTypeLoadException ex)
            { 
                assembliesWithHandlers = ex.Types?.Where(t => t != null && t.GetInterfaces().Any(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == handlerInterFaceOpen)).Select(t => t.Assembly)
                    .Distinct()
                    .ToArray() ?? Array.Empty<Assembly>(); 
            } 
            if (!assembliesWithHandlers.Any())
            {
                assembliesWithHandlers = new[] { assembly };
            } 
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembliesWithHandlers.ToArray())); 
        }
    }

}
