using EMS.Domain.Entities.Common;
using System.Linq.Expressions;

namespace EMS.Infrastructure.Persistence.Interface
{
    public interface IQueryBuilder<TEntity, in TId> : IQueryBuilderFetch<TEntity>
       where TEntity : BaseEntity<TId>
    {
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
        IQueryable<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector);
        IQueryable<TEntity> OrderByDesc<TKey>(Expression<Func<TEntity, TKey>> keySelector);
        IQueryable<TEntity> Build();
    }
}
