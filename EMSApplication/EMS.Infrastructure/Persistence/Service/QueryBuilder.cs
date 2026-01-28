using EMS.Domain.Entities.Common;
using EMS.Infrastructure.Persistence.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EMS.Infrastructure.Persistence.Service
{
    public class QueryBuilder<TEntity, TId> : IQueryBuilder<TEntity, TId> where TEntity : BaseEntity<TId> where TId : struct, IEquatable<TId>
    {
        private IQueryable<TEntity> _query;
        private IAppDbContext _appDbContext;
        public QueryBuilder()
        {

        }

        public QueryBuilder(IUnitOfWork unitOfWork)
        {
            _query =
                unitOfWork
                .Set<TEntity>()
                .AsNoTracking()
                .AsQueryable();

            _appDbContext = unitOfWork.CurrentSession;

        }

        protected IQueryable Query
        {
            get { return _query; }
        }

        protected IAppDbContext Session
        {
            get { return _appDbContext; }
        }

        public IQueryable<TEntity> GetQuery()
        {
            return _query;
        }

        public IQueryable<TEntity> SetQuery(IQueryable<TEntity> qry)
        {
            _query = qry;
            return _query;
        }
        public IQueryable<TEntity> AsNoTrackEntity()
        {
            _query = _query.AsNoTracking();

            return _query;
        }

        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            _query = _query.Where(predicate);
            return _query;
        }
        public IQueryable<IGrouping<TKey, TEntity>> GroupBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            return _query.GroupBy(keySelector);
        }
        public IQueryable<TEntity> Include<TProperty>(
           Expression<Func<TEntity, IEnumerable<TProperty>>> navigationPropertyPath,
           Func<IQueryable<TProperty>, IQueryable<TProperty>> includeCondition = null)
        {
            _query = _query.Include(navigationPropertyPath);

            return _query;
        }
        public IQueryable<TEntity> Include<TProperty>(
           Expression<Func<TEntity, TProperty>> navigationPropertyPath,
           Func<IQueryable<TProperty>, IQueryable<TProperty>> includeCondition = null)
        {
            _query = _query.Include(navigationPropertyPath);

            return _query;
        }

        public IQueryable<TEntity> ThenInclude<TProperty, TNestedProperty>(
            Expression<Func<TEntity, IEnumerable<TProperty>>> navigationPropertyPath,
            Expression<Func<TProperty, TNestedProperty>> nestedNavigationPropertyPath)
        {
            var includeQuery = _query.Include(navigationPropertyPath);
            var thenIncludeQuery = includeQuery.ThenInclude(nestedNavigationPropertyPath);

            var flattenedQuery = thenIncludeQuery.Select(nestedProperty => nestedProperty);

            _query = flattenedQuery;
            return _query;
        }

        //public async Task<IReadOnlyList<TEntity>> ExecuteQryAsync(string qry)
        //{
        //    return await unit.QueryAsync<TEntity>(qry, CancellationToken.None).ConfigureAwait(false);
        //}
        //public async Task<TEntity> ExecuteFirstOrDefaultQryAsync(string qry)
        //{
        //    return await _appReadDb.QueryFirstOrDefaultAsync<TEntity>(qry, CancellationToken.None).ConfigureAwait(false);
        //}
        //public async Task<TEntity> ExecuteSingleQryAsync(string qry)
        //{
        //    return await _appReadDb.QuerySingleAsync<TEntity>(qry, CancellationToken.None).ConfigureAwait(false);
        //}
        public IQueryable<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            _query = _query.OrderBy(keySelector);
            return _query;
        }
        public IQueryable<TEntity> OrderByDesc<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        {
            _query = _query.OrderByDescending(keySelector);
            return _query;
        }

        public IQueryable<TEntity> Build()
        {
            return _query;
        }
        public virtual TEntity? Get(TId id)
        {
            return _query
                .Where(x => x.Id.Equals(id))
                .FirstOrDefault();
        }
        public virtual async Task<TEntity?> GetAsync(TId id)
        {
            return await
                _query.Where(x => x.Id.Equals(id))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        ///   Gets the whole result set
        /// </summary>
        /// <returns> </returns>
        public virtual IList<TEntity> GetAll()
        {
            var query = _query.ToList();
            return query;
        }
        public virtual IList<TEntity> GetAllWithTracking()
        {
            var query = _query.AsTracking().ToList();
            return query;
        }
        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            var query =
                await _query
                .ToListAsync()
                .ConfigureAwait(false);

            return query;
        }

        public virtual async Task<IList<TEntity>> GetAllWithTrackingAsync()
        {
            var query =
                await
                _query
                .AsTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return query;

        }

        /// <summary>
        ///   Gets the root table's distinct id
        /// </summary>
        /// <returns> </returns>
        public virtual IList<int> GetAllDistinctRootId()
        {
            var query =
                _query.Select(x => Convert.ToInt32(x.Id))
                .ToList();

            return query;
        }

        public virtual async Task<IList<int>> GetAllDistinctRootIdAsync()
        {
            var query =
                await
                _query
                .Select(x => Convert.ToInt32(x.Id))
                .ToListAsync()
                .ConfigureAwait(false);

            return query;
        }

        /// <summary>
        ///   Gets the first machineFamily out of the result set or its default.
        /// </summary>
        /// <returns> </returns>
        public virtual TEntity? GetFirstOrDefault()
        {
            try
            {
                var query =
               _query
               .Take(1)
               .SingleOrDefault();

                return query;
            }
            catch (Exception e)
            {
                var query =
               _query
               .Take(1)
               .SingleOrDefault();

                return query;
            }

        }
        public virtual async Task<TEntity?> GetFirstOrDefaultAsync()
        {
            var query =
                await
                _query
                .Take(1)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);

            return query;

        }

        public virtual TEntity? GetFirstOrDefaultWithTracking()
        {
            var query =
               _query
               .AsTracking()
               .Take(1)
               .SingleOrDefault();

            return query;

        }
        public virtual async Task<TEntity?> GetFirstOrDefaultWithTrackingAsync()
        {
            var query =
                await
                _query
                .AsTracking()
                .Take(1)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);

            return query;

        }


        /// <summary>
        ///   Gets the last machineFamily out of the resultset or its default.
        /// </summary>
        /// <returns> </returns>
        public virtual TEntity? GetLastOrDefault()
        {
            var query =
            _query
            .OrderByDescending(x => x.Id)
            .Take(1)
            .SingleOrDefault();

            return query;
        }
        public virtual async Task<TEntity?> GetLastOrDefaultAsync()
        {
            var query =
            await
            _query
            .OrderByDescending(x => x.Id)
            .Take(1)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);

            return query;
        }

        public virtual TEntity? GetLastOrDefaultWithTracking()
        {
            var query =
                _query
                .OrderByDescending(x => x.Id)
                .AsTracking()
                .Take(1)
                .SingleOrDefault();

            return query;

        }

        public virtual async Task<TEntity?> GetLastOrDefaultWithTrackingAsync()
        {
            var query =
                await
                _query
                .OrderByDescending(x => x.Id)
                .AsTracking()
                .Take(1)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);

            return query;

        }

        
        public List<TEntity> GetPageList(int pageSize, int pageIndex)
        {
            var query =
                _query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return query;
        }




        /// <summary>
        ///   Gets the whole result set
        /// </summary>
        /// <returns> </returns>
        public IEnumerable<TEntity> GetPageData(int pageSize, int pageIndex)
        {
            var query =
                _query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return query;
        }

        public async Task<IEnumerable<TEntity>> GetPageDataAsync(int pageSize, int pageIndex)
        {
            var query =
                await
                _query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);

            return query;
        }

        /// <summary>
        ///   Gets the row count for a query.
        /// </summary>
        /// <returns> </returns>
        public virtual int GetRowCount()
        {
            var query = _query.Count();
            return query;
        }
        public virtual async Task<int> GetRowCountAsync()
        {
            var query =
                await
                _query
                .CountAsync()
                .ConfigureAwait(false);

            return query;
        }

        /// <summary>
        ///   Gets a single machineFamily out of the result set
        /// </summary>
        /// <returns> </returns>
        public TEntity GetSingleOrDefault()
        {
            var query = _query.SingleOrDefault();
            return query;
        }
        public async Task<TEntity> GetSingleOrDefaultAsync()
        {
            var query = await _query.SingleOrDefaultAsync().ConfigureAwait(false);
            return query;
        }

        /// <summary>
        ///   Gets a limited size result set
        /// </summary>
        /// <param name="size"> The size. </param>
        /// <returns> </returns>
        public IList<TEntity> GetTop(int size)
        {
            var query =
                _query
                .Take(size)
                .ToList();

            return query;
        }
        public async Task<IList<TEntity>> GetTopAsync(int size)
        {
            var query =
                await
                _query
                .Take(size)
                .ToListAsync()
                .ConfigureAwait(false);

            return query;
        }


        /// <summary>
        ///   Filters out the object with specified id.
        /// </summary>
        /// <param name="id"> The id. </param>
        /// <returns> </returns>
        public IQueryable<TEntity> WhereIdIsNot(TId id)
        {
            _query =
                _query
                .Where(x => !x.Equals(id));

            return _query;
        }

        public IQueryable<TEntity> WhereIdsAreIn(IEnumerable<TId> ids)
        {
            _query =
                _query
                .Where(x => ids.Contains(x.Id));

            return _query;
        }

        public IQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> path, SortOrder order)
        {
            if (order == SortOrder.Ascending)
            {
                return OrderBy(path);
            }
            else
            {
                return OrderByDesc(path);
            }
        }

    }

}
