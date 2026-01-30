using ApiTwitterUala.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTwitterUala.Domain.Configuration
{
    internal sealed class TweetConfiguration : IEntityTypeConfiguration<Tweet>
    {
        public void Configure(EntityTypeBuilder<Tweet> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Content)
                .IsRequired()
                .HasMaxLength(280);

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.CreatedAt);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
