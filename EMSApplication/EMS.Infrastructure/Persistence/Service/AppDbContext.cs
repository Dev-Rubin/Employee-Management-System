using EMS.Domain.Entities;
using EMS.Domain.Entities.Common;
using EMS.Infrastructure.Persistence.Interface;
using Microsoft.EntityFrameworkCore;
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
                //Auto Timestamp
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

            //builder.IgnoreAny(typeof(UnitMoney));

            base.ConfigureConventions(builder);

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public IDbConnection Connection => Database.GetDbConnection();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    }
}
