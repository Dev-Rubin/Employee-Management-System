using EMS.Application.Queryables;
using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence.Interface;
using EMS.Infrastructure.Persistence.Service;

namespace EMS.Logic.Queryables
{
    public class UserQuery : QueryBuilder<User, int>, IUserQuery
    {
        public UserQuery(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IUserQuery WhereIdIs(int id)
        {
            Where(x => x.Id == id);
            return this;
        }
    }

}
