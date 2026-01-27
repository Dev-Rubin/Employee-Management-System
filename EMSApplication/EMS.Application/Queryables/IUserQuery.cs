using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence.Interface;

namespace EMS.Application.Queryables
{
    public interface IUserQuery : IQueryBuilder<User, int>
    {
        IUserQuery WhereIdIs(int id);
    }
}
