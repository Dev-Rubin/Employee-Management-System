using EMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired();

            builder.Property(x => x.TokenHash)
                   .HasMaxLength(64)
                   .IsRequired();

            builder.HasIndex(x => x.TokenHash)
                   .IsUnique();

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId);
        }
    }

}
