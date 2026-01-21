using EMS.Infrastructure.Persistence.Interface;

namespace EMS.Infrastructure.Persistence.Service
{
    public class DefaultResourceManager : IResourceManager
    {
        public string GetLocalResourceString(string resourceSet, string name)
        {
            return name;
        }
    }
}
