using EMS.Domain.Entities.Common;

namespace EMS.Infrastructure.Persistence.Interface
{
    public interface IBasicCrudService<in T, in TId> where T : BaseEntity<TId>
    {
        ITransactionResult Save(T entity);
        Task<ITransactionResult> SaveAsync(T entity);
        ITransactionResult Save(T entity, string successMessage, string failureMessage);
        Task<ITransactionResult> SaveAsync(T entity, string successMessage, string failureMessage);
        ITransactionResult Save(IEnumerable<T> entities);
        Task<ITransactionResult> SaveAsync(IEnumerable<T> entities);
        ITransactionResult Save(IEnumerable<T> entities, string successMessage, string failureMessage);
        Task<ITransactionResult> SaveAsync(IEnumerable<T> entities, string successMessage, string failureMessage);
        ITransactionResult Update(T entity);
        Task<ITransactionResult> UpdateAsync(T entity);
        ITransactionResult Update(T entity, string successMessage, string failureMessage);
        Task<ITransactionResult> UpdateAsync(T entity, string successMessage, string failureMessage);
        ITransactionResult Update(IEnumerable<T> entities);
        Task<ITransactionResult> UpdateAsync(IEnumerable<T> entities);
        ITransactionResult Update(IEnumerable<T> entities, string successMessage, string failureMessage);
        Task<ITransactionResult> UpdateAsync(IEnumerable<T> entities, string successMessage, string failureMessage);
        ITransactionResult SaveOrUpdate(T entity);
        Task<ITransactionResult> SaveOrUpdateAsync(T entity);
        ITransactionResult SaveOrUpdate(T entity, string successMessage, string failureMessage);
        Task<ITransactionResult> SaveOrUpdateAsync(T entity, string successMessage, string failureMessage);
        ITransactionResult SaveOrUpdate(IEnumerable<T> entities);
        Task<ITransactionResult> SaveOrUpdateAsync(IEnumerable<T> entities);
        ITransactionResult SaveOrUpdate(IEnumerable<T> entities, string successMessage, string failureMessage);
        Task<ITransactionResult> SaveOrUpdateAsync(IEnumerable<T> entities, string successMessage, string failureMessage);
        ITransactionResult Delete(T entity);
        Task<ITransactionResult> DeleteAsync(T entity);
        Task<ITransactionResult> DeleteAsync(IEnumerable<TId> ids);
        Task<ITransactionResult> DeleteAsync(IEnumerable<T> ids);
    }
}
