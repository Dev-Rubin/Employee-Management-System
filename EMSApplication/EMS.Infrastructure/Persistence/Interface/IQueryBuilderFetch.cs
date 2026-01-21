using System;
using System.Collections.Generic;
using System.Text;

namespace EMS.Infrastructure.Persistence.Interface
{
    public interface IQueryBuilderFetch<TEntity> : IQueryBuilderBase
    {
        /// <summary>
        /// Gets a single entity out of the result set
        /// </summary>
        /// <returns></returns>
        TEntity? GetSingleOrDefault();

        /// <summary>
        /// Gets the first entity out of the result set result or its default.
        /// </summary>
        /// <returns></returns>
        TEntity? GetFirstOrDefault();
        Task<TEntity?> GetFirstOrDefaultAsync();
        TEntity? GetFirstOrDefaultWithTracking();
        Task<TEntity?> GetFirstOrDefaultWithTrackingAsync();
        Task<TEntity?> GetLastOrDefaultWithTrackingAsync();
        /// <summary>
        /// Gets the last entity out of the resultset or its default.
        /// </summary>
        /// <returns></returns>
        TEntity? GetLastOrDefault();
        Task<TEntity?> GetLastOrDefaultAsync();
        Task<TEntity> GetSingleOrDefaultAsync();
        /// <summary>
        /// Limits the result set to a specific size.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        IList<TEntity> GetTop(int size);
        Task<IList<TEntity>?> GetTopAsync(int size);
        /// <summary>
        /// Gets the whole result set
        /// </summary>
        /// <returns></returns>
        IList<TEntity> GetAll();
        Task<IList<TEntity>> GetAllAsync();

        Task<IList<TEntity>> GetAllWithTrackingAsync();

        /// <summary>
        ///   Gets the root table's distinct id
        /// </summary>
        /// <returns> </returns>
        IList<int> GetAllDistinctRootId();
        Task<IList<int>> GetAllDistinctRootIdAsync();

        /// <summary>
        /// Gets the row count for the current query.
        /// </summary>
        /// <returns></returns>
        int GetRowCount();

        /// <summary>
        /// Gets the whole result set
        /// </summary>
        /// <returns></returns>
        List<TEntity> GetPageList(int pageSize, int pageIndex);
    }
}
