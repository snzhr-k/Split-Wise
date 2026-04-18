using FairSplit.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairSplit.Api.Infrastructure.Persistence.Configurations;

public sealed class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.ToTable("Balances");

        builder.HasKey(balance => balance.Id);

        builder.Property(balance => balance.NetAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(balance => balance.GroupId);
        builder.HasIndex(balance => balance.MemberId);

        builder.HasIndex(balance => new { balance.GroupId, balance.MemberId })
            .IsUnique();

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(balance => balance.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(balance => balance.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
