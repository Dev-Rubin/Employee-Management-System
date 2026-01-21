using EMS.Domain.Entities.Common;

namespace EMS.Infrastructure.Persistence.Interface
{
    public interface IRepository
    {
        void Save<T>(T obj)
            where T : class;
        void SaveUpdate<T>(T obj)
           where T : BaseEntity<int>;
        void Update<T>(T obj)
            where T : class;
        void Delete<T>(T obj)
            where T : class;
    }
}
