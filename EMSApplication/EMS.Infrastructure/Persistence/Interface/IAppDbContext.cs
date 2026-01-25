using EMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace EMS.Infrastructure.Persistence.Interface
{
    public interface IAppDbContext : IDisposable
    {
        public IDbConnection Connection { get; }
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<ExceptionLog> ExceptionLogs => Set<ExceptionLog>();
        DatabaseFacade Database { get; }
        EntityEntry Entry(object entity);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        int SaveChanges();
    }
}
