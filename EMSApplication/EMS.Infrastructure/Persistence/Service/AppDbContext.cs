using EMS.Domain.Entities;
using EMS.Domain.Entities.Common;
using EMS.Infrastructure.Persistence.Configurations;
using EMS.Infrastructure.Persistence.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace EMS.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext()
        {

        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public Task<int> SaveChangesAsync<TIdentity>(CancellationToken cancellationToken = default(CancellationToken))
            where TIdentity : class
        {
            foreach (var item in ChangeTracker.Entries<BaseEntity<TIdentity>>().AsEnumerable())
            {
                item.Entity.AddedDate = DateTime.Now;
                item.Entity.UpdatedDate = DateTime.Now;
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            base.ConfigureConventions(builder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }

        public IDbConnection Connection => Database.GetDbConnection();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<ExceptionLog> ExceptionLogs => Set<ExceptionLog>();

    }
}
