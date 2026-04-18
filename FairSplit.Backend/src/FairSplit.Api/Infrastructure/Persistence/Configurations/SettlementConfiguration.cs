using FairSplit.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairSplit.Api.Infrastructure.Persistence.Configurations;

public sealed class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> builder)
    {
        builder.ToTable("Settlements");

        builder.HasKey(settlement => settlement.Id);

        builder.Property(settlement => settlement.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(settlement => settlement.SettledAtUtc)
            .IsRequired();

        builder.HasIndex(settlement => settlement.GroupId);
        builder.HasIndex(settlement => settlement.FromMemberId);
        builder.HasIndex(settlement => settlement.ToMemberId);
        builder.HasIndex(settlement => settlement.SettledAtUtc);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(settlement => settlement.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(settlement => settlement.FromMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(settlement => settlement.ToMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
