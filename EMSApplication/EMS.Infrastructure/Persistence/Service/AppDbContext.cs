using EMS.Domain.Entities;
using EMS.Domain.Entities.Common;
using EMS.Infrastructure.Persistence.Configurations;
using EMS.Infrastructure.Persistence.Interface;
using EMS.Infrastructure.Persistence.UserContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace EMS.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        private readonly IUserContext _userContext;
        public AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _userContext = serviceProvider.GetService<IUserContext>()!; 
        }

        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public Task<int> SaveChangesAsync<TIdentity>(CancellationToken cancellationToken = default(CancellationToken)) where TIdentity : class
        {
            ApplyAuditInfo();
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
        private void ApplyAuditInfo()
        {
            var userId = _userContext.UserId;

            foreach (var entry in ChangeTracker.Entries<BaseEntity<int>>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.AddedDate = DateTime.UtcNow;
                    entry.Entity.AddedByUserId = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
                    entry.Entity.UpdatedByUserId = userId;
                }
            }
        }

        public IDbConnection Connection => Database.GetDbConnection();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<ExceptionLog> ExceptionLogs => Set<ExceptionLog>();

    }
}
