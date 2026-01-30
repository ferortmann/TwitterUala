using ApiTwitterUala.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiTwitterUala.Domain.Configuration
{
    internal sealed class FollowConfiguration : IEntityTypeConfiguration<Follow>
    {
        public void Configure(EntityTypeBuilder<Follow> builder)
        {
            builder.HasKey(f => new { f.UserId, f.UserFollowerId });

            builder.Property(f => f.UserId)
                .IsRequired();

            builder.Property(f => f.UserFollowerId)
                .IsRequired();

            builder.HasIndex(f => f.UserId);
            builder.HasIndex(f => f.UserFollowerId);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.UserFollowerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}