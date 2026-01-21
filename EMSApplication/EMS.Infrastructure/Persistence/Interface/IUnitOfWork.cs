using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace EMS.Infrastructure.Persistence.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IAppDbContext CurrentSession { get; }
        IAppReadDbConnection AppRead { get; }
        IAppWriteDbConnection AppWrite { get; }
        IMapper Mapper { get; }
        bool IsActiveTransaction();
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        ITransactionResult CommitTransaction();
        Task<ITransactionResult> CommitTransactionAsync(CancellationToken cancellationToken = default);
        ITransactionResult RollbackTransaction();
        Task<ITransactionResult> RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        EntityEntry Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}
