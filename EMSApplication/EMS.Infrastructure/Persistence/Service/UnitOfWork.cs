using AutoMapper;
using EMS.Infrastructure.Common;
using EMS.Infrastructure.Persistence.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace EMS.Infrastructure.Persistence.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IAppDbContext _appDbContext;
        private IDbContextTransaction? _dbContextTransaction;
        private readonly IAppReadDbConnection _appDbRead;
        private readonly IAppWriteDbConnection _appWriteDb;
        private readonly IMapper _mapper;

        private string _errorMessage = string.Empty;
        private bool _disposed;


        public UnitOfWork()
        {

        }

        public UnitOfWork(
            IAppDbContext appDbContext,
            IAppReadDbConnection appDbRead,
            IAppWriteDbConnection appWriteDb,
            IMapper mapper)
        {
            _appDbContext = appDbContext;
            _appDbRead = appDbRead;
            _appWriteDb = appWriteDb;
            _mapper = mapper;
        }

        public IAppDbContext CurrentSession
        {
            get { return _appDbContext; }
        }
        public IAppReadDbConnection AppRead
        {
            get { return _appDbRead; }
        }
        public IAppWriteDbConnection AppWrite
        {
            get { return _appWriteDb; }
        }

        public IMapper Mapper
        {
            get { return _mapper; }
        }

        public bool IsActiveTransaction()
        {
            if (_appDbContext.Database.CurrentTransaction == null)
                return false;

            return true;

        }

        public IDbContextTransaction? CurrentTransaction()
        {
            return _appDbContext.Database.CurrentTransaction;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDbContextTransaction BeginTransaction()
        {
            _dbContextTransaction = _appDbContext.Database.BeginTransaction();
            return _dbContextTransaction;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _dbContextTransaction = await _appDbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            return _dbContextTransaction;
        }

        public void Commit()
        {
            _dbContextTransaction?.Commit();
        }

        public ITransactionResult CommitTransaction()
        {
            try
            {
                if (_dbContextTransaction != null)
                {
                    _appDbContext.SaveChanges();
                    _dbContextTransaction?.Commit();
                }
                else
                {
                    return new FailedTransaction("");
                }

                return new SuccessfulTransaction("");
            }
            catch (Exception ex)
            {
                return new FailedTransaction(ex.Message);
            }
        }

        public async Task<ITransactionResult> CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_dbContextTransaction != null)
                {
                    await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await _dbContextTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return new FailedTransaction("");
                }
                return new SuccessfulTransaction("");
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains(" Truncated value") == true)
            {
                await _dbContextTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return new FailedTransaction("");

            }
            catch (Exception ex)
            {
                return new FailedTransaction(ex.Message, ex);
            }
        }

        public void RollBack()
        {
            _dbContextTransaction?.Rollback();
            _dbContextTransaction?.Dispose();
        }
        public ITransactionResult RollbackTransaction()
        {
            try
            {
                if (_dbContextTransaction != null)
                {
                    _dbContextTransaction.Rollback();
                }
                else
                {
                    return new FailedTransaction("");
                }

                return new FailedTransaction("");
            }
            catch (Exception ex)
            {
                return new FailedTransaction(ex.Message);
            }
        }
        public async Task<ITransactionResult> RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_dbContextTransaction != null)
                {
                    await _dbContextTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return new FailedTransaction("");
                }

                return new FailedTransaction("");
            }
            catch (Exception ex)
            {
                return new FailedTransaction(ex.Message);
            }
        }
        public void Save()
        {
            try
            {
                _appDbContext.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                HandleUnitOfWorkException(dbEx);
                throw new Exception(_errorMessage, dbEx);
            }
            catch (DBConcurrencyException dbEx)
            {
                throw new Exception(_errorMessage, dbEx);
            }
        }
        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateException dbEx)
            {
                HandleUnitOfWorkException(dbEx);
                throw new Exception(_errorMessage, dbEx);
            }
            catch (DBConcurrencyException dbEx)
            {
                throw new Exception(_errorMessage, dbEx);
            }
        }
        private void HandleUnitOfWorkException(DbUpdateException dbEx)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _appDbContext.Dispose();

            _disposed = true;
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _appDbContext.Set<TEntity>();
        }

        public EntityEntry Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return _appDbContext.Entry(entity);
        }
    }
}
