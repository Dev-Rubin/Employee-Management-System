using EMS.Infrastructure.Persistence.Interface;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace EMS.Infrastructure.Common
{
    public static class BaseLibraryExtensions
    {
        public static bool IsNullableType(this Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        [DebuggerNonUserCode]
        public static IList<T> Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null) return null;
            var list = enumerable.ToList();
            foreach (var item in list)
            {
                action(item);
            }

            return list;
        }

        public static string ToConcatenatedString<T>(this IEnumerable<T> instance, char separator)
        {
            if (instance != null)
            {
                if (instance.IsEmpty()) return string.Empty;

                var csv = new StringBuilder();
                instance.Each(value => csv.AppendFormat("{0}{1}", value, separator));
                return csv.ToString(0, csv.Length - 1).Trim();
            }

            return null;
        }

        public static string ToConcatenatedString<T>(this IEnumerable<T> instance, string separator)
        {
            if (instance != null && instance.Any())
            {
                var csv = new StringBuilder();
                instance.Each(value => csv.AppendFormat("{0}{1}", value, separator));
                return csv.ToString(0, csv.Length - separator.Length).Trim();
            }

            return null;
        }

        public static string ToCsv<T>(this IEnumerable<T> instance)
        {
            if (instance != null)
            {
                var csv = new StringBuilder();
                instance.Each(v => csv.AppendFormat("{0}, ", v));
                if (csv.Length > 0)
                    return csv.ToString(0, csv.Length - 2);
                return string.Empty;
            }

            return null;
        }

        [DebuggerNonUserCode]
        public static string GetClassDescription(this Type type, bool getsFromResourceManager = true)
        {
            var attributes =
                TypeDescriptor.GetAttributes(type);

            var descriptionAttribute =
                (DescriptionAttribute)attributes[typeof(DescriptionAttribute)];

            string name;

            if (descriptionAttribute == null ||
                string.IsNullOrWhiteSpace(descriptionAttribute.Description))
            {
                name = type.Name;
            }
            else
            {
                name = descriptionAttribute.Description;
            }

            var services = new ServiceCollection();

            var serviceProvider = services.BuildServiceProvider();

            IResourceManager resourceManagerService = (IResourceManager)serviceProvider.GetService(typeof(IResourceManager));

            if (resourceManagerService != null && getsFromResourceManager)
            {
                return resourceManagerService.GetLocalResourceString("ClassName", name);
            }

            return name;
        }
    }
}
